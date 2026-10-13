# ProPhoneService

Сервис записи на ремонт телефонов: витрина, заказы на ремонт, личный кабинет клиента и панель мастера.

Модульный монорепозиторий:

```
.
├── client/   # фронтенд (Svelte + TypeScript, Vite) - пока только конфиг и линтер
├── server/   # бэкенд ASP.NET Core Web API + xUnit-тесты
├── docs/     # продуктовая документация, ERD
├── .github/  # CI (GitHub Actions)
├── docker-compose.yml  # PostgreSQL + API
└── .env.example        # шаблон переменных окружения
```

## Стек

| Слой     | Технологии |
|----------|------------|
| Фронт    | Svelte 5 + TypeScript, Vite, ESLint |
| Бэк      | ASP.NET Core (Web API), EF Core 10 + Npgsql, xUnit |
| БД       | PostgreSQL 17 (схема ниже, миграции EF Core) |
| CI       | GitHub Actions |

## Запуск

### Через Docker (PostgreSQL + API)

```bash
cp .env.example .env        # при необходимости поправить пароль
docker compose up --build   # БД на 5432, API на 8080
```

API применяет миграции при старте в режиме разработки (`Database.Migrate()`), Connection string передаётся через env `ConnectionStrings__Default`

### Backend без Docker

```bash
cd server
dotnet restore
dotnet build
dotnet test
dotnet run --project ProPhoneService.Api
```

Connection string для локального запуска - в `server/ProPhoneService.Api/appsettings.Development.json` (в `.gitignore`):

```bash
cp server/ProPhoneService.Api/appsettings.Development.json.example server/ProPhoneService.Api/appsettings.Development.json
```

Пароль в example совпадает с дефолтом из `.env.example`, так что с `docker compose up db` заработает сразу, БД при этом всё равно нужна

Health-check: `GET /health` → `Healthy`.

### Frontend

```bash
cd client
npm ci
npm run dev
```

Open `http://localhost:5173/`. Without `client/.env`, the frontend uses deterministic mock data.

To connect the API from `Darowiin/ProPhoneService`, branch `feat/backend-foundation`, copy `client/.env.example` to `client/.env` and start that backend on port 8080. Vite proxies `/api` to `VITE_API_PROXY_TARGET`, so local development does not require CORS.

```bash
cd client
copy .env.example .env   # Windows PowerShell; use cp on POSIX
npm run dev
```

The backend branch currently does not persist `ProblemDescription` and its order-list DTO omits client name, services, price, and a human-readable order number. The frontend sends `problemDescription` for the agreed backend contract and displays only fields the current API returns; the backend owner must add persistence before issue descriptions are stored. For production, host the static frontend and proxy `/api` to the backend on the same origin; the backend branch does not configure CORS.

```bash
npm run lint
npm run check
npm run build
npm run preview
```

## Схема БД

Полная ERD: [`docs/ErdDiagram.png`](docs/ErdDiagram.png)

| Таблица                | Назначение |
|------------------------|------------|
| `client`               | клиенты (uuid PK, name, phone, email, created_at) |
| `device`               | техника (type, manufacturer, model, serial_number) |
| `repair_order`         | заказ на ремонт (FK client_id, device_id; status, total_price, created_at) |
| `repair_status_history`| история смены статусов заказа (status, changed_at, comment) |
| `repair_order_service` | связка заказ↔услуга (FK repair_order_id, service_id; price) |
| `service`              | прайс-лист услуг (name, description, price, is_active) |
| `review`               | отзывы клиентов (FK client_id; rating, text, created_at) |

## Архитектура server

```
server/
├── ProPhoneService.Domain.Shared/   # enum-ы, машина состояний (без зависимостей)
├── ProPhoneService.Domain/          # сущности, интерфейсы IRepository
├── ProPhoneService.Infrastructure.EfCore/  # DbContext, конфигурации, миграции, реализации репозиториев
├── ProPhoneService.Api/             # эндпоинты, DI
└── ProPhoneService.Api.Tests/      # xUnit
```

Репозитории - generic `IRepository<T>` + специализированные, scoped lifetime, batch-сохранение через `SaveChangesAsync`

## Машина состояний заказа

Смена статуса - только через `RepairOrder.ChangeStatus()`, Карта переходов - `RepairStatusTransitions` в `Domain.Shared`.
Заказ создаётся фабрикой `RepairOrder.Create(...)` (или вручную через `Start()`): до инициализации `Status == null` и `ChangeStatus` выбрасывает исключение

```
Матрица переходов: Created → Diagnostics → Agreed → InProgress → Ready → Completed; отмена (Cancelled) из Created/Agreed/InProgress
```

Каждая смена пишет запись в `repair_status_history`, Запрещённый переход -> `InvalidOperationException`, конфликт параллельных изменений (токен конкурентности `xmin`) -> `DbUpdateConcurrencyException` (на уровне API оба будут 409)

## CI

`.github/workflows/ci.yml` - конфигурация CI/CD, запускается на каждый PR и push в `main`:

- **backend**: `dotnet restore` → `build` → `test`
- **client**: `npm ci` → `lint`
