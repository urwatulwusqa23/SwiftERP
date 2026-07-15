# SwiftERP

A modular ERP system for small and medium businesses, built as a portfolio project to demonstrate backend engineering discipline: Clean Architecture, real transactional consistency across modules, and a testing/CI setup that mirrors production practice rather than a toy CRUD demo.

This repository currently contains all four ERP modules — **Phase 1 (Inventory & Procurement)**, **Phase 2 (Sales & Invoicing + Finance Ledger)**, **Phase 3 (HR & Payroll)** — plus **Phase 4 (cross-module dashboard + Redis)**: two independent cross-module atomic transactions (Sales→Inventory/Finance, HR→Finance), a real-time notification pipeline over Redis pub/sub, and a cache-aside dashboard that aggregates all four modules.

## Why this project

Most ERP-shaped side projects are single-purpose CRUD apps. What makes an ERP genuinely hard is that its modules share entities and must stay transactionally consistent — a sale has to decrement stock *and* post a ledger entry in the same transaction, or the books don't balance. Phase 1 built the Inventory module and its testing/CI scaffolding. Phase 2 added Sales & Invoicing and a minimal Finance ledger, implementing that cross-module atomic transaction end-to-end. Phase 3 adds HR & Payroll and proves the same transactional pattern isn't a one-off special case: posting a payroll run marks it Posted and records a Finance ledger expense atomically, using the identical DbContext-sharing technique as Sales. Phase 4 turns the domain events those transactions already raise (`ProductLowStockEvent`, `PayrollRunPostedEvent`) into a real capability: a Redis pub/sub notification stream, and a cross-module dashboard cached with the same events used for explicit invalidation — closing the loop between "a domain event fired" and "something outside the process actually reacted to it."

## Architecture

Clean Architecture, one project per layer, organized as a modular monolith so each business module owns its own Domain/Application/Infrastructure slice:

```
src/
  Modules/
    Inventory/
      SwiftERP.Inventory.Domain/         # Product, Supplier, PurchaseOrder — no framework dependencies
      SwiftERP.Inventory.Application/    # MediatR commands/queries, FluentValidation
      SwiftERP.Inventory.Infrastructure/ # EF Core, SQL Server, repositories
    Sales/
      SwiftERP.Sales.Domain/             # SaleOrder (+ lines), Draft/Confirmed/Cancelled lifecycle
      SwiftERP.Sales.Application/        # CreateSaleOrder, ConfirmSaleOrder (the cross-module transaction), MarkPaid
      SwiftERP.Sales.Infrastructure/     # SalesDbContext — spans Sales/Inventory/Finance for atomic confirm
    Finance/
      SwiftERP.Finance.Domain/           # LedgerEntry, signed amount by entry type
      SwiftERP.Finance.Application/      # GetRunningBalance
      SwiftERP.Finance.Infrastructure/   # EF Core, SQL Server, repositories
    HR/
      SwiftERP.HR.Domain/                # Employee, AttendanceRecord, PayrollRun (+ lines)
      SwiftERP.HR.Application/           # HireEmployee, RecordAttendance, CreatePayrollRun, PostPayrollRun (cross-module)
      SwiftERP.HR.Infrastructure/        # HrDbContext — spans HR/Finance for atomic payroll posting
  SwiftERP.Api/                          # ASP.NET Core host: endpoints, Swagger, Serilog, DI wiring,
                                          #   dashboard cache-aside, Redis notification handlers
  SwiftERP.SharedKernel/                 # Entity, DomainEvent, Result, DomainEventCollector — shared across all modules
tests/
  SwiftERP.Inventory.Domain.Tests/       # xUnit — Inventory domain rules
  SwiftERP.Sales.Domain.Tests/           # xUnit — Sales domain rules
  SwiftERP.Finance.Domain.Tests/         # xUnit — Finance domain rules
  SwiftERP.HR.Domain.Tests/              # xUnit — HR domain rules
  SwiftERP.Inventory.Integration.Tests/  # xUnit + Testcontainers — real SQL Server + real Redis, real EF Core
                                          # migrations, full HTTP pipeline, cross-module atomicity, cache-aside,
                                          # and pub/sub proofs
```

Modules communicate through Application-layer interfaces the *consuming* module owns (e.g. `Sales.Application.Abstractions.ISalesInventoryPort`, `HR.Application.Abstractions.IHrLedgerPort`), not the producing module's own repository interface — see **Engineering decisions** below for why.

### Domain events → Redis, end to end

