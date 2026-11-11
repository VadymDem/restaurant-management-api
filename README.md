# RRMS API — Restaurant Reservation Management System

REST API backend for restaurant reservation management system.

## Tech Stack

| Component         | Technology                          |
|-------------------|-------------------------------------|
| Framework         | ASP.NET Core 10 Web API (.NET 10)   |
| ORM               | Entity Framework Core 10            |
| Database          | PostgreSQL (hosted on Supabase)     |
| Authentication    | JWT Bearer tokens                   |
| Password Hashing  | BCrypt (BCrypt.Net-Next)            |
| API Documentation | Swashbuckle 10 (Swagger UI)         |

## Architecture

Layered (4 projects) following clean separation of concerns:

```
RRMS.slnx
├── src/
│   ├── RRMS.Domain/            # Entities, enums, domain constants
│   ├── RRMS.Application/       # Business logic interfaces, DTOs, service contracts
│   ├── RRMS.Infrastructure/    # EF Core DbContext, repositories, DB seeding
│   └── RRMS.Api/               # ASP.NET Core controllers, middleware, startup
└── tests/
    └── RRMS.Application.Tests/ # xUnit unit tests
```

Dependency flow:

```
RRMS.Api ──► RRMS.Application ──► RRMS.Domain
       └──► RRMS.Infrastructure ──► RRMS.Application
                                   ──► RRMS.Domain
```

## Project Structure

### RRMS.Domain

Core entities and shared constants. No external dependencies.

- `Entities/` — `User`, `RestaurantTable`, `Reservation`, `MenuItem`
- `Enums/` — `UserRole` (Customer / Administrator), `ReservationStatus` (Pending / Confirmed / Cancelled / Completed)
- `Constants/` — `Roles` string constants for `[Authorize]` attributes

### RRMS.Application

Contracts between API and Infrastructure layers.

- `DTOs/` — request/response records grouped by domain (`Auth/`, `Menu/`, `Tables/`, `Reservations/`)
- `Interfaces/Repositories/` — `IUserRepository`, `IMenuItemRepository`, `IRestaurantTableRepository`, `IReservationRepository`, `IUnitOfWork`
- `Interfaces/Services/` — `IAuthService`, `IMenuService`, `ITableService`, `IReservationService`, `IJwtTokenService`
- `Services/` — `AuthService` (registration/login), `MenuService` (menu CRUD), `TableService` (table management and availability), `ReservationService` (creation, own reservations, admin status management; customer cancellation is not implemented)
- `Options/JwtOptions.cs` — JWT configuration model
- `Exceptions/` — `DomainException` and derived errors (404/401/409) mapped to ProblemDetails by middleware
- `DependencyInjection.cs` — `AddApplication()` extension method

### RRMS.Infrastructure

Data access implementation.

- `Data/AppDbContext.cs` — EF Core `DbContext` with `DbSet<>` properties for all entities
- `Data/AppDbContextFactory.cs` — design-time factory for `dotnet ef migrations`
- `Data/DbSeeder.cs` — startup data seeding (admin account, sample menu)
- `Configurations/` — EF Fluent API per entity (`IEntityTypeConfiguration<T>`)
- `Repositories/` — repository and `UnitOfWork` implementations
- `DependencyInjection.cs` — `AddInfrastructure()` extension method (registers DbContext + repositories)

### RRMS.Api

Web API layer (entry point).

- `Controllers/`
  - `AuthController` — `POST /api/auth/register`, `POST /api/auth/login` (anonymous)
  - `MenuController` — full CRUD `GET/POST/PUT/DELETE /api/menu` (read: anonymous, write: admin)
  - `TablesController` — full CRUD `GET/POST/PUT/DELETE /api/tables` (read: authenticated, write: admin)
  - `ReservationsController` — `POST`, `GET /my`, `DELETE /{id}` (authenticated)
  - `AdminReservationsController` — `GET`, `PUT /{id}/status` at `/api/admin/reservations` (admin only)
- `Middleware/ExceptionHandlingMiddleware.cs` — centralized error handling
- `Extensions/ServiceCollectionExtensions.cs` — Swagger + JWT + CORS registration
- `Program.cs` — application entry point
- `appsettings.json` — connection string and JWT configuration placeholders

## API Endpoints

