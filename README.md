# Cinema System

Cinema System is a microservices-based movie ticket booking application built with .NET Aspire. The current local flow uses an Aspire AppHost, PostgreSQL, a YARP Gateway, JWT authentication, Cloudinary for movie posters, and SePay payment callbacks through ngrok.

## Architecture

![Cinema System Architecture](docs/images/cinemas_architecture.png)

## Services

| Project | Responsibility |
|---|---|
| `Cys.AppHost` | Aspire orchestration for services, PostgreSQL, Gateway, and ngrok |
| `Gateway` | YARP API Gateway |
| `Identity.API` | Registration, login, JWT authentication, users, passkeys |
| `Cinema.API` | Cinema, hall, and seat management |
| `Movie.API` | Movie, genre, showtime, and Cloudinary poster management |
| `Booking.API` | Booking, seat locking, ticket operations, dashboards |
| `Payment.API` | Payment creation, checkout, and SePay IPN handling |
| `*.API.Client` | Typed HTTP clients; defaults point to `gateway` |
| `Cys.ServiceDefaults` | Shared Aspire service defaults |
| `Cinema.UI` | React frontend |

## Current Notes

- Gateway routes are configured in `Gateway/appsettings.json`.
- API clients call `https+http://gateway` by default.
- PostgreSQL uses the Docker volume `cinema-postgres-data`, so database files survive AppHost restarts.
- Redis, RabbitMQ/MassTransit, SignalR hubs, and background event publishers are currently disabled in Booking/Payment refactor paths.
- Booking uses in-memory seat locks for now. This is OK for local/single instance, but not for multi-instance production.
- ngrok is managed by AppHost as `ngrok-gateway` and forwards to Gateway, not directly to Payment.

## Prerequisites

- .NET SDK matching the solution target, currently `net10.0`
- Docker Desktop, for Aspire-managed PostgreSQL
- `dotnet-ef`
- ngrok, only if testing SePay IPN locally

Install `dotnet-ef` if needed:

```powershell
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

## AppHost Configuration

AppHost parameters are defined in `Cys.AppHost/AppHost.cs`:

- `jwt-key`
- `jwt-issuer`
- `jwt-audience`
- `cloudinary-cloud-name`
- `cloudinary-api-key`
- `cloudinary-api-secret`
- `sepay-merchant-id`
- `sepay-secret-key`

For local development, store secrets with AppHost user-secrets:

```powershell
dotnet user-secrets set "Parameters:jwt-key" "<base64-encoded-32-byte-key>" --project Cys.AppHost
dotnet user-secrets set "Parameters:jwt-issuer" "CinemaSystem" --project Cys.AppHost
dotnet user-secrets set "Parameters:jwt-audience" "CinemaSystem.Client" --project Cys.AppHost

dotnet user-secrets set "Parameters:cloudinary-cloud-name" "<cloud-name>" --project Cys.AppHost
dotnet user-secrets set "Parameters:cloudinary-api-key" "<api-key>" --project Cys.AppHost
dotnet user-secrets set "Parameters:cloudinary-api-secret" "<api-secret>" --project Cys.AppHost