`Product.DecrementStock` and `PayrollRun.Post` raise domain events (`ProductLowStockEvent`, `PayrollRunPostedEvent`) on the entity via `SharedKernel.Entity.Raise`. Every DbContext that tracks those entity types (`InventoryDbContext`, `SalesDbContext`, `HrDbContext` — Product is tracked by two of them, see Engineering decisions) collects and clears those events after a successful `SaveChangesAsync`, then publishes each through MediatR's `IPublisher`. Two kinds of Api-layer `INotificationHandler`s pick them up:

- `ProductLowStockNotificationHandler` / `PayrollRunPostedNotificationHandler` → `RedisNotificationPublisher.PublishAsync` → a message on the `swifterp:notifications` Redis pub/sub channel (`stock.low`, `payroll.processed`).
- `DashboardCacheInvalidationHandler` → removes the cached dashboard summary from Redis immediately, rather than waiting out its TTL.

Because publishing happens inside `SaveChangesAsync` (after the underlying transaction has actually committed), a subscriber never sees a notification for a change that later rolled back.

### Business rules enforced in the domain layer

- Stock can never go negative (`Product.DecrementStock`)
- Crossing the reorder threshold raises a `ProductLowStockEvent`
- Purchase orders and sale orders require at least one line and can only be received/confirmed/cancelled once
- Concurrent stock decrements against limited quantity cannot oversell — enforced via an EF Core optimistic concurrency token (`RowVersion`), verified by a dedicated concurrency test
- A sale order can only be marked paid once, and only after confirmation
- Ledger entries have a positive amount and a type-derived sign (`SaleRevenue` credits, `PurchaseExpense`/`PayrollExpense` debit)
- A payroll run requires at least one line, targets a valid month (1–12), and can only be posted once
- Employees have a positive monthly salary and can only be terminated once

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core (.NET 10) minimal APIs |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API), modular monolith |
| CQRS | MediatR, with a validation pipeline behavior per module |
| Validation | FluentValidation |
| ORM | Entity Framework Core |
| Database | SQL Server (single shared database across modules) |
| Cache / messaging | Redis — `IDistributedCache` cache-aside for the dashboard, pub/sub for real-time notifications |
| Logging | Serilog (structured, request logging) |
| Testing | xUnit, Testcontainers.MsSql, Microsoft.AspNetCore.Mvc.Testing |
| CI/CD | GitHub Actions |
| Containerization | Docker Compose |

## Running locally

### Prerequisites
- .NET 10 SDK
- Docker Desktop

### 1. Start local infrastructure

```bash
docker compose up -d
```

This starts SQL Server (port 1433) and Redis (port 6379).

### 2. Apply migrations

Each module owns its own EF Core migrations against the same `SwiftErpDb` database:

```bash
dotnet ef database update --project src/Modules/Inventory/SwiftERP.Inventory.Infrastructure --startup-project src/SwiftERP.Api
dotnet ef database update --project src/Modules/Finance/SwiftERP.Finance.Infrastructure --startup-project src/SwiftERP.Api --context SwiftERP.Finance.Infrastructure.Persistence.FinanceDbContext
dotnet ef database update --project src/Modules/Sales/SwiftERP.Sales.Infrastructure --startup-project src/SwiftERP.Api --context SwiftERP.Sales.Infrastructure.Persistence.SalesDbContext
dotnet ef database update --project src/Modules/HR/SwiftERP.HR.Infrastructure --startup-project src/SwiftERP.Api --context SwiftERP.HR.Infrastructure.Persistence.HrDbContext
```

Order matters: Inventory owns `Products`, Finance owns `LedgerEntries`; Sales' migration only creates `SaleOrders`/`SaleOrderLines`, and HR's only creates `Employees`/`AttendanceRecords`/`PayrollRuns`/`PayrollRunLines` (Product and LedgerEntry are mapped as `ExcludeFromMigrations` in `SalesDbContext`/`HrDbContext` — see Engineering decisions).

### 3. Run the API

```bash
dotnet run --project src/SwiftERP.Api
```

Open the Swagger UI at the URL printed on startup (e.g. `https://localhost:xxxx/swagger`) to try:

