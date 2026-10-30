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
- `Enums/` — `UserRole` (Customer / Administrator), `ReservationStatus` (Pending / Confirmed / Cancelled)
- `Constants/` — `Roles` string constants for `[Authorize]` attributes

### RRMS.Application

Contracts between API and Infrastructure layers.

- `DTOs/` — request/response records grouped by domain (`Auth/`, `Menu/`, `Tables/`, `Reservations/`)
- `Interfaces/Repositories/` — `IUserRepository`, `IMenuItemRepository`, `IRestaurantTableRepository`, `IReservationRepository`, `IUnitOfWork`
- `Interfaces/Services/` — `IAuthService`, `IMenuService`, `ITableService`, `IReservationService`, `IJwtTokenService`
- `Services/` — `AuthService` (login), `MenuService` (menu CRUD), others throw `NotImplementedException`
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
| DELETE  | `/api/tables/{id}`               | Administrator   | Delete a table                   |
| POST    | `/api/reservations`              | Authenticated   | Create reservation               |
| GET     | `/api/reservations/my`           | Authenticated   | Get own reservations             |
| DELETE  | `/api/reservations/{id}`         | Reservation owner| Cancel reservation               |
| GET     | `/api/admin/reservations`        | Administrator   | Get all reservations             |
| PUT     | `/api/admin/reservations/{id}/status` | Administrator | Update reservation status     |

## Reservation Statuses

Statuses are exchanged as strings (`Pending`, `Confirmed`, `Cancelled`) both in requests and responses.

- `Pending` — initial state after creation
- `Confirmed` — set by administrator
- `Cancelled` — set by administrator or by the reservation owner

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Supabase project (for PostgreSQL connection string)

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

Open `https://localhost:<port>/swagger` to explore the API via Swagger UI (use **Authorize** with a token from `POST /api/auth/login`).

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
  "AdminSeed": {
    "Name": "Administrator",
    "Email": "your-admin-email",
    "Password": "your-admin-password"
  }
}
```

`AdminSeed` is used on the first startup: if no users exist yet, it creates an administrator account (idempotent; skipped if empty).

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

### Database Migrations

```bash
# create a migration (run from solution root)
dotnet ef migrations add InitialCreate --project src/RRMS.Infrastructure --startup-project src/RRMS.Api

# apply migrations
dotnet ef database update --project src/RRMS.Infrastructure --startup-project src/RRMS.Api
```

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

## Status

Stage 2 completed:

- `POST /api/auth/login` — verifies credentials with BCrypt and issues a signed JWT.
- JWT bearer authentication configured (`AddJwtBearer`, issuer/audience/signing-key validation).
- Swagger UI with Bearer security scheme; SwaggerUI is only enabled in Development.
- Centralized `ExceptionHandlingMiddleware` — domain errors map to proper HTTP statuses (404/401/409), unexpected exceptions to 500.
- Startup seeding of the default Administrator account.

Stage 3 (menu) completed:

- Menu CRUD at `/api/menu` backed by the `MenuItems` table (`GET` anonymous, writes administrator-only).
- Price validation: `0.01`–`1000000` with at most two decimal places (matches `NUMERIC(10,2)`).
- xUnit unit tests (`tests/RRMS.Application.Tests`) and GitHub Actions CI (`.github/workflows/ci.yml`).

Remaining (`NotImplementedException`): `POST /api/auth/register` and the table/reservation endpoints, plus EF Core migrations.