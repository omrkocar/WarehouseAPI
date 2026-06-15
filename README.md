# WarehouseAPI

An inventory and order management REST API built with ASP.NET Core and PostgreSQL, developed as a backend portfolio project demonstrating production-shaped .NET development.

**Live:** `https://warehouseapi-h38c.onrender.com` *(free tier — first request may take ~30s to cold start)*  
**Interactive docs:** `https://warehouseapi-h38c.onrender.com/scalar/v1`

---

## Stack

- **ASP.NET Core** (.NET 10) — Web API
- **Entity Framework Core** + **Npgsql** — ORM with PostgreSQL
- **BCrypt.Net** — password hashing
- **JWT Bearer** — authentication and role-based authorization
- **Serilog** — structured logging to console and file
- **Scalar** — interactive API documentation (replaces Swagger UI; native .NET 10)
- **xUnit** — unit and integration tests
- **Docker** — containerized deployment on Render

---

## Features

- Full product CRUD with soft-delete (discontinued products hidden globally via EF query filter)
- Order placement with server-side stock validation and decrement
- Order status state machine (Pending → Paid → Shipped / Cancelled) with restock on cancellation
- JWT authentication with Admin and Customer roles
- Role-gated endpoints — product mutations admin-only, order placement requires authentication
- Optimistic concurrency control on stock updates (race condition protection)
- Global exception handler returning clean error responses
- Structured request and event logging via Serilog
- Interactive API docs via Scalar with JWT bearer auth support

---

## Design Decisions

**Result pattern over exceptions for expected failures.** Business-rule rejections (insufficient stock, illegal status transition, username taken) return `Result<T>.Failure(message)` rather than throwing exceptions. Exceptions are reserved for genuinely unexpected failures, which the global handler catches and converts to clean 500/409 responses.

**Optimistic concurrency on stock.** Each `Product` carries a `Version` GUID token, bumped on every stock change. EF Core embeds this in UPDATE WHERE clauses — if two orders hit the last unit simultaneously, the second's save fails with `DbUpdateConcurrencyException`, triggering a retry loop (up to 3 attempts) that re-reads current stock and re-validates. One order wins; no overselling.

**Role-based registration security.** `POST /auth/register` is public but hardcodes `Role = "Customer"` — the field is absent from `RegisterDto`, so callers cannot self-assign admin privileges. Admin access is seeded at startup; privilege escalation requires an already-privileged actor.

**DTO boundaries in both directions.** Input DTOs prevent over-posting (e.g. `UpdateProductDto` excludes `StockQuantity` — stock changes are a server-side operation, not a free-form field). Output DTOs (`ProductDto`, `OrderDto`) control what callers see — `IsDiscontinued` is an internal implementation detail and never exposed.

**Scalar over Swagger UI.** Swashbuckle was deprecated in .NET 9+ templates. Scalar is the modern replacement — first-class .NET 10 support, no package version conflicts, cleaner UI. API docs are available at `/scalar/v1` in all environments.

---

## API Overview

### Auth
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | `/api/auth/register` | Public | Register a customer account |
| POST | `/api/auth/login` | Public | Login, receive JWT |

### Products
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | `/api/products` | Public | List all active products |
| GET | `/api/products/{id}` | Public | Get a product |
| POST | `/api/products` | Admin | Create a product |
| PUT | `/api/products/{id}` | Admin | Update a product |
| DELETE | `/api/products/{id}` | Admin | Soft-delete a product |

### Orders
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | `/api/orders` | Admin | List all orders |
| GET | `/api/orders/{id}` | Admin | Get an order |
| POST | `/api/orders` | Authenticated | Place an order |
| PUT | `/api/orders/{id}/status` | Admin | Transition order status |

---

## Testing the Live API

The easiest way to explore the API is via the interactive docs:

**`https://warehouseapi-h38c.onrender.com/scalar/v1`**

A default admin account is seeded on startup. To authenticate in Scalar:

1. Open the docs, find `POST /api/auth/login`, click **Test Request**
2. Send `{ "username": "admin", "password": "Admin123!" }`
3. Copy the token from the response
4. Paste the token in the **Authenticate** button (next to Bearer Token) — all subsequent requests will include it automatically

**Quick sequence to exercise the full API:**
1. Login as admin → get token, authenticate in Scalar
2. `POST /api/products` → create a product
3. `POST /api/auth/register` → create a customer account
4. Login as customer → get customer token, re-authenticate
5. `POST /api/orders` with customer token → stock decrements
6. `PUT /api/orders/{id}/status` with admin token → transition to Paid, then Shipped
7. Try `POST /api/products` with customer token → expect 403

---

## Running Locally

**Prerequisites:** .NET 10 SDK

SQLite is used automatically in Development — no local PostgreSQL needed.

```bash
git clone https://github.com/YOUR-USERNAME/WarehouseAPI
cd WarehouseAPI

# uses SQLite locally via appsettings.Development.json)
dotnet run
```

Scalar will be available at `https://localhost:7052/scalar/v1`.

**Run tests** (no external dependencies — tests use SQLite in-memory):
```bash
dotnet test
```

---

## Known Limitations

- Deployed on Render free tier — web service cold starts after 15 minutes of inactivity (~30s first response)
- Render free PostgreSQL instance expires 90 days from creation; the project will be redeployed if needed
- SQLite used locally and in tests vs PostgreSQL in production — EF Core's abstraction makes this valid; the one behavioral difference (SQLite file-level locking vs PostgreSQL row-level locking) is noted in the concurrency test
- No token revocation — JWTs are stateless and valid until expiry; logout is client-side only

---

## Project Structure
- WarehouseAPI/

  - Controllers/       — thin controllers, delegate to repositories/services

  - Models/

  - Entities/        — EF Core domain entities

  - DTOs/            — input and output contracts

  - Repositories/      — data access, IRepository pattern

  - Services/          — AuthService (registration, JWT issuance)

  - Mappings/          — ToDto() extension methods

  - Common/            — Result<T>, GlobalExceptionHandler

  - Data/              — WarehouseDbContext, EF configuration

  - Migrations/        — EF Core migrations

- WarehouseAPI.Tests/

  - Fixtures/          — DatabaseFixture (SQLite per-test isolation)

  - OrderStateMachineTests.cs

  - OrderRepositoryTests.cs  — includes concurrency integration test

---

*Built by Omer Kocar — [LinkedIn](https://www.linkedin.com/in/omrkocar/)*