- `POST /api/v1/inventory/products` / `PUT /api/v1/inventory/products/{id}/stock` / `GET /api/v1/inventory/products/low-stock`
- `POST /api/v1/inventory/purchase-orders` / `POST /api/v1/inventory/purchase-orders/{id}/receive`
- `POST /api/v1/sales/orders` — create a draft sale order
- `POST /api/v1/sales/orders/{id}/confirm` — **the cross-module transaction**: decrements stock on every line's product and posts a ledger entry, atomically
- `POST /api/v1/sales/orders/{id}/mark-paid`
- `GET /api/v1/finance/ledger/balance` — running balance across all posted ledger entries
- `POST /api/v1/hr/employees` — hire an employee
- `POST /api/v1/hr/employees/{id}/attendance` — record a daily attendance entry
- `POST /api/v1/hr/payroll-runs` — create a draft payroll run from all active employees' salaries
- `POST /api/v1/hr/payroll-runs/{id}/post` — **the second cross-module transaction**: marks the run Posted and records a Finance ledger expense, atomically
- `GET /api/v1/dashboard` — cross-module summary (low-stock count, draft sale orders, Finance running balance, active employees), cache-aside via Redis

To watch the pub/sub side live, run `redis-cli SUBSCRIBE swifterp:notifications` in another terminal, then adjust a product's stock below its reorder threshold or post a payroll run — a `stock.low` or `payroll.processed` message shows up immediately.

## Testing

```bash
# Unit tests — pure domain rules, no external dependencies
dotnet test tests/SwiftERP.Inventory.Domain.Tests
dotnet test tests/SwiftERP.Sales.Domain.Tests
dotnet test tests/SwiftERP.Finance.Domain.Tests
dotnet test tests/SwiftERP.HR.Domain.Tests

# Integration tests — spins up real SQL Server + Redis containers via Testcontainers,
# runs actual EF Core migrations, and exercises the full HTTP pipeline
dotnet test tests/SwiftERP.Inventory.Integration.Tests
```

74 tests total (60 unit + 14 integration). Six integration tests are the project's centerpiece:

- **`ConfirmSaleOrder_WithInsufficientStock_RollsBackBothStockAndLedger`** — confirms a sale order for more stock than exists, then asserts the Product quantity is **unchanged** and **no ledger entry was created** — proving the failure rolled back both sides, not just one.
- **`ConfirmSaleOrder_WithSufficientStock_CommitsStockDecrementAndLedgerEntryTogether`** — confirms the happy path and asserts the stock decrement and the ledger entry both landed together.
- **`PostPayrollRun_WithActiveEmployees_PostsRunAndLedgerEntryTogether`** — posts a payroll run and asserts the ledger expense matches the run's own persisted line total.
- **`PostPayrollRun_WhenAlreadyPosted_FailsAndDoesNotDuplicateLedgerEntry`** — posts twice, asserts the second attempt fails and exactly one ledger entry exists — proving idempotency at the domain layer, not just the database.
- **`ProductCrossingLowStockThreshold_InvalidatesCachedDashboard`** — warms the dashboard cache, then crosses a product's reorder threshold, and polls Redis for the cache key to actually disappear — proving explicit invalidation fires, not just that the TTL will eventually expire.
- **`DecrementingStockBelowThreshold_PublishesStockLowNotification`** / **`PostingPayrollRun_PublishesPayrollProcessedNotification`** — a *real* `ISubscriber` on `swifterp:notifications` receives the message end-to-end, proving the domain-event → MediatR → Redis pub/sub pipeline actually delivers, not just that a handler method was called.

Plus the Phase 1 concurrency test: two simultaneous stock decrements against limited quantity, asserting exactly one succeeds.

## Engineering decisions

