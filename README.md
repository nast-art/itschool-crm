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

## Быстрый старт (полный контур в Docker)

\`\`\`bash
git clone &lt;repo&gt; && cd it-school-crm

# 1. Секреты: создайте .env в корне (образец — .env.example)
cp .env.example .env
#   впишите: DB_PASSWORD, KEYCLOAK_ADMIN_CLIENT_SECRET,
#   S3_ACCESS_KEY / S3_SECRET_KEY

# 2. Сборка и запуск всех сервисов (API, фронт, PostgreSQL, Keycloak, KeyDB)
docker compose up -d --build

# 3. UI: http://localhost:5173, Swagger: http://localhost:5026/swagger
\`\`\`

&gt; **Для разработки** удобнее поднимать только инфраструктуру
&gt; (`docker compose up -d postgres keycloak keydb`) и запускать
&gt; backend/frontend с хоста (шаги ниже) — горячая пересборка и отладка в IDE.

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
| `backend/ITSchoolCRM.API/appsettings.json` | Всё без секретов: TTL кэша, адреса, пустые заглушки ключей S3 |
| `backend/ITSchoolCRM.API/appsettings.Development.json` | Строки подключения для разработки (localhost) |
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
backend/ITSchoolCRM.API/   ASP.NET Core API (контроллеры, сервисы, Caching/, Storage/)
frontend/ITSchoolCRMWEB/   React SPA (страницы, api/, компоненты)
db/                        Скрипты PostgreSQL (crm-db.sql: схема + seed-данные)
keycloak/                  Экспорт realm (автоимпорт при старте контейнера)
loadtest/                  Скрипты нагрузочного тестирования k6
jstest.json                Коллекция Postman (ручные прогоны, демо кэша)
docker-compose.yml         PostgreSQL + Keycloak + KeyDB + API + фронт
.env.example               Образец файла секретов (скопировать в .env)
```

## Документация

- Swagger: `/swagger` (интерактивная проверка всех эндпоинтов, OAuth2 через
  Keycloak);
- встроенная документация: раздел «Документация» в боковом меню UI
  (руководство пользователя, руководство администратора, архитектура,
  коды ошибок);
- сопроводительная документация к защите: &lt;ссылка на pdf/docx&gt;.
