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

- `Entities/` — `User`
- `Enums/` — `UserRole` (Customer / Administrator)
- `Constants/` — `Roles` string constants for `[Authorize]` attributes

### RRMS.Application

Contracts between API and Infrastructure layers.

- `DTOs/Auth/` — `RegisterRequest`, `LoginRequest`, `LoginResponse`
- `Interfaces/Repositories/` — `IUserRepository`, `IUnitOfWork`
- `Interfaces/Services/` — `IAuthService`, `IJwtTokenService`
- `Services/` — skeleton implementations (throw `NotImplementedException`)
- `DependencyInjection.cs` — `AddApplication()` extension method

### RRMS.Infrastructure

Data access implementation.

- `Data/AppDbContext.cs` — EF Core `DbContext` with `User` DbSet
- `Data/AppDbContextFactory.cs` — design-time factory for `dotnet ef migrations`
- `Data/DbSeeder.cs` — startup data seeding (admin account)
- `Configurations/UserConfiguration.cs` — EF Fluent API for User
- `Repositories/` — `UserRepository`, `UnitOfWork`
- `DependencyInjection.cs` — `AddInfrastructure()` extension method (registers DbContext + repositories)

### RRMS.Api

Web API layer (entry point).

- `Controllers/AuthController` — `POST /api/auth/register`, `POST /api/auth/login` (anonymous)
- `Middleware/ExceptionHandlingMiddleware.cs` — centralized error handling
- `Extensions/ServiceCollectionExtensions.cs` — Swagger + JWT + CORS registration
- `Program.cs` — application entry point
- `appsettings.json` — connection string and JWT configuration placeholders

## API Endpoints

| Method | Endpoint              | Access | Description                    |
|--------|-----------------------|--------|--------------------------------|
| POST   | `/api/auth/register`  | Public | Register a new customer account |
| POST   | `/api/auth/login`     | Public | Authenticate and get JWT token  |

### POST `/api/auth/register`

**Request body:**
```json
{
  "name": "Anna Smith",
  "email": "anna@example.com",
  "password": "Password123"
}
```

**Logic:**
1. Validate: `name`, `email`, `password` required; `email` must be valid format; `password` min 6 chars
2. Check email uniqueness → `400 Bad Request` if duplicate
3. Hash the password
4. Create `User` with role `Customer`
5. Save to database
6. Generate JWT token with claims: `sub` (UserId), `email`, `role`
7. Return `200 OK` with `{ "token": "jwt-token" }`

### POST `/api/auth/login`

**Request body:**
```json
{
  "email": "anna@example.com",
  "password": "Password123"
}
```

**Logic:**
1. Find user by `email` → `401 Unauthorized` if not found
2. Verify password hash → `401 Unauthorized` if invalid
3. Generate JWT token (same format as registration)
4. Return `200 OK` with `{ "token": "jwt-token" }`

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

Scaffold phase — Auth endpoints are skeleton only (throw `NotImplementedException`).
The project compiles successfully. Database schema is being finalized by the team.