- **Why a modular monolith instead of microservices**: cross-module transactional consistency (the project's core challenge) is dramatically harder across service boundaries. A modular monolith gets the same domain separation and interview talking points without distributed-transaction complexity that isn't the point of this project.
- **How the cross-module transaction actually commits atomically**: `SalesDbContext` (Sales.Infrastructure) maps `SaleOrder` (owned) alongside Inventory's `Product` and Finance's `LedgerEntry` entities, reusing their existing `IEntityTypeConfiguration` classes but marking those two `ExcludeFromMigrations` — Sales doesn't own their schema, it only shares the connection and change tracker. Because all three entities are tracked by **one `DbContext` instance**, a single `SaveChangesAsync()` call wraps the stock decrement, the ledger insert, and the order-confirmation update in one SQL transaction for free — no distributed transaction coordinator needed. If `Product.DecrementStock` throws (insufficient stock), it throws *before* `SaveChangesAsync` is ever called, so nothing partially commits.
- **Why Sales defines its own `ISalesInventoryPort`/`ISalesLedgerPort` instead of reusing Inventory's `IProductRepository`/Finance's `ILedgerRepository`**: registering `SalesScopedProductRepository` against Inventory's own `IProductRepository` interface in the same DI container would have created two competing registrations for the same interface — whichever module's `AddXInfrastructure()` ran last in `Program.cs` would silently win, meaning Inventory's *own* endpoints could end up resolving Sales' repository instead of its own. Distinct consumer-owned interfaces avoid that collision entirely while still sharing the same underlying table.
- **Why `ProductConfiguration`/`LedgerEntryConfiguration` set an explicit schema** (`"inventory"`, `"finance"`) instead of relying on each `DbContext`'s `HasDefaultSchema`: `SalesDbContext` has its own default schema (`"sales"`). Without an explicit schema on the shared entity configs, EF Core resolved `Product` to `sales.Products` under `SalesDbContext` — a table that doesn't exist — instead of the real `inventory.Products`. This was an actual bug caught by the integration tests (`Invalid object name 'sales.Products'`), not a hypothetical.
- **Why the connection string is read *inside* the `AddDbContext` options delegate, not captured as a variable beforehand**: `configuration.GetConnectionString(...)` evaluated eagerly in `AddXInfrastructure()` bakes in whatever `appsettings.json` says at DI-registration time, before `WebApplicationFactory`'s test-time configuration override has a chance to apply. This silently pointed the API's test host at a real local SQL Server instance instead of the ephemeral Testcontainers database, and only surfaced once a test cross-checked data seeded directly against Testcontainers with data written through the API — exactly the kind of bug the cross-module atomicity tests exist to catch.
- **Why HR's `PostPayrollRun` reuses the exact same DbContext-sharing pattern as Sales' `ConfirmSaleOrder`, rather than inventing something new**: proving the pattern is reusable — not a one-off hack that happened to work for Sales — is the point of building a second, independent cross-module transaction in Phase 3. `HrDbContext` tracks `PayrollRun` (owned) and Finance's `LedgerEntry` (`ExcludeFromMigrations`, same as Sales does with `Product`/`LedgerEntry`), and `IHrLedgerPort` is HR's own consumer-owned interface for the same DI-collision reason as `ISalesInventoryPort`/`ISalesLedgerPort`.
- **Why the payroll idempotency test checks the ledger table, not just the HTTP status code**: `PostPayrollRunCommandHandler` calls `payrollRun.Post()` before adding the ledger entry, so a naive test could pass on "second call returns 400" while still silently double-posting a ledger entry if the ordering were ever wrong. `PostPayrollRun_WhenAlreadyPosted_FailsAndDoesNotDuplicateLedgerEntry` asserts `Assert.Single(ledgerEntries)` after both calls — proving the domain-level guard (`PayrollRun.Post` throwing when already `Posted`) actually prevents the second `SaveChangesAsync` from running at all, not just that the response looked right.
- **Why optimistic concurrency over pessimistic locking**: stock adjustments are infrequent relative to reads; a `RowVersion` token keeps the common path lock-free and only pays a retry cost on genuine conflicts, which is what the concurrency test verifies.
- **Why MediatR + a validation pipeline behavior**: keeps FluentValidation out of every handler — validation runs once, centrally, before any handler executes, and API endpoints stay thin (parse request → send command → map result).
- **Why `DomainEventCollector` lives in `SharedKernel` but doesn't touch EF Core**: three different DbContexts (`InventoryDbContext`, `SalesDbContext`, `HrDbContext`) all need the identical "snapshot tracked entities → save → collect their events → clear → publish" sequence. Putting that logic in the shared kernel avoids copy-pasting it three times, but `SharedKernel` is also referenced directly by every `Domain` project — pulling in `Microsoft.EntityFrameworkCore` there would leak an infrastructure dependency into code that's supposed to have none. `DomainEventCollector` is plain C# over `IEnumerable<Entity>`; each DbContext still owns its own `ChangeTracker.Entries<Entity>()` call.
- **Why `SalesDbContext` needs the *same* event-dispatch logic as `InventoryDbContext`, not just its own**: `ConfirmSaleOrder`'s stock decrement runs through `ISalesInventoryPort`, which is backed by `SalesDbContext`, not `InventoryDbContext` — so a `ProductLowStockEvent` raised by a sale crossing the reorder threshold is tracked and would be collected by *that* context. Missing this would mean stock-outs caused by Inventory's own adjustment endpoint notify correctly, but stock-outs caused by a sale silently don't — an easy gap to leave if the dispatch logic isn't re-verified for every DbContext that happens to track `Entity` subclasses, not just the "obvious" owning one.
- **Why the dashboard cache-aside doesn't invalidate on *every* write**: doing that precisely would mean raising a domain event from every command that changes a number the dashboard shows (stock received, sale order created, employee hired, ...) — a lot of surface area for a portfolio-scale feature. The chosen scope explicitly invalidates on the two events that already exist for the notification pipeline (`ProductLowStockEvent`, `PayrollRunPostedEvent`) and relies on a short (30s) TTL as a self-healing fallback for everything else — a standard, documented trade-off in cache-aside designs, not an oversight.
- **Why domain events are published from *inside* `SaveChangesAsync`, after the base call, rather than from the command handler**: publishing after `base.SaveChangesAsync()` succeeds guarantees a subscriber only ever sees a notification for a change that is actually durable — if the transaction fails, the method throws before any event reaches MediatR. Publishing from the handler instead would require every handler to remember to do it in the right order, and would break if a future handler used its own transaction scope.

## Frontend — "Command Deck" (`frontend/`)

A React 18 + TypeScript + Vite SPA, deliberately not styled like a generic admin template: a full-viewport animated canvas renders the four modules as a live node graph (Inventory/Sales/Finance/HR), with connections that pulse to visualize data flowing between them — the same relationships `MODULE_EDGES` in `networkLayout.ts` encodes are the actual cross-module dependencies the backend has. A dark, near-black palette with per-module accent colors (teal/amber/blue/rose, no purple) replaces the default light/dark Vite template; type pairing is Fraunces (display serif) + Space Grotesk (UI) + JetBrains Mono (data), not Inter.

- **Live, not simulated**: `useNotificationStream` opens a real `EventSource` against `GET /api/v1/notifications/stream` (a new SSE endpoint bridging the same `swifterp:notifications` Redis channel the backend already publishes to). A stock-low or payroll-processed event triggers a ring pulse on the corresponding node in the canvas *and* a live entry in the Activity Feed — verified by hand: creating a product via curl and pushing its stock below threshold showed the event land in the browser within the same second, no page refresh.
- **Dashboard stats** (`GET /api/v1/dashboard`) animate in with Framer Motion's `useMotionValue`/`animate`, not static numbers.
- **Module pages** (Inventory/Sales/Finance/HR) are functional against the real API — create a product, confirm a sale order, hire an employee, post a payroll run — styled consistently with the same design tokens, not a second visual language bolted on for "the CRUD part."

Run it:

```bash
cd frontend
npm install
npm run dev   # http://localhost:5173 — expects the API running on :5199 (see CORS policy in Program.cs)
```

## HR Tier 1 — full Employee Database, Leave, Attendance, Self-Service

Beyond the original payroll-focused HR module, Tier 1 adds:

- **Employee Database**: full profile (personal/job/contact info), a self-referencing `ManagerId` for the org chart (`GET /api/v1/hr/org-chart`, built in-memory with cycle protection), and document storage — upload a contract/ID/certificate via multipart form data, stored on local disk (`IDocumentStorage` port, `LocalDiskDocumentStorage` impl — swappable for cloud blob storage later without touching Application/Domain).
- **Leave Management**: `LeaveBalance` (get-or-create per employee/type/year, defaults: 10 sick / 10 casual / 20 annual), `LeaveRequest` (Pending → Approved/Rejected), and a holiday calendar. Approval is the authoritative balance check (`LeaveBalance.UseDays` throws if it would overdraw); the request-time check is a fast-fail UX nicety, not the source of truth — proven by `LeaveManagementTests`.
- **Attendance upgrade**: replaced the original present/absent boolean with real clock-in/clock-out timestamps and computed overtime (`AttendanceRecord.OvertimeHours`, hours beyond an 8-hour standard shift).
- **Self-Service Portal** (`/portal` in the frontend): no auth yet, so it's an explicit "viewing as" employee picker rather than a login — profile edit, clock in/out, leave balances + request/approve/reject, and document upload/download, all against the real API.

Enums now serialize as their string names (`JsonStringEnumConverter`, registered globally in `Program.cs`) rather than raw integers — added specifically so the growing number of enum-typed DTOs (`LeaveType`, `EmployeeDocumentType`, ...) stay readable across the API surface.

## Roadmap

- [x] Phase 1 — Inventory & Procurement
- [x] Phase 2 — Sales & Invoicing, Finance Ledger, cross-module atomic transaction
- [x] Phase 3 — HR & Payroll, second independent cross-module atomic transaction
- [x] Phase 4 — Cross-module dashboard, Redis cache-aside + pub/sub notifications
- [x] Phase 5 — React frontend, live SSE-driven Command Deck
- [x] HR Tier 1 — Employee Database, Leave Management, Attendance upgrade, Self-Service Portal (this repo state)
- [ ] Phase 6 — Docker image publish + live deployment
