# Car Care API

A production-grade backend for managing a car maintenance center, built with **.NET 8** and **Clean Architecture**. Customers register vehicles and book service appointments; staff advance appointments through a strict lifecycle; managers administer the service catalog and monitor background jobs via a secured Hangfire dashboard.

Built from the [CarCare PRD](CarCare_Project_EraSoft_PRD.pdf) — all 15 business rules (BR-01 … BR-15) are implemented and covered by tests.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Authentication & Authorization](#authentication--authorization)
- [Appointment Lifecycle](#appointment-lifecycle)
- [Business Rules](#business-rules)
- [Background Jobs](#background-jobs)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Contributing](#contributing)

---

## Features

- **Customer self-service** — registration, JWT login, profile, vehicle garage, appointment booking with overlap protection, self-cancellation, and per-vehicle service history.
- **Staff operations** — advance appointments through a strict state machine (`Requested → Confirmed → InService → Completed`).
- **Manager tools** — full service-catalog CRUD with soft-delete (deactivation), plus a role-protected Hangfire dashboard.
- **Price snapshotting** — booked appointments keep the price/duration captured at booking time; later catalog edits never rewrite history.
- **Concurrency-safe scheduling** — double-booking is prevented with serializable transactions and SQL Server lock hints (`UPDLOCK, ROWLOCK, HOLDLOCK`) around the conflict check + insert.
- **Stale-appointment cleanup** — hourly Hangfire recurring job auto-cancels `Requested` appointments untouched for 24 hours.
- **Robust API surface** — RFC 7807 ProblemDetails errors, FluentValidation pipeline behavior, health checks, and Swagger/OpenAPI with full XML documentation.

## Architecture

Clean Architecture with strict inward dependency flow:

```
┌─────────────────────────────────────────────────┐
│  App.Api          Controllers, Middleware,      │
│                   Program composition, Hangfire │
│                   dashboard authorization       │
├─────────────────────────────────────────────────┤
│  App.Infrastructure   EF Core, Identity, JWT,   │
│                   Hangfire jobs, repository &   │
│                   unit-of-work implementations  │
├─────────────────────────────────────────────────┤
│  App.Application      MediatR commands/queries, │
│                   validators, DTOs, mappers,    │
│                   abstraction interfaces        │
├─────────────────────────────────────────────────┤
│  App.Domain             Entities, enums,        │
│                   domain exceptions, status     │
│                   machine, role constants       │
└─────────────────────────────────────────────────┘
```

- **CQRS** via MediatR — every endpoint dispatches a command or query object.
- **Abstractions in Application, implementations in Infrastructure** — `IGenericRepository`, `IUnitOfWork`, `IVehicleScheduleLock`, `IIdentityService`, `IJwtTokenService`, `IClock`, `IBackgroundJobQueue`.
- **Cross-cutting validation** through a MediatR `ValidationBehavior` that runs FluentValidation validators before handlers.
- **Centralized error handling** — a `GlobalExceptionHandler` maps domain/application exceptions to ProblemDetails responses (400/403/404/409/500).

## Tech Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 8, ASP.NET Core Web API |
| CQRS / Mediation | MediatR 14 |
| Validation | FluentValidation 11 |
| ORM | EF Core 8 (SQL Server) |
| Identity | ASP.NET Core Identity (`IdentityRole<Guid>`) |
| Auth | JWT Bearer + cookie scheme for the Hangfire dashboard |
| Background jobs | Hangfire 1.8 (SQL Server storage) |
| API docs | Swashbuckle/Swagger with XML comments |
| Testing | xUnit, FluentAssertions, `WebApplicationFactory` (SQLite in-memory) |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express or full) — two databases are used: `CarCare` (app) and `CarCareHangfire` (job storage). Both are created/migrated automatically at startup.

### Configuration

Connection strings and seed credentials live in `Car Care/App.Api/appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<your-server>;Database=CarCare;Trusted_Connection=True;TrustServerCertificate=True",
    "HangfireConnection": "Server=<your-server>;Database=CarCareHangfire;Integrated Security=True;TrustServerCertificate=True"
  },
  "Jwt": { "Issuer": "CarCare", "Audience": "CarCare", "Key": "<min-32-char-secret>", "ExpiryMinutes": 60 },
  "Hangfire": { "Enabled": true }
}
```

> Change the JWT key before deploying anywhere real — the committed value is for local development only.

### Run

```bash
cd "Car Care"
dotnet run --project App.Api
```

- Swagger UI: `http://localhost:5247/swagger`
- Health check: `http://localhost:5247/health`
- Hangfire dashboard: `http://localhost:5247/hangfire` (Manager role required — see [Background Jobs](#background-jobs))

At startup the app applies EF migrations (or creates the schema), then seeds roles, staff accounts, and an initial service catalog.

### Seeded development accounts

| Role | Email | Password |
|---|---|---|
| Manager | `manager@carcare.local` | `Manager123!` |
| Staff | `staff@carcare.local` | `Staff123!` |

Customers are created through `POST /api/auth/register`.

## API Reference

Full descriptions, sample payloads, and response codes are available in Swagger.

### Auth

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Anonymous | Register a new customer |
| POST | `/api/auth/login` | Anonymous | Obtain a JWT access token |
| GET | `/api/auth/me` | Any role | Current user's profile |

### Vehicles

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/vehicles` | Customer | Add a vehicle to my garage |
| GET | `/api/vehicles/my` | Customer | List my vehicles |
| GET | `/api/vehicles/{id}` | Any role | Get a vehicle (owners only for customers) |
| GET | `/api/vehicles/{id}/service-history` | Any role | Service history for a vehicle |

### Services catalog

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/services` | Anonymous | List active services |
| POST | `/api/services` | Manager | Create a service |
| PUT | `/api/services/{id}` | Manager | Update a service |
| PATCH | `/api/services/{id}/deactivate` | Manager | Soft-delete a service |

### Appointments

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/appointments` | Customer | Book an appointment |
| GET | `/api/appointments/my` | Customer | List my appointments |
| DELETE | `/api/appointments/{id}` | Customer | Cancel my appointment |
| PUT | `/api/appointments/{id}/status` | Staff, Manager | Transition appointment status |

## Authentication & Authorization

- All endpoints (except register, login, and the public catalog) require a `Bearer` JWT. Click **Authorize** in Swagger and paste the `accessToken` from login.
- Three roles: **Customer**, **Staff**, **Manager**. Role checks are enforced with `[Authorize(Roles = ...)]`.
- Identity is always derived from token claims — no endpoint trusts a client-supplied user id (route ids are validated against ownership).
- The Hangfire dashboard uses a dual scheme: the Manager JWT works for programmatic access, and logging in as Manager in the browser also sets an HTTP-only cookie scoped to `/hangfire` only (API routes never accept the cookie).

## Appointment Lifecycle

```
Requested ──► Confirmed ──► InService ──► Completed
    │             │             │
    ▼             ▼             ▼
 Cancelled    Cancelled      NoShow
```

- Transitions are validated by a domain state machine; anything off-matrix returns `400`.
- `Completed`, `Cancelled`, and `NoShow` are terminal.
- Customers may cancel their own appointments only while not `InService`/`Completed`.
- A `Requested` appointment untouched for 24 hours is auto-cancelled by the cleanup job.

## Business Rules

| # | Rule | Enforcement |
|---|---|---|
| BR-01 | All operations require authentication | `[Authorize]` on every non-public endpoint |
| BR-02 | Customers access only their own vehicles/appointments | Ownership checks → `403` |
| BR-03 | Only active services can be booked | Validation at booking → `400` |
| BR-04 | Appointments must be in the future | Validator + handler double-check |
| BR-05 | No overlapping appointments per vehicle | Interval overlap check vs active statuses → `409` |
| BR-06 | Only the owner can cancel | `403` otherwise |
| BR-07 | No cancellation once InService/Completed | Domain exception → `400` |
| BR-08 | Strict status transition matrix | `AppointmentStatusMachine` → `400` |
| BR-09 | Only Staff/Manager elevate status | Role-based authorization |
| BR-10 | Prices snapshotted at booking | `AppointmentService` rows never mutated by catalog updates |
| BR-11 | Deactivated services immune in existing appointments | Soft delete flag only |
| BR-12 | Identity from claims, never from request body | Claims-based `ICurrentUser` |
| BR-13 | Self-registration is Customer-only; staff seeded | Role hardcoded at registration |
| BR-14 | Atomic conflict prevention under concurrency | Serializable transaction + `UPDLOCK, ROWLOCK, HOLDLOCK` |
| BR-15 | Stale requested appointments cancelled hourly | Hangfire recurring job (24h threshold) |

## Background Jobs

- **Hangfire** with SQL Server storage (`CarCareHangfire` database, schema auto-prepared).
- **Recurring job** `stale-appointment-cleanup` runs hourly and cancels stale `Requested` appointments.
- **Notification hooks** fire on booking/status change via `IBackgroundJobQueue` (enqueued safely — notification failures never roll back committed business transactions).
- **Dashboard** at `/hangfire`, protected by a custom `IDashboardAuthorizationFilter` requiring an authenticated **Manager** (verified: anonymous → `401`, staff → `403`, manager → `200`).
- Set `"Hangfire": { "Enabled": false }` to run without the dashboard/server (tests use a no-op job queue).

## Testing

```bash
cd "Car Care"
dotnet test
```

- **Integration tests** (`App.Tests/Api`) run the real pipeline via `WebApplicationFactory` against SQLite in-memory with a fake job queue: auth & catalog flows, full appointment workflow, diagnostics.
- **Domain tests** (`App.Tests/Domain`) cover the appointment state machine transition matrix.

## Project Structure

```
Car Care/
├── App.Api/                  # Web API: controllers, middleware, Hangfire dashboard auth, Program.cs
├── App.Application/          # CQRS features (commands/queries/validators/DTOs), abstractions, behaviors
├── App.Domain/               # Entities, enums, domain exceptions, status machine, role constants
├── App.Infrastructure/       # EF Core DbContext & configurations, Identity, JWT, Hangfire jobs,
│                             # repository/unit-of-work/lock implementations, migrations, seeding
└── App.Tests/                # xUnit integration + domain tests with shared test factory
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the code style guidelines (e.g., mandatory `CancellationToken` propagation through controllers and the MediatR pipeline).
