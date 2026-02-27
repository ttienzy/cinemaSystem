# Cinema Booking System

A modern, high-performance Cinema Management and Booking System built with **.NET 8**, using **Clean Architecture**, **CQRS (MediatR)**, and **Domain-Driven Design (DDD)** principles.

## 🚀 Key Features

- **🎬 Movie Management**: Advanced movie catalog with pagination, search, and categorization.
- **🕒 Showtime Management**: Dynamic showtime scheduling with multiplier-based pricing.
- **🎫 Real-time Booking**: Distributed seat locking using **Redis** and real-time updates via **SignalR**.
- **💳 Payment Integration**: Secure payment processing with **VnPay** (Sandbox support).
- **🎟️ Advanced Seating**: Support for Couple/Double seats, maintenance blocking, and tiered refund policies.
- **🍿 Concession Management**: POS-ready counter sales for popcorn and snacks.
- **📦 Inventory Management**: Real-time stock tracking with automated deduction on sales and low-stock alerts.
- **🔐 Security**: Robust identity management with **ASP.NET Core Identity** and **JWT** Bearer authentication.
- **Logging & Observability**: Full OpenTelemetry integration for Distributed Tracing, Metrics, and Structured Logging.
- **🏗️ Orchestration**: Managed by **.NET Aspire** for local development and cloud-ready deployment.
- **🏢 Clean Architecture**: Strict separation of concerns between Domain, Application, Infrastructure, and Api layers.

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Orchestration**: .NET Aspire (Dashboard, Service Discovery)
- **Database**: Entity Framework Core with SQL Server
- **Caching & Locks**: Redis (StackExchange.Redis)
- **Real-time**: SignalR with Redis Backplane
- **Observability**: OpenTelemetry
- **Communication**: MediatR for CQRS
- **Validation**: FluentValidation

## 🏗️ Project Structure

```text
├── Api            # Main Entry Point, Controllers
├── Application    # Business Logic, Commands, Queries
├── Domain         # Entities, Value Objects, Domain Events
├── Infrastructure # Persistence, Redis, Hubs, Mail Service
├── Shared         # DTOs, Common Models
├── AppHost        # .NET Aspire Orchestrator (Infrastructure Resource Management)
├── Migrations     # Dedicated Migration & Seeding Worker
└── ServiceDefaults# Shared OTel and Health Check configurations
```

## 🚦 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [.NET Aspire Workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installation & Run

1. **Clone the repository**:

   ```bash
   git clone https://github.com/your-username/cinemaSystem.git
   cd cinemaSystem
   ```

2. **Setup Secrets** (Optional but recommended):

   ```bash
   cd cinemaSystem.AppHost
   dotnet user-secrets set "Parameters:sql-password" "YourStrongPassword123!"
   ```

3. **Run with .NET Aspire**:

   ```bash
   dotnet run --project cinemaSystem.AppHost
   ```

   _This will launch the Aspire Dashboard at https://localhost:18888, where you can monitor logs, traces, and metrics for SQL Server, Redis, Migrations, and the API._

## 🧪 API Documentation

The project includes **Swagger UI** for easy API exploration and testing.
The easiest way to access it is through the **Aspire Dashboard** by clicking the endpoint link for `cinema-api`.

## 🐳 Deployment

The solution is cloud-ready via the **Aspire Manifest**. Use `azd init` and `azd up` to deploy the entire infrastructure to Azure Container Apps or other platforms.

---

_Created with ❤️ by the Architecture Team._
