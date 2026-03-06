# Cinema System - Technical Overview

## Tổng quan

Dự án hệ thống rạp chiếu phim với kiến trúc **Clean Architecture**, sử dụng **CQRS Pattern** thông qua MediatR.

---

## Backend (API)

| Công nghệ                     | Mô tả                              |
| ----------------------------- | ---------------------------------- |
| **ASP.NET Core 8.0**          | Framework web server               |
| **Entity Framework Core 8.0** | ORM cho SQL Server                 |
| **MediatR 12.4.1**            | CQRS pattern - xử lý command/query |
| **FluentValidation 11.3**     | Validation input                   |
| **JWT Bearer**                | Authentication/Authorization       |
| **SQL Server 2022**           | Database chính                     |
| **Redis 7**                   | Cache & Seat locking               |
| **Swashbuckle**               | Swagger API documentation          |
| **Ardalis Specification**     | Repository pattern                 |

### Project Structure (Backend)

```
Api/           - Web API Controllers
Application/   - Business Logic (Commands, Queries, Handlers)
Domain/        - Entities, Value Objects
Infrastructure/ - Database, External Services
Shared/        - Common utilities
```

---

## Frontend (Web)

| Công nghệ          | Mô tả                |
| ------------------ | -------------------- |
| **React 19**       | UI Framework         |
| **Ant Design 6.3** | UI Component Library |
| **TypeScript 5.9** | Type safety          |
| **Vite 7**         | Build tool           |
| **ESLint**         | Code linting         |

---

## Infrastructure & DevOps

| Công nghệ          | Mô tả                       |
| ------------------ | --------------------------- |
| **Docker**         | Containerization            |
| **Docker Compose** | Multi-service orchestration |
| **Mailhog**        | Email testing (dev)         |

### Docker Services

- `api` - ASP.NET Core Web API
- `sqlserver` - SQL Server 2022
- `redis` - Redis Cache
- `mailhog` - SMTP testing UI

---

## Payment Integration

- **VNPay** - Payment gateway (sandbox)

---

## Architecture Highlights

- **Clean Architecture** - Tách biệt rõ ràng các layer
- **CQRS** - Command Query Responsibility Segregation qua MediatR
- **Specification Pattern** - Repository abstraction
- **JWT Token** - Access + Refresh token
- **Redis-based Seat Locking** - Concurrent booking handling