| Method  | Endpoint                         | Access          | Description                      |
|---------|----------------------------------|-----------------|----------------------------------|
| POST    | `/api/auth/register`             | Public          | Register a customer account      |
| POST    | `/api/auth/login`                | Public          | Authenticate and get JWT token   |
| GET     | `/api/menu`                      | Public          | Get menu items (unavailable hidden; `?includeUnavailable=true` — administrator only) |
| GET     | `/api/menu/{id}`                 | Public          | Get a single menu item           |
| POST    | `/api/menu`                      | Administrator   | Create menu item                 |
| PUT     | `/api/menu/{id}`                 | Administrator   | Update menu item                 |
| DELETE  | `/api/menu/{id}`                 | Administrator   | Delete menu item                 |
| GET     | `/api/tables`                    | Authenticated   | Get all tables                   |
| GET     | `/api/tables/available`          | Authenticated   | Get available tables (optional `date`, `time`, `guests`) |
| POST    | `/api/tables`                    | Administrator   | Create a table                   |
| PUT     | `/api/tables/{id}`               | Administrator   | Update a table                   |
| DELETE  | `/api/tables/{id}`               | Administrator   | Deactivate a table (soft delete)  |
| POST    | `/api/reservations`              | Authenticated   | Create reservation               |
| GET     | `/api/reservations/my`           | Authenticated   | Get own reservations             |
| DELETE  | `/api/reservations/{id}`         | Reservation owner| Cancel reservation (NOT IMPLEMENTED) |
| GET     | `/api/admin/reservations`        | Administrator   | Get all reservations             |
| PUT     | `/api/admin/reservations/{id}/status` | Administrator | Update reservation status     |

## Reservation Statuses

Statuses are exchanged as strings (`Pending`, `Confirmed`, `Cancelled`, `Completed`) in API requests and responses.

- `Pending` — initial state after creation
- `Confirmed` — set by administrator
- `Cancelled` — may be set by an administrator; customer cancellation is not implemented.
- `Completed` — terminal status set by an administrator

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Node.js and npm compatible with the checked-in Vite 8 toolchain (check `frontend/package.json` and package engine requirements)
- A PostgreSQL instance (the project uses Supabase PostgreSQL)
- Network access to the configured PostgreSQL instance

### Setup

```bash
# restore dependencies
dotnet restore RRMS.slnx

# build
dotnet build RRMS.slnx

# run tests
dotnet test RRMS.slnx

# set configuration (see "Environment Variables" below)

# run
dotnet run --project src/RRMS.Api
```

In the Development environment, open the Swagger UI at `<backend-base-url>/swagger` (where `<backend-base-url>` is the URL printed by `dotnet run`). Use **Authorize** with the JWT returned by `POST /api/auth/login`. Swagger UI is enabled only in Development.

### Environment Variables

Override `appsettings.json` without editing tracked files (PowerShell syntax):

```powershell
$env:ConnectionStrings__SupabaseConnection = "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
$env:Jwt__Issuer = "rrms-api"
$env:Jwt__Audience = "rrms-client"
$env:Jwt__SecretKey = "random-secret-at-least-32-chars"
$env:Restaurant__TimeZone = "Europe/Kyiv"
$env:AdminSeed__Email = "admin@example.com"
$env:AdminSeed__Password = "a-strong-password"
```

**Required:** the JWT `SecretKey` must be at least 32 characters long, otherwise the API fails fast at startup.

### Configuration

`src/RRMS.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "your-supabase-connection-string"
  },
  "Jwt": {
    "Issuer": "rrms-api",
    "Audience": "rrms-client",
    "SecretKey": "your-secret-key-at-least-32-chars",
    "ExpirationMinutes": 60
  },
  "Restaurant": {
    "TimeZone": "Europe/Kyiv"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:5173" ]
  },
  "Resend": {
    "ApiKey": "your-resend-api-key"
  },
  "AdminSeed": {
    "Name": "Administrator",
    "Email": "your-admin-email",
    "Password": "your-admin-password"
  }
}
```

`AdminSeed` is used on the first startup: if no users exist yet, it creates an administrator account (idempotent; skipped if empty).

`Cors:AllowedOrigins` lists the browser origins allowed to call the API; when the list is empty, no
cross-origin requests are allowed. `Resend:ApiKey` is required for welcome emails — when it is missing the
registration still succeeds and the failure is logged as a warning.

> **Never commit real credentials.** Use environment variables or a secrets manager in production.

### Local Secrets

