# 🎬 Cinema Booking System

A modern, full-stack Cinema Management and Booking System built with **Clean Architecture**, **CQRS**.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## � Overview

A comprehensive cinema management solution featuring real-time seat booking, payment integration, inventory management, and multi-role access control. Built with modern technologies and best practices for scalability and maintainability.

## ✨ Key Features

### 🎯 Core Functionality

- **🎬 Movie Management**: Advanced catalog with search, filters, and categorization
- **🕒 Dynamic Scheduling**: Flexible showtime management with pricing tiers
- **🎫 Real-time Booking**: Distributed seat locking with Redis and SignalR
- **💳 Payment Gateway**: Integrated VnPay payment processing
- **🪑 Smart Seating**: Support for couple seats, blocking, and maintenance
- **🍿 POS System**: Counter sales for concessions with inventory tracking

### 🔐 Security & Access Control

- **Multi-role Authentication**: Admin, Manager, Staff, Customer roles
- **JWT Bearer Tokens**: Secure API authentication with refresh tokens
- **Role-based Authorization**: Granular permission control
- **ASP.NET Core Identity**: Robust user management

### 📊 Management Features

- **📦 Inventory System**: Real-time stock tracking with low-stock alerts
- **👥 Staff Management**: Shift scheduling and work assignments
- **🎟️ Refund Processing**: Tiered refund policies with approval workflow
- **📈 Analytics Dashboard**: Revenue tracking and performance metrics
- **🔧 Equipment Maintenance**: Maintenance logging and scheduling

### 🏗️ Technical Excellence

- **Clean Architecture**: Strict layer separation (Domain → Application → Infrastructure → API)
- **CQRS Pattern**: Command/Query separation with MediatR
- **Domain Events**: Event-driven architecture for loose coupling
- **Repository Pattern**: Abstracted data access
- **Unit of Work**: Transaction management

## �️ Technology Stack

### Backend

- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022
- **Cache**: Redis 7 (StackExchange.Redis)
- **Real-time**: SignalR with Redis backplane
- **Validation**: FluentValidation
- **Mapping**: Manual DTOs for performance
- **Patterns**: CQRS (MediatR), Repository, Unit of Work

### Frontend

- **Framework**: React 19
- **Build Tool**: Vite 7
- **Language**: TypeScript 5.9
- **UI Library**: Ant Design 6
- **State Management**: Zustand 5
- **Data Fetching**: TanStack Query (React Query) 5
- **Forms**: React Hook Form 7 + Zod 4
- **HTTP Client**: Axios 1.13
- **Routing**: React Router 7

### Infrastructure

- **Containerization**: Docker & Docker Compose
- **Reverse Proxy**: Nginx (production)
- **Email Testing**: Mailhog (development)
- **Orchestration**: Docker Compose with profiles

## 📁 Project Structure

