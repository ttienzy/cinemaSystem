# Cinema Booking System

A modern, high-performance Cinema Management and Booking System built with **.NET 8**, using **Clean Architecture**, **CQRS (MediatR)**, and **Domain-Driven Design (DDD)** principles.

## 🚀 Key Features

- **🎬 Movie Management**: Advanced movie catalog with pagination, search, and categorization.
- **🕒 Showtime Management**: Dynamic showtime scheduling with multiplier-based pricing.
- **🎫 Real-time Booking**: Distributed seat locking using **Redis** and real-time updates via **SignalR**.
- **💳 Payment Integration**: Secure payment processing with **VnPay** (Sandbox support).
- **🎟️ Advanced Seating**: Support for Couple/Double seats, maintenance blocking, and tiered refund policies.
- **🔐 Security**: Robust identity management with **ASP.NET Core Identity** and **JWT** Bearer authentication.
- **🏢 Clean Architecture**: Strict separation of concerns between Domain, Application, Infrastructure, and Api layers.

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: Entity Framework Core with SQL Server
- **Caching & Locks**: Redis (StackExchange.Redis)
- **Real-time**: SignalR with Redis Backplane
- **Communication**: MediatR for CQRS
- **Validation**: FluentValidation
- **Logging**: Structured Console Logging
- **Containerization**: Docker & Docker Compose

## 🏗️ Project Structure

```text
├── Api            # Entry point, Controllers, Middlewares
├── Application    # Business Logic, Commands, Queries, Interfaces
├── Domain         # Entities, Value Objects, Domain Events, Domain Services
├── Infrastructure # Persistence, Redis, Hubs, External Services
└── Shared         # DTOs, Common Models, Constants
```

## 🚦 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or use Docker)

### Installation & Run

1. **Clone the repository**:

   ```bash
   git clone https://github.com/your-username/cinemaSystem.git
   cd cinemaSystem
   ```

2. **Setup Environment**:
   - Copy `.env.example` to `.env`.
   - Update connection strings in `appsettings.json` or `.env`.

3. **Run with Docker Compose**:

   ```bash
   docker-compose up -d
   ```

4. **Run via .NET CLI**:
   ```bash
   dotnet restore
   dotnet run --project Api/Api.csproj
   ```

## 🧪 API Documentation

The project includes **Swagger UI** for easy API exploration and testing.
Once running, navigate to:

- `http://localhost:5190/swagger` (Local)
- `http://localhost:8080/swagger` (Docker)

## 🐳 Docker Deployment

The solution is fully containerized. Use the provided `Dockerfile` and `docker-compose.yml` for rapid deployment of the API, SQL Server, Redis, and Adminer.

---

_Created with ❤️ by the Architecture Team._
