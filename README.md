# IT School CRM

## Быстрый старт
1. `git clone ... && cd it-school-crm`
2. `docker compose up -d` — поднимутся PostgreSQL (5432) и Keycloak (8080, admin/admin).
   Realm itschool импортируется автоматически.
3. База: создайте БД и накатите скрипты:
   psql -U postgres -h localhost -c "CREATE DATABASE it_school_crm_db;"
   psql -U postgres -h localhost -d it_school_crm_db -f itschooldb.sql
   (либо выполните скрипты через pgAdmin/DBeaver)
4. Backend: в backend/ пропишите строку подключения в
   appsettings.Development.json, затем:
   dotnet ef dbcontext scaffold "&lt;строка&gt;" Npgsql.EntityFrameworkCore.PostgreSQL -o Models --context-dir Data -c CrmDbContext --no-onconfiguring --use-database-names --no-build -f
   dotnet run
   API: https://localhost:7001, Swagger: /swagger
5. Frontend: в frontend/ создайте .env с VITE_API_URL=https://localhost:7001/api
   npm install && npm run dev

## Учётки Keycloak (демо)
- admin / admin (консоль Keycloak)
- пользователи CRM — заводятся через регистрацию в интерфейсе