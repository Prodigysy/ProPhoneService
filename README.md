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

### Полный стек в Docker (PostgreSQL + API + фронт)

Требуется установленный Docker Desktop (на Windows — с бэкендом WSL 2). Проверка: `docker version` и `docker compose version`.

Фронтенд и рабочий API сейчас лежат на разных ветках, поэтому сначала объедините их в одну:

```bash
git checkout feature/frontend-core
git fetch https://github.com/Darowiin/ProPhoneService.git feat/backend-foundation
git merge FETCH_HEAD        # backend: контроллеры, Application-слои, свой Dockerfile
```

Затем поднимите всё:

```bash
cp .env.example .env        # при необходимости поправить пароль
docker compose up --build   # db 5432, api 8080, web 8081
```

| Адрес | Что это |
|---|---|
| http://localhost:8081 | фронтенд (nginx, статика из `client/build`) |
| http://localhost:8081/api/services | тот же origin, nginx проксирует в `api:8080` |
| http://localhost:8080/health | health-check API |
| localhost:5432 | PostgreSQL (`prophoneservice`) |

Как это устроено:

- `client/Dockerfile` — multi-stage: `node:22-alpine` собирает статику (`npm run build` → `build/`), `nginx:1.27-alpine` её раздаёт. `VITE_API_URL=/api` передаётся build-arg, потому что Vite подставляет `VITE_*` на этапе сборки.
- `client/nginx.conf` — SPA-фолбэк `try_files $uri $uri/ /index.html` (adapter-static кладёт fallback в `index.html`) и `location /api/ → http://api:8080`. CORS не нужен: браузер ходит на тот же origin.
- `docker-compose.yml` — сервис `web` собирается из `./client` и зависит от `api`.
- API в режиме `Development` сам применяет миграции и создаёт БД при старте, поэтому прайс-лист засеивается автоматически.

Полезные команды:

```bash
docker compose logs -f web      # логи nginx
docker compose logs -f api      # логи API + миграции
docker compose up --build web   # пересобрать только фронт после правок
docker compose down             # остановить
docker compose down -v          # остановить и стереть данные PostgreSQL
```

### Только БД и API в Docker, фронт локально

Быстрый путь, если фронт удобнее править с HMR:

```bash
cp .env.example .env
docker compose up --build db api    # API на 8080
```

```bash
cd client && npm ci && cp .env.example .env && npm run dev   # 5173, прокси /api → 8080
```

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

The backend branch currently exposes no client, service or price data in its order list DTO, so the master panel shows only the fields it returns and renders placeholders for monthly revenue and clients. For production, host the static frontend and proxy `/api` to the backend on the same origin; the backend branch does not configure CORS.

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
| `repair_order`         | заказ на ремонт (FK client_id, device_id; status, total_price, preferred_visit_at, created_at) |
| `repair_status_history`| история смены статусов заказа (status, changed_at, comment) |
| `repair_order_service` | связка заказ↔услуга (FK repair_order_id, service_id; price) |
| `service`              | прайс-лист услуг (name, description, price, is_active) |
| `review`               | отзывы клиентов (FK client_id; rating, text, created_at) |

## Архитектура server

```
server/
├── ProPhoneService.Domain.Shared/        # enum-ы, машина состояний, WorkshopOptions (без зависимостей)
├── ProPhoneService.Domain/               # сущности, интерфейсы IRepository, доменные исключения, VisitTimeValidator
├── ProPhoneService.Infrastructure.EfCore/ # DbContext, конфигурации, миграции, реализации репозиториев, аудит
├── ProPhoneService.Application.Contracts/ # DTO-контракты
├── ProPhoneService.Application/          # сервисы (просмотр заявок, создание заявки, изменение статуса заявки, прайс-лист), маппинг Mapster
├── ProPhoneService.Api/                  # контроллеры, DI
└── ProPhoneService.Api.Tests/            # xUnit
```

Слои направлены внутрь: `Api` -> `Application` -> `Domain`, Репозитории - generic `IRepository<T>` + специализированные, scoped lifetime, batch-сохранение через `SaveChangesAsync`

## Машина состояний заказа

Смена статуса - только через `RepairOrder.ChangeStatus()`, карта переходов - `RepairStatusTransitions` в `Domain.Shared`
Заказ создаётся фабрикой `RepairOrder.Create(...)`: до инициализации `Status == null`, `ChangeStatus` выбрасывает исключение

```
Created → InDiagnostic → Agreed → InProgress → Ready → Completed; отмена (Cancelled) из Created/Agreed/InProgress
```

Каждая смена пишет запись в `repair_status_history` одним батчем. Запрещённый переход -> 409 Conflict. Конфликт параллельных изменений (токен конкурентности `xmin`) ловится в репозитории как `ConcurrencyConflictException` и тоже отдаётся как 409

## Эндпоинты API

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/services` | прайс-лист активных услуг (HybridCache, 5 мин) |
| GET | `/api/orders?status=&page=` | страница заявок панели мастера (фильтр по статусу, пагинация) |
| GET | `/api/orders/{id}` | карточка заказа: клиент, устройство, услуги, тайм-лайн статусов |
| POST | `/api/orders` | создание заявки (клиент find-or-create по телефону/email, 201 + номер) |
| PATCH | `/api/orders/{id}/status` | смена статуса по машине состояний (409 при запрещённом переходе/конфликте) |

Ошибки валидации возвращаются как `ValidationProblemDetails` со списком полей; при создании заявки обязательно должен быть телефон или почта, дата визита проверяется на рабочие часы и "не в прошлом"

## CI

`.github/workflows/ci.yml` - конфигурация CI/CD, запускается на каждый push:

- **backend**: `dotnet restore` → `build` → `test`
- **client**: `npm ci` → `lint`
