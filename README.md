# RRMS API — Restaurant Reservation Management System

REST API backend for restaurant reservation management system.

## Tech Stack

| Component         | Technology                          |
|-------------------|-------------------------------------|
| Framework         | ASP.NET Core 10 Web API (.NET 10)   |
| ORM               | Entity Framework Core 10            |
| Database          | PostgreSQL (hosted on Supabase)     |
| Authentication    | JWT Bearer tokens                   |
| API Documentation | Swashbuckle (Swagger UI)            |

## Architecture

Layered (4 projects) following clean separation of concerns:

```
RRMS.slnx
├── src/
│   ├── RRMS.Domain/            # Entities, enums, domain constants
│   ├── RRMS.Application/       # Business logic interfaces, DTOs, service contracts
│   ├── RRMS.Infrastructure/    # EF Core DbContext, repositories, DB seeding
│   └── RRMS.Api/               # ASP.NET Core controllers, middleware, startup
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
- `Services/` — skeleton implementations (throw `NotImplementedException`)
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
| GET     | `/api/menu`                      | Public          | Get all menu items               |
| POST    | `/api/menu`                      | Administrator   | Create menu item                 |
| PUT     | `/api/menu/{id}`                 | Administrator   | Update menu item                 |
| DELETE  | `/api/menu/{id}`                 | Administrator   | Delete menu item                 |
| GET     | `/api/tables`                    | Authenticated   | Get all tables                   |
| POST    | `/api/tables`                    | Administrator   | Create a table                   |
| PUT     | `/api/tables/{id}`               | Administrator   | Update a table                   |
| DELETE  | `/api/tables/{id}`               | Administrator   | Delete a table                   |
| POST    | `/api/reservations`              | Authenticated   | Create reservation               |
| GET     | `/api/reservations/my`           | Authenticated   | Get own reservations             |
| DELETE  | `/api/reservations/{id}`         | Reservation owner| Cancel reservation               |
| GET     | `/api/admin/reservations`        | Administrator   | Get all reservations             |
| PUT     | `/api/admin/reservations/{id}/status` | Administrator | Update reservation status     |

## Reservation Statuses

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

# set your Supabase connection string (PowerShell)
$env:SUPABASE_CONNECTION_STRING = "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"

# run
dotnet run --project src/RRMS.Api
```

Open `https://localhost:<port>/swagger` to explore the API via Swagger UI.

### Configuration

`src/RRMS.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "your-supabase-connection-string"
  },
  "Jwt": {
    "Issuer": "your-app",
    "Audience": "your-app",
    "SecretKey": "your-256-bit-secret",
    "ExpirationMinutes": 60
  }
}
```

> **Never commit real credentials.** Use environment variables or a secrets manager in production.

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
| RRMS.Infrastructure | `Npgsql.EntityFrameworkCore.PostgreSQL`    | PostgreSQL provider     |
| RRMS.Infrastructure | `Microsoft.EntityFrameworkCore`             | EF Core runtime         |
| RRMS.Infrastructure | `Microsoft.Extensions.Configuration.Abstractions` | Configuration access |

## Status

Scaffold phase — all service methods throw `NotImplementedException`.
The project compiles successfully. Implementation is pending.