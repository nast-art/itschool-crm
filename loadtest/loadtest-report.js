// Нагрузочный тест NFR 7: 10 параллельных отчётов разной сложности.
// Механика токенов — идентична loadtest-users.js: каждый VU сам
// логинится и рефрешится, тест неограничен по времени.
import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: 10,
  iterations: 100,
  thresholds: {
    http_req_failed: ['rate<0.01'],
  },
};

const BASE = __ENV.BASE || 'http://localhost:5026';
const LOGIN = __ENV.LOGIN;
const PASSWORD = __ENV.PASSWORD;

let tokens = null;

function doLogin() {
  const r = http.post(
    `${BASE}/api/Auth/login`,
    JSON.stringify({ userName: LOGIN, password: PASSWORD }),
    { headers: { 'Content-Type': 'application/json' }, tags: { name: 'auth_login' } },
  );
  check(r, { 'login 200': (x) => x.status === 200 });
  tokens = r.json();
}

function doRefresh() {
  const r = http.post(
    `${BASE}/api/Auth/refresh`,
    JSON.stringify({ refreshToken: tokens.refreshToken }),
    { headers: { 'Content-Type': 'application/json' }, tags: { name: 'auth_refresh' } },
  );
  if (r.status === 200) {
    tokens = r.json();
  } else {
    doLogin();
  }
}

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

const REPORTS = [
  {
    name: 'easy_xlsx',
    endpoint: '/api/Reports/xlsx',
    body: {
      dateFrom: '2026-09-01', dateTo: null,
      universityIds: [], directionIds: [], productIds: [], managerIds: [],
      columns: ['university', 'status'],
    },
  },
  {
    name: 'medium_pdf',
    endpoint: '/api/Reports/pdf',
    body: {
      dateFrom: '2026-07-01', dateTo: null,
      universityIds: [], directionIds: [1], productIds: [], managerIds: [],
      columns: ['university', 'direction', 'status', 'manager', 'vendor'],
    },
  },
  {
    name: 'hard_xlsx_year',
    endpoint: '/api/Reports/xlsx',
    body: {
      dateFrom: '2026-01-01', dateTo: null,
      universityIds: [], directionIds: [], productIds: [], managerIds: [],
      columns: ['university', 'direction', 'product', 'status',
                'manager', 'vendor', 'contractNumber'],
    },
  },
];

export default function () {
  if (tokens === null) {
    doLogin();
  }

  const report = REPORTS[(__VU - 1) % REPORTS.length];

  const r = authed('POST', report.endpoint, report.body, report.name);

  check(r, {
    [`${report.name} 200`]: (x) => x.status === 200,
    [`${report.name} это файл`]: (x) => {
      const ct = x.headers['Content-Type'] || '';
      return ct.includes('spreadsheet') || ct.includes('pdf');
    },
  });
}