`appsettings.Development.json` intentionally contains no credentials. Keep local values in
[user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (stored outside the repo,
loaded automatically only in the Development environment):

```bash
dotnet user-secrets set "Jwt:SecretKey" "<random-secret-at-least-32-chars>" --project src/RRMS.Api
dotnet user-secrets set "AdminSeed:Email" "admin@example.com" --project src/RRMS.Api
dotnet user-secrets set "AdminSeed:Password" "<strong-password>" --project src/RRMS.Api
```

### Database preparation

The repository includes `db/schema.sql`, which documents the project's PostgreSQL schema. The ZIP does not include a checked-in EF Core `Migrations/` directory. **Do not assume `dotnet ef database update` will create the necessary tables in a new database.**

For a fresh, isolated development database, inspect `db/schema.sql` before applying it and use an authorized PostgreSQL client to execute the reviewed schema. **Never reapply a schema blindly to an existing database containing data.** Verify the schema, PostgreSQL connection settings, and seed behavior before starting the application. The application invokes `DbSeeder.SeedAsync` during startup; it does not automatically run EF Core migrations.

If the team later introduces tracked EF Core migrations, document the approved migration and rollback procedure here. Backup and restoration procedures must be tested separately for handover.

## NuGet Packages

| Project            | Package                                      | Purpose                 |
|--------------------|----------------------------------------------|-------------------------|
| RRMS.Api           | `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT authentication  |
| RRMS.Api           | `Swashbuckle.AspNetCore`                     | Swagger UI / OpenAPI    |
| RRMS.Api           | `Microsoft.EntityFrameworkCore.Design`      | EF Core migrations CLI  |
| RRMS.Application   | `Microsoft.Extensions.DependencyInjection.Abstractions` | DI registration |
| RRMS.Application   | `Microsoft.Extensions.Options`               | JWT options binding     |
| RRMS.Application   | `System.IdentityModel.Tokens.Jwt`            | JWT token generation    |
| RRMS.Application   | `BCrypt.Net-Next`                            | Password hashing        |
| RRMS.Infrastructure | `Npgsql.EntityFrameworkCore.PostgreSQL`    | PostgreSQL provider     |
| RRMS.Infrastructure | `Microsoft.EntityFrameworkCore`             | EF Core runtime         |
| RRMS.Infrastructure | `Microsoft.Extensions.Configuration.Abstractions` | Configuration access |

## Frontend (React + TypeScript + Vite)

The frontend source is in `frontend/`. It includes registration, login, menu browsing, reservation creation, personal reservations, and administrator pages for menu, tables, and reservations.

Run the backend first and note its actual base URL. From the repository root:

```bash
cd frontend
npm ci
```

Create `frontend/.env` from `frontend/.env.example` and set `VITE_API_BASE_URL` to the running backend's origin, such as `http://localhost:5214` **only if the API actually runs on that URL**. Keep the frontend origin allowed in the backend `Cors:AllowedOrigins` configuration.

```bash
npm run dev
npm run build
npm run lint
```

`npm run dev` starts the development server; open the URL printed by Vite. `npm run build` runs TypeScript checks and creates a production bundle. `npm run lint` runs ESLint. There is no frontend unit-test script in the supplied `frontend/package.json`.

## Continuous Integration

The workflow `.github/workflows/ci.yml` is triggered on pushes and pull requests targeting `main` and executes .NET 10 restore, Release build, and backend tests. It does **not** currently run frontend lint or frontend build. Verify results in GitHub Actions; the existence of the workflow is not a guarantee that the latest run passed.

## Features and implementation status (ZIP snapshot: 10 November 2026)

| Module | Status |
|---|---|
| Customer registration and JWT login | Implemented |
| Public menu browsing and administrator menu CRUD | Implemented |
| Table browsing and availability filtering | Implemented |
| Administrator table create/update/deactivation | Implemented |
| Customer reservation creation and `GET /my` | Implemented |
| Administrator reservation listing and status transitions | Implemented |
| Customer `DELETE /api/reservations/{id}` | **Not implemented** (`ReservationService.CancelAsync` throws `NotImplementedException`) |

### Known limitations

- Customer cancellation is exposed as an API route but is not functional. The unimplemented service operation may produce HTTP 500 rather than a successful cancellation. Track it as an open defect; do not mark it as tested or delivered.
- Reservation availability protects the same table/date/start-time slot. It does not check arbitrary overlapping time intervals with different starting times.
- Table deletion is a **soft delete** (`IsActive = false`); there is no dedicated reactivation endpoint in the supplied code.
- Database creation/upgrade and backup restoration are not provided as a fully automated, verified release procedure in this repository snapshot.
- Frontend build/lint is not part of the currently checked-in GitHub Actions workflow.

## Project documentation and handover

Keep documentation and evidence in the team's Confluence space and Jira project. Relevant tasks: SCRUM-48 (documentation), SCRUM-49 (regression testing), SCRUM-50 (final bug/test summary). Archive test runs, source revision or release tag, known limitations, ownership, access-granting instructions (never share passwords), and an independently performed setup/verification walkthrough before handover.

Repository: https://github.com/VadymDem/restaurant-management-api