dotnet user-secrets set "Parameters:sepay-merchant-id" "<merchant-id>" --project Cys.AppHost
dotnet user-secrets set "Parameters:sepay-secret-key" "<secret-key>" --project Cys.AppHost
```

Optional static ngrok domain:

```powershell
dotnet user-secrets set "Ngrok:Domain" "your-domain.ngrok-free.app" --project Cys.AppHost
```

## Entity Framework Commands

All design-time factories support these environment variables:

- `ConnectionStrings__identitydb`
- `ConnectionStrings__cinemadb`
- `ConnectionStrings__moviedb`
- `ConnectionStrings__bookingdb`
- `ConnectionStrings__paymentdb`

If not set, they fall back to:

```text
Host=localhost;Database=<db-name>;Username=postgres;Password=postgres
```

If your PostgreSQL uses another port/password, set connection strings first:

```powershell
$env:ConnectionStrings__identitydb = "Host=localhost;Port=5432;Database=identitydb;Username=postgres;Password=postgres"
$env:ConnectionStrings__cinemadb = "Host=localhost;Port=5432;Database=cinemadb;Username=postgres;Password=postgres"
$env:ConnectionStrings__moviedb = "Host=localhost;Port=5432;Database=moviedb;Username=postgres;Password=postgres"
$env:ConnectionStrings__bookingdb = "Host=localhost;Port=5432;Database=bookingdb;Username=postgres;Password=postgres"
$env:ConnectionStrings__paymentdb = "Host=localhost;Port=5432;Database=paymentdb;Username=postgres;Password=postgres"
```

Create migrations:

```powershell
dotnet ef migrations add InitialCreate --project Identity.API --startup-project Identity.API --context IdentityDbContext --output-dir Data/Migrations
dotnet ef migrations add InitialCreate --project Cinema.API --startup-project Cinema.API --context CinemaDbContext --output-dir Data/Migrations
dotnet ef migrations add InitialCreate --project Movie.API --startup-project Movie.API --context MovieDbContext --output-dir Data/Migrations
dotnet ef migrations add InitialCreate --project Booking.API --startup-project Booking.API --context BookingDbContext --output-dir Data/Migrations
dotnet ef migrations add InitialCreate --project Payment.API --startup-project Payment.API --context PaymentDbContext --output-dir Data/Migrations
```

Apply migrations:

```powershell
dotnet ef database update --project Identity.API --startup-project Identity.API --context IdentityDbContext
dotnet ef database update --project Cinema.API --startup-project Cinema.API --context CinemaDbContext
dotnet ef database update --project Movie.API --startup-project Movie.API --context MovieDbContext
dotnet ef database update --project Booking.API --startup-project Booking.API --context BookingDbContext
dotnet ef database update --project Payment.API --startup-project Payment.API --context PaymentDbContext
```

Useful remove commands if a new migration has not been applied yet:

```powershell
dotnet ef migrations remove --project Identity.API --startup-project Identity.API --context IdentityDbContext
dotnet ef migrations remove --project Cinema.API --startup-project Cinema.API --context CinemaDbContext
dotnet ef migrations remove --project Movie.API --startup-project Movie.API --context MovieDbContext
dotnet ef migrations remove --project Booking.API --startup-project Booking.API --context BookingDbContext
dotnet ef migrations remove --project Payment.API --startup-project Payment.API --context PaymentDbContext
```

## Run AppHost

```powershell
dotnet run --project Cys.AppHost
```

AppHost starts:

- PostgreSQL with persistent data volume `cinema-postgres-data`
- databases: `identitydb`, `cinemadb`, `moviedb`, `bookingdb`, `paymentdb`
- `identity`
- `cinema`
- `movie`
- `booking`
- `payment`
- `gateway`
- `ngrok-gateway`

Gateway local URLs from launch settings:

- HTTPS: `https://localhost:55000`
- HTTP: `http://localhost:55001`

## SePay And Ngrok

ngrok should forward to Gateway. The IPN URL should use the Gateway route:

```text
https://<ngrok-domain>/api/payments/sepay/ipn
```

`Gateway/appsettings.json` supports both:

- `/api/v1/payments/sepay/ipn`
- `/api/payments/sepay/ipn`

The fallback script also forwards to Gateway:

```powershell
.\scripts\start-ngrok-payment.ps1
```

## Run Frontend

```powershell
cd Cinema.UI
npm install
npm run dev
```

Frontend URL:

```text
http://localhost:5173
```

## Demo Accounts

Identity seeding creates local users/roles when the service starts.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@123.local` | `Admin123!` |
| Customer | `demo@123.local` | `Demo123!` |

## Verification

Before running the full AppHost, these builds should pass:

```powershell
dotnet build Gateway/Gateway.csproj
dotnet build Identity.API/Identity.API.csproj
dotnet build Cinema.API/Cinema.API.csproj
dotnet build Movie.API/Movie.API.csproj
dotnet build Booking.API/Booking.API.csproj
dotnet build Payment.API/Payment.API.csproj
dotnet build Cys.AppHost/Cys.AppHost.csproj
```

## Notes

- Do not commit real secrets in `appsettings.json`, `appsettings.Development.json`, or user-specific scripts.
- If `dotnet ef database update` cannot connect, confirm which PostgreSQL port Aspire allocated in the Aspire dashboard, then set the matching `ConnectionStrings__...` environment variable.
- All API database owners now call `MigrateAsync` on startup. Identity/Cinema/Movie also seed demo data, but seeders skip insertion when existing data is already present.
