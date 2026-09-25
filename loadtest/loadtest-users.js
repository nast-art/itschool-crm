// Нагрузочный тест NFR 6: 50 параллельных пользователей.
// Токены: каждый VU сам логинится по логину/паролю и обновляет
// access-токен через /api/Auth/refresh при 401. Тест можно гонять
// часами и с любым числом VU — протухание токена больше не влияет.
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 50,
  duration: '5m',
  thresholds: {
    http_req_duration: ['p(95)<1000'],   // NFR 1
    http_req_failed: ['rate<0.01'],      // NFR 6: ошибок < 1%
  },
};

const BASE = __ENV.BASE || 'http://localhost:5026';

// Учётные данные тестового пользователя — через переменные окружения:
//   k6 run -e LOGIN=admin@example.com -e PASSWORD=password loadtest-users.js
const LOGIN = __ENV.LOGIN;
const PASSWORD = __ENV.PASSWORD;

// Пара токенов ВУ-шки (k6: у каждого VU свой экземпляр замыкания)
let tokens = null;

// Логин → пара токенов. Не считаем эти запросы в метрики нагрузки
// (tags name=auth, их доля ничтожна, но на графиках не смешиваем).
function doLogin() {
  const r = http.post(
    `${BASE}/api/Auth/login`,
    JSON.stringify({ userName: LOGIN, password: PASSWORD }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'auth_login' },
    },
  );
  check(r, { 'login 200': (x) => x.status === 200 });
  tokens = r.json();
}

// Обновление пары через refresh-токен
function doRefresh() {
  const r = http.post(
    `${BASE}/api/Auth/refresh`,
    JSON.stringify({ refreshToken: tokens.refreshToken }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'auth_refresh' },
    },
  );
  if (r.status === 200) {
    tokens = r.json();
    return true;
  }
  // Рефреш протух (сессия умерла) — логинимся заново
  doLogin();
  return true;
}

// Обертка: выполнить запрос с актуальным токеном;
// при 401 — рефрешимся и повторяем ОДИН раз.
function authed(method, path, body, tags) {
  const headers = { Authorization: `Bearer ${tokens.accessToken}` };
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  let r = http.request(method, `${BASE}${path}`, body ? JSON.stringify(body) : null,
    { headers, tags: { name: tags } });

  if (r.status === 401) {
    doRefresh();
    headers.Authorization = `Bearer ${tokens.accessToken}`;
    r = http.request(method, `${BASE}${path}`, body ? JSON.stringify(body) : null,
      { headers, tags: { name: tags } });
  }

  return r;
}

// Первичный логин ВУ перед стартом его итераций
export function setup() {
  // глобальный setup оставляем пустым: логин делаем в init VU ниже
}

export default function () {
  // Ленивый первый логин этого конкретного VU (один раз на весь прогон)
  if (tokens === null) {
    doLogin();
  }

  const r1 = authed('GET', '/api/Interactions', undefined, 'interactions');
  check(r1, { 'interactions 2xx': (x) => x.status === 200 });

  const r2 = authed('GET', '/api/Universities', undefined, 'universities');
  check(r2, { 'universities 2xx': (x) => x.status === 200 });

  const r3 = authed('POST', '/api/Reports/statistics', {
    dateFrom: null, dateTo: null,
    universityIds: [], directionIds: [], productIds: [], managerIds: [],
    columns: ['university', 'status'],
  }, 'statistics');
  check(r3, { 'statistics 2xx': (x) => x.status === 200 });

  sleep(1);
}