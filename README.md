# WarehouseAPI

Inventory and order management REST API built with ASP.NET Core and PostgreSQL.

**API docs:** [warehouseapi-h38c.onrender.com/scalar/v1](https://warehouseapi-h38c.onrender.com/scalar/v1)  
**Demo app:** [warehouse-api-frontend.vercel.app](https://warehouse-api-frontend.vercel.app) — React client ([source](https://github.com/omrkocar/WarehouseAPI-Frontend))

Demo credentials: `admin` / `Admin123!`. Hosted on Render's free tier, so the first request after inactivity takes ~30s to cold start.

---

## Stack

ASP.NET Core (.NET 10) · Entity Framework Core + Npgsql · PostgreSQL · JWT Bearer auth · BCrypt.Net · Serilog · Scalar · xUnit · Docker

---

## Features

- Product CRUD with soft delete — discontinued products hidden globally via an EF query filter
- Order placement with server-side stock validation and decrement
- Order status state machine (Pending → Paid → Shipped / Cancelled) with restock on cancellation
- JWT authentication with Admin and Customer roles; product mutations are admin-only
- Optimistic concurrency control on stock updates
- Global exception handling and structured request logging

---

## Design Decisions

**Result pattern over exceptions for expected failures.** Business-rule rejections — insufficient stock, illegal status transition, username taken — return `Result<T>.Failure(message)` instead of throwing. Exceptions stay reserved for genuinely unexpected failures, which the global handler converts to clean responses.

**Optimistic concurrency on stock.** Each `Product` carries a `Version` GUID that EF Core embeds in the UPDATE WHERE clause. If two orders hit the last unit simultaneously, the second save throws `DbUpdateConcurrencyException`, triggering a retry that re-reads stock and re-validates. One order wins; no overselling.

**Registration cannot self-assign roles.** `POST /auth/register` is public but hardcodes `Role = "Customer"`, and the field is absent from `RegisterDto` entirely. Admin access is seeded at startup — escalation requires an already-privileged actor.

**DTO boundaries in both directions.** Input DTOs prevent over-posting: `UpdateProductDto` excludes `StockQuantity` because stock is a server-side operation, not a free-form field. Output DTOs control exposure — `IsDiscontinued` is internal and never leaves the API.

**Scalar over Swagger UI.** Swashbuckle was dropped from .NET 9+ templates. Scalar has first-class .NET 10 support and no package version conflicts.

---

## API

### Auth
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | `/api/auth/register` | Public | Register a customer account |
| POST | `/api/auth/login` | Public | Login, receive JWT |

### Products
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | `/api/products` | Public | List active products |
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

## Testing

Two layers, both against SQLite in-memory — the suite needs no external services.

**Unit tests** cover order creation rules, the status state machine, and a concurrency test that fires two simultaneous orders at the last unit in stock and asserts exactly one succeeds.

**Integration tests** boot the app in-process via `WebApplicationFactory<Program>` and exercise real HTTP endpoints through the full pipeline — routing, model binding, authentication, authorization, EF Core. The DbContext registration is swapped for an in-memory SQLite connection held open for the fixture's lifetime, so tests never touch development data.

These caught a real bug. `JwtBearerOptions.MapInboundClaims` defaults to `true`, rewriting the token's short claim names to legacy `ClaimTypes` URIs before the identity is built — so `RoleClaimType = "role"` never matched and every authenticated user got 403 on admin-only endpoints. The defect lived in middleware configuration rather than in any single testable class, which is the gap integration tests exist to close.

```bash
dotnet test
```

---

## Running Locally

Requires the .NET 10 SDK. SQLite is used automatically in Development — no local PostgreSQL needed.

```bash
git clone https://github.com/omrkocar/WarehouseAPI
cd WarehouseAPI
dotnet run
```

Scalar is then at `http://localhost:5108/scalar/v1`.

**Exercising the API:** log in as admin and paste the token into Scalar's **Authenticate** button, then create a product, register a customer, place an order as that customer (stock decrements), and transition the order status as admin. Posting a product with the customer token returns 403.

---

## Known Limitations

- Render's free tier cold starts after 15 minutes of inactivity (~30s first response)
- The free PostgreSQL instance expires periodically and is recreated; schema and the seed admin rebuild automatically on startup
- SQLite locally and in tests vs PostgreSQL in production — EF Core abstracts this, though SQLite's file-level locking differs from PostgreSQL's row-level locking, as noted in the concurrency test
- No token revocation — JWTs are stateless and valid until expiry; logout is client-side only

---

## Structure

```
WarehouseAPI/
├── Controllers/              thin controllers, delegate to repositories/services
├── Models/
│   ├── Entities/             EF Core domain entities
│   └── DTOs/                 input and output contracts
├── Repositories/             data access
├── Services/                 AuthService (registration, JWT issuance)
├── Mappings/                 ToDto() extension methods
├── Common/                   Result<T>, GlobalExceptionHandler
├── Data/                     WarehouseDbContext, EF configuration
└── Migrations/

WarehouseAPI.Tests/
├── Fixtures/
│   ├── DatabaseFixture.cs    SQLite isolation for unit tests
│   └── ApiFactory.cs         WebApplicationFactory host
├── OrderRepositoryTests.cs   order rules + concurrency
├── OrderStateMachineTests.cs status transition matrix
└── ProductsEndpointTests.cs  HTTP-level auth, roles, CRUD
```

---

*Built by Omer Kocar — [LinkedIn](https://www.linkedin.com/in/omrkocar/)*
