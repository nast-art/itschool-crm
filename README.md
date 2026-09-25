# IT School CRM

CRM-система контроля взаимодействия ИТ Школы РТК с вузами по образовательным
программам: каталоги (вузы, направления, программы, продукты), workflow
взаимодействий (14 базовых этапов по ТЗ), комментарии, вложения, отчёты
(xls/xlsx/pdf/json), визуализация статистики, встроенная документация.

## Технологический стек

| Слой | Технология |
|---|---|
| Backend | ASP.NET Core (.NET 10), EF Core 10, Npgsql |
| Frontend | React 18 + Vite, React Router |
| База данных | PostgreSQL 16 |
| Кэш | KeyDB 6.3 (Redis-совместимый), cache-aside + версионированные ключи |
| Хранилище вложений | S3-совместимое (Yandex Object Storage); слой `IFileStorage` позволяет заменить на MinIO/диск одной строкой |
| Авторизация | Keycloak 24 (JWT, роли user / manager / admin) |
| Отчёты | NPOI (xls/xlsx), QuestPDF (pdf), System.Text.Json (json) |
| Нагрузочное тестирование | k6 (скрипты в `loadtest/`), коллекция Postman `jstest.json` для ручных прогонов |

## Быстрый старт (инфраструктура через Docker)

```bash
git clone &lt;repo&gt; && cd it-school-crm

# 1. Секреты: создайте .env в корне (образец — .env.example):
cp .env.example .env
#   впишите: пароль БД (DB_PASSWORD), secret Keycloak-админа
#   (KEYCLOAK_ADMIN_CLIENT_SECRET) и ключи S3 (S3_ACCESS_KEY / S3_SECRET_KEY)

# 2. Инфраструктура: PostgreSQL (5432), Keycloak (8080), KeyDB (6379)
docker compose up -d

# 3. База данных: создайте БД и накатите скрипты из db/
psql -U postgres -h localhost -c "CREATE DATABASE it_school_crm_db;"
psql -U postgres -h localhost -d it_school_crm_db -f db/01-schema.sql
psql -U postgres -h localhost -d it_school_crm_db -f db/02-seed.sql

# 4. Backend (локальная разработка — вне Docker)
cd backend
dotnet user-secrets set "Storage:S3:AccessKey" "&lt;ключ&gt;"
dotnet user-secrets set "Storage:S3:SecretKey" "&lt;секрет&gt;"
dotnet run
# API: http://localhost:5026, Swagger: /swagger

# 5. Frontend
cd ../frontend
echo "VITE_API_URL=http://localhost:5026/api" &gt; .env
npm install && npm run dev
# UI: http://localhost:5173
```

&gt; **Запуск всего контура в Docker** (API + фронт в контейнерах): в папках
&gt; `backend/` и `frontend/` должны лежать `Dockerfile` (многостадийные
&gt; сборки), тогда вместо шагов 4–5 достаточно `docker compose up -d --build`
&gt; — UI будет на `http://localhost:5173`. Для разработки удобнее запускать
&gt; backend/frontend с хоста, как описано выше: горячая пересборка,
&gt; отладка в IDE.

Приложение доступно по адресу фронтенда; авторизация — через форму
входа (Keycloak, за кадром).

## Учётные записи (демо-контур)

| Кто | Логин | Пароль | Роль |
|---|---|---|---|
| Администратор платформы | admin@example.com | 12345 | admin |
| Консоль Keycloak | admin | admin | — (управление realm) |

Пользователи с ролями user/manager заводятся через регистрацию в интерфейсе
(роль `user` назначается автоматически; manager/admin — через консоль Keycloak:
realm `itschool` → Users → Role mapping).

## Конфигурация

Приоритет настроек .NET (от высшего): переменные окружения → User Secrets →
`appsettings.{Environment}.json` → `appsettings.json`.

| Где | Что задаёт |
|---|---|
| `backend/appsettings.json` | Всё без секретов: TTL кэша, адреса, пустые заглушки ключей S3 |
| `backend/appsettings.Development.json` | Строки подключения для разработки (localhost) |
| User Secrets (`dotnet user-secrets`) | S3 AccessKey/SecretKey (локальная разработка) |
| `.env` + секция `environment:` в `docker-compose.yml` | Секреты и строки подключения для Docker-окружения |

Ключевые секции конфигурации:

- `ConnectionStrings:Keydb` — `keydb:6379,abortConnect=false` (в Docker) /
  `localhost:6379,abortConnect=false` (разработка);
- `Cache` — TTL каталогов (600 с), взаимодействий (300 с), статистики (60 с);
- `Storage:S3` — бакет, эндпоинт `https://storage.yandexcloud.net`, регион
  `ru-central1`, `ForcePathStyle: false`.

## Проверка работоспособности

```bash
# Кэш (KeyDB): ключи приложения и счётчики версий
docker exec -it itschoolcrm-keydb keydb-cli KEYS "itschoolcrm:*"
docker exec -it itschoolcrm-keydb keydb-cli GET itschoolcrm:interaction:version

# S3: загрузите файл через интерфейс (карточка → «Файлы»),
# объект появится в бакете по пути uploads/attachments/{interactionId}/{statusId}/

# Отказоустойчивость кэша: docker stop itschoolcrm-keydb — система продолжает
# работать (fallback в PostgreSQL, warning в логах), скорость снижается.
# docker start itschoolcrm-keydb — кэш прогревается автоматически.

# Ручной прогон эндпоинтов (Postman): импортировать jstest.json,
# Login → Run collection — видно попадания/промахи кэша по времени ответа.
```

## Нагрузочное тестирование (NFR 6, 7)

Скрипты k6 — в `loadtest/` (авторизация login + refresh внутри теста,
работает при любом времени жизни токена):

```bash
cd loadtest
k6 run -e LOGIN=admin@example.com -e PASSWORD=12345 loadtest-users.js    # 50 VU, 5 мин
k6 run -e LOGIN=admin@example.com -e PASSWORD=12345 loadtest-reports.js  # 10 VU, отчёты 3 уровней сложности
```

Подтверждённые результаты (демо-контур): 50 VU × 5 мин ≈ 43 000 запросов,
0% ошибок, p95 = 51 мс (норматив ≤ 1 с); 10 параллельных отчётов разной
сложности — 0% ошибок. Отключение KeyDB увеличивает отклик — система при
этом продолжает работать (fallback), эффект кэширования подтверждён.

## Структура репозитория

```
backend/            ASP.NET Core API (контроллеры, сервисы, Caching/, Storage/)
frontend/           React SPA (страницы, api/, компоненты)
db/                 Скрипты PostgreSQL: схема и seed-данные
keycloak/           Экспорт realm (автоимпорт при старте контейнера)
loadtest/           Скрипты нагрузочного тестирования k6
jstest.json         Коллекция Postman (ручные прогоны, демо кэша)
docker-compose.yml  PostgreSQL + Keycloak + KeyDB + API + фронт
.env.example        Образец файла секретов (скопировать в .env)
```

## Документация

- Swagger: `/swagger` (интерактивная проверка всех эндпоинтов, OAuth2 через
  Keycloak);
- встроенная документация: раздел «Документация» в боковом меню UI
  (руководство пользователя, руководство администратора, архитектура,
  коды ошибок);
- сопроводительная документация к защите: &lt;ссылка на pdf/docx&gt;.