```
cinemaSystem/
├── Api/                          # 🌐 Web API Layer
│   ├── Controllers/              # API endpoints
│   ├── Middleware/               # Exception handlers
│   └── Program.cs                # Application entry point
│
├── Application/                  # 💼 Application Layer
│   ├── Features/                 # CQRS Commands & Queries
│   │   ├── Bookings/
│   │   ├── Movies/
│   │   ├── Showtimes/
│   │   └── ...
│   ├── Common/
│   │   ├── Behaviors/            # MediatR pipelines
│   │   ├── Interfaces/           # Abstractions
│   │   └── Exceptions/           # Application exceptions
│   └── Settings/                 # Configuration models
│
├── Domain/                       # 🎯 Domain Layer
│   ├── Entities/                 # Aggregates & Entities
│   │   ├── BookingAggregate/
│   │   ├── ShowtimeAggregate/
│   │   ├── CinemaAggregate/
│   │   └── ...
│   ├── Events/                   # Domain events
│   ├── Services/                 # Domain services
│   └── Common/                   # Base classes, value objects
│
├── Infrastructure/               # 🔧 Infrastructure Layer
│   ├── Persistence/              # EF Core, Repositories
│   ├── Identity/                 # Authentication & Authorization
│   ├── Redis/                    # Caching & Locking
│   ├── Hubs/                     # SignalR hubs
│   ├── Payments/                 # Payment gateway integration
│   └── Email/                    # Email service
│
├── Shared/                       # 📦 Shared Layer
│   └── Models/                   # DTOs, Response models
│
├── Web/cinema-ui/                # ⚛️ React Frontend
│   ├── src/
│   │   ├── features/             # Feature modules
│   │   ├── shared/               # Shared components & utilities
│   │   ├── pages/                # Page components
│   │   └── App.tsx
│   ├── Dockerfile                # Frontend container
│   └── nginx.conf                # Nginx configuration
│
├── nginx/                        # 🔀 Reverse Proxy Config
│   ├── nginx.conf
│   └── conf.d/
│
├── scripts/                      # 📜 Utility Scripts
│   ├── init-db.sql
│   └── health-check.sh
│
├── docker-compose.yml            # 🐳 Main orchestration
├── docker-compose.dev.yml        # Development overrides
├── Dockerfile                    # Backend container
├── Makefile                      # Docker commands
└── README.md                     # This file
```

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) 20.10+
- [Docker Compose](https://docs.docker.com/compose/) 2.0+
- 8GB RAM minimum
- 20GB free disk space

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/your-username/cinemaSystem.git
   cd cinemaSystem
   ```

2. **Setup environment**:

   ```bash
   cp .env.example .env
   # Edit .env if needed
   ```

3. **Start all services**:

   ```bash
   # Using Makefile (recommended)
   make install

   # Or using docker-compose directly
   docker-compose up -d --build
   ```

4. **Wait for services to be ready** (30-60 seconds):

   ```bash
   make health
   ```

5. **Access the application**:
   - 🌐 **Frontend**: http://localhost:3000
   - 🔧 **API/Swagger**: http://localhost:8080/swagger
   - 📧 **Mailhog**: http://localhost:8025
   - 💾 **SQL Server**: localhost:1433
   - 🔴 **Redis**: localhost:6379

### Default Credentials

**Admin Account**:

- Email: `admin@cinema.com`
- Password: `Admin@Cinema2024!`

**Database**:

- Server: `localhost:1433`
- User: `sa`
- Password: `ComplexPassword123!` (configurable in `.env`)

## 🎮 Usage

### Common Commands

```bash
# Start services
make up

# Stop services
make down

# View logs
make logs

# Check health
make health

# Restart services
make restart

# Database backup
make db-backup

# Database restore
make db-restore

# Clean up everything
make clean
```

### Development Mode

Start with hot reload support:

```bash
make dev
```

- Frontend: http://localhost:5173 (Vite dev server)
- API: http://localhost:8080 (with hot reload)

### Production Mode

Start with Nginx reverse proxy:

```bash
make prod
```

- Access via: http://localhost (port 80)

### Management Tools

Start with Redis Commander:

```bash
make tools
```

- Redis Commander: http://localhost:8081

## 📚 Documentation

- **[Quick Start Guide](QUICKSTART.md)** - Get running in 5 minutes
- **[Docker Guide](README.Docker.md)** - Complete Docker documentation
- **[Docker Setup Summary](DOCKER_SETUP_SUMMARY.md)** - Technical details
- **[Implementation Notes](IMPLEMENTATION_NOTES.md)** - Recent fixes and improvements

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                     Presentation                         │
│              (Api Controllers, Frontend)                 │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                    Application                           │
│         (Use Cases, Commands, Queries, DTOs)            │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                      Domain                              │
│    (Entities, Value Objects, Domain Events, Rules)      │
└────────────────────▲────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│                  Infrastructure                          │
│  (EF Core, Redis, SignalR, Email, Payment Gateway)      │
└─────────────────────────────────────────────────────────┘
```

### Service Architecture

```
                    ┌─────────────┐
                    │   Nginx     │ (Production)
                    │   Port 80   │
                    └──────┬──────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼─────┐      ┌────▼─────┐      ┌────▼─────┐
   │ Frontend │      │   API    │      │ SignalR  │
   │  React   │      │ ASP.NET  │      │   Hub    │
   │ Port 3000│      │ Port 8080│      │          │
   └──────────┘      └────┬─────┘      └──────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
   ┌────▼─────┐     ┌────▼─────┐     ┌────▼─────┐
   │   SQL    │     │  Redis   │     │ Mailhog  │
   │  Server  │     │  Cache   │     │  Email   │
   │ Port 1433│     │ Port 6379│     │ Port 8025│
   └──────────┘     └──────────┘     └──────────┘
```

## 🔧 Configuration

### Environment Variables

Key configurations in `.env`:

```env
# Database
SA_PASSWORD=ComplexPassword123!

# JWT
JWT_SECRETKEY=your-secret-key-here
JWT_ACCESSTOKENEXPIRATION=20
JWT_REFRESHTOKENEXPIRATION=7

# VnPay Payment Gateway
VNPAY_TMNCODE=your-tmn-code
VNPAY_HASHSECRET=your-hash-secret

# SMTP (Mailhog for development)
SMTP_SERVER=mailhog
SMTP_PORT=1025

# Admin Seed
IDENTITY_DEFAULT_ADMIN_EMAIL=admin@cinema.com
IDENTITY_DEFAULT_ADMIN_PASSWORD=Admin@Cinema2024!
```

### Connection Strings

Automatically configured in Docker:

```
Booking DB: Server=sqlserver;Database=Booking;User=sa;Password=${SA_PASSWORD}
Identity DB: Server=sqlserver;Database=Identity;User=sa;Password=${SA_PASSWORD}
Redis: redis:6379
```

## 🧪 API Documentation

### Swagger UI

Access interactive API documentation at:

- **Development**: http://localhost:8080/swagger
- **Production**: http://localhost/swagger

### Key Endpoints

**Authentication**:

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Logout

**Movies**:

- `GET /api/movies` - List movies
- `GET /api/movies/{id}` - Movie details
- `GET /api/movies/now-showing` - Current movies
- `GET /api/movies/coming-soon` - Upcoming movies

**Showtimes**:

- `GET /api/showtimes/movie/{movieId}` - Showtimes by movie
- `GET /api/showtimes/cinema/{cinemaId}` - Showtimes by cinema
- `GET /api/showtimes/{id}/seating-plan` - Seat availability

**Bookings**:

- `POST /api/bookings` - Create booking
- `GET /api/bookings/my` - My bookings
- `POST /api/bookings/{id}/cancel` - Cancel booking
- `GET /api/bookings/callback` - Payment callback

**Admin** (requires Admin role):

- `GET /api/admin/users` - Manage users
- `POST /api/admin/movies` - Create movie
- `POST /api/admin/cinemas` - Create cinema
- `GET /api/admin/dashboard/stats` - Dashboard statistics

## 🧪 Testing

### Run Tests

```bash
# In Docker
docker-compose exec api dotnet test

# Locally
dotnet test
```

### Health Checks

```bash
# All services
make health

# Individual checks
curl http://localhost:8080/swagger/index.html  # API
curl http://localhost:3000                      # Frontend
curl http://localhost:8025                      # Mailhog
```

## 🐛 Troubleshooting

### Services won't start

```bash
# Check logs
make logs

# Rebuild
make down
make build
make up
```

### Port conflicts

```bash
# Check ports in use
netstat -ano | findstr :8080
netstat -ano | findstr :3000

# Change ports in docker-compose.yml
```

### Database connection issues

```bash
# Restart SQL Server
docker-compose restart sqlserver

# Check SQL Server health
docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "SELECT 1" -C
```

### Frontend build fails

```bash
# Rebuild without cache
docker-compose build --no-cache frontend
```

See [README.Docker.md](README.Docker.md) for more troubleshooting tips.

## 📊 Monitoring

### View Logs

```bash
make logs              # All services
make logs-api          # API only
make logs-frontend     # Frontend only
make logs-db           # Database only
```

### Resource Usage

```bash
docker stats
```

### Container Details

```bash
docker inspect cinema_api
docker inspect cinema_sqlserver
```

## 🚢 Deployment

### Production Deployment

1. **Update environment variables** for production
2. **Enable HTTPS** with SSL certificates
3. **Start with production profile**:
   ```bash
   docker-compose --profile production up -d
   ```

### Cloud Deployment

The system is cloud-ready and can be deployed to:

- **Azure Container Apps**
- **AWS ECS/Fargate**
- **Google Cloud Run**
- **Kubernetes** (K8s manifests can be generated)

## 🔐 Security

- ✅ JWT Bearer authentication
- ✅ Role-based authorization
- ✅ Password hashing (ASP.NET Core Identity)
- ✅ HTTPS support (production)
- ✅ CORS configuration
- ✅ SQL injection prevention (EF Core)
- ✅ XSS protection headers
- ✅ Rate limiting (Nginx)
- ✅ Non-root Docker containers

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Team

Created with ❤️ by the Cinema System Development Team

## 🙏 Acknowledgments

- ASP.NET Core Team
- React Team
- Docker Community
- Open Source Contributors

## 📞 Support

- **Documentation**: See docs folder
- **Issues**: GitHub Issues
- **Email**: support@cinema-system.com

---

**Version**: 1.0.0  
**Last Updated**: 2024  
**Status**: Production Ready ✅
