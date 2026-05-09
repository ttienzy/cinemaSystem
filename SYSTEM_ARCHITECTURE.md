# 🏗️ KIẾN TRÚC HỆ THỐNG CINEMA

## 📋 TỔNG HỢP TẤT CẢ URLs

### 🌐 Frontend
| Service | URL | Port | Description |
|---------|-----|------|-------------|
| **Cinema UI** | `http://localhost:5173` | 5173 | React + Vite Frontend |

### 🔐 Backend APIs
| Service | URL | Port | Description |
|---------|-----|------|-------------|
| **Gateway API** | `http://localhost:5200` | 5200 | API Gateway (Ocelot) |
| **Identity API** | `http://localhost:5216` | 5216 | Authentication & Authorization |
| **Cinema API** | `http://localhost:5073` | 5073 | Cinema & Hall Management |
| **Movie API** | `http://localhost:5153` | 5153 | Movie Management |
| **Booking API** | `http://localhost:5250` | 5250 | Booking & Seat Management |
| **Payment API** | `http://localhost:5145` | 5145 | Payment Processing |
| **Notification API** | `http://localhost:5112` | 5112 | Notification Service |

### 🔧 Infrastructure
| Service | URL | Port | Description |
|---------|-----|------|-------------|
| **SQL Server** | `localhost:11433` | 11433 | Database |
| **Redis** | `localhost:6379` | 6379 | Cache & SignalR Backplane |
| **RabbitMQ** | `localhost:5672` | 5672 | Message Broker |
| **RabbitMQ Management** | `http://localhost:15672` | 15672 | RabbitMQ UI |
| **Consul** | `http://localhost:8500` | 8500 | Service Discovery |

### 💳 External Services
| Service | URL | Description |
|---------|-----|-------------|
| **SePay Gateway** | `https://my.sepay.vn` | Payment Gateway |
| **SePay IPN** | `https://[ngrok-url]/api/payments/sepay/ipn` | IPN Callback |

---

## 🏛️ KIẾN TRÚC HỆ THỐNG

```
┌─────────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                          │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  Cinema UI (React + Vite) - Port 5173                        │  │
│  │  - Customer Portal                                            │  │
│  │  - Admin Dashboard                                            │  │
│  │  - SignalR Client (Real-time updates)                        │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                              ▲                                      │
│                              │ HTTP/HTTPS                           │
│                              │ WebSocket (SignalR)                  │
└──────────────────────────────┼─────────────────────────────────────┘
                               │
┌──────────────────────────────┼─────────────────────────────────────┐
│                         API GATEWAY LAYER                           │
│                              │                                      │
│  ┌──────────────────────────▼───────────────────────────────────┐  │
│  │  Gateway API (Ocelot) - Port 5200                            │  │
│  │  ┌────────────────────────────────────────────────────────┐  │  │
│  │  │  - Routing & Load Balancing                            │  │  │
│  │  │  - Authentication (JWT)                                 │  │  │
│  │  │  - Rate Limiting                                        │  │  │
│  │  │  - CORS Policy                                          │  │  │
│  │  │  - WebSocket Proxying (SignalR)                        │  │  │
│  │  └────────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                              │                                      │
└──────────────────────────────┼─────────────────────────────────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
┌───────▼────────┐  ┌─────────▼────────┐  ┌─────────▼────────┐
│                │  │                  │  │                  │
│  Identity API  │  │   Cinema API     │  │   Movie API      │
│  Port 5216     │  │   Port 5073      │  │   Port 5153      │
│                │  │                  │  │                  │
│ - JWT Auth     │  │ - Cinemas        │  │ - Movies         │
│ - User Mgmt    │  │ - Halls          │  │ - Genres         │
│ - Roles        │  │ - Seats          │  │ - Showtimes      │
│                │  │ - Showtimes      │  │                  │
└────────────────┘  └──────────────────┘  └──────────────────┘
        │                      │                      │
        │           ┌──────────┼──────────┐          │
        │           │          │          │          │
┌───────▼────────┐  │  ┌───────▼────────┐ │  ┌──────▼─────────┐
│                │  │  │                │ │  │                │
│  Booking API   │◄─┘  │  Payment API   │ │  │ Notification   │
│  Port 5250     │     │  Port 5145     │ │  │ API            │
│                │     │                │ │  │ Port 5112      │
│ - Bookings     │     │ - Payments     │ │  │                │
│ - Seat Lock    │     │ - SePay        │ │  │ - Email        │
│ - SignalR Hub  │     │ - IPN Handler  │ │  │ - SMS          │
│ - Email Send   │     │                │ │  │ - Push Notif   │
└────────┬───────┘     └────────┬───────┘ │  └────────────────┘
         │                      │         │
         │                      │         │
┌────────▼──────────────────────▼─────────▼──────────────────────┐
│                    MESSAGE BROKER LAYER                         │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  RabbitMQ - Port 5672                                     │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  Integration Events:                                │  │  │
│  │  │  - BookingCreatedIntegrationEvent                   │  │  │
│  │  │  - PaymentCompletedIntegrationEvent                 │  │  │
│  │  │  - PaymentFailedIntegrationEvent                    │  │  │
│  │  │  - BookingCancelledIntegrationEvent                 │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                           │
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │  SQL Server  │  │    Redis     │  │  Consul (Service Disc.)  │  │
│  │  Port 11433  │  │  Port 6379   │  │  Port 8500               │  │
│  │              │  │              │  │                          │  │
│  │ - Identity   │  │ - Cache      │  │ - Health Checks          │  │
│  │ - Cinema     │  │ - Seat Lock  │  │ - Service Registry       │  │
│  │ - Movie      │  │ - SignalR    │  │                          │  │
│  │ - Booking    │  │   Backplane  │  │                          │  │
│  │ - Payment    │  │              │  │                          │  │
│  └──────────────┘  └──────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      EXTERNAL SERVICES                              │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  SePay Payment Gateway                                        │  │
│  │  - Checkout URL: https://my.sepay.vn/userpage/paynow         │  │
│  │  - IPN Callback: https://[ngrok]/api/payments/sepay/ipn      │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 LUỒNG NGHIỆP VỤ CHÍNH

### 1️⃣ User Authentication Flow
```
User → Frontend → Gateway → Identity API
                              ↓
                         Generate JWT
                              ↓
                    Return Access Token
                              ↓
                    Store in localStorage
```

### 2️⃣ Booking Flow
```
User chọn ghế → Frontend → Gateway → Booking API
                                      ↓
                                 Lock seats (Redis)
                                      ↓
                                 Create booking
                                      ↓
                            Publish: BookingCreated
                                      ↓
                                  RabbitMQ
                                      ↓
                                  Payment API
                                      ↓
                              Create payment link
                                      ↓
                            Redirect to SePay
```

### 3️⃣ Payment Flow (IPN)
```
User thanh toán → SePay
                   ↓
        ┌──────────┴──────────┐
        │                     │
   (1) Redirect          (2) IPN Callback
        │                     │
        ▼                     ▼
    Frontend            Payment API
        │                     ↓
        │              Update payment
        │                     ↓
        │              Publish: PaymentCompleted
        │                     ↓
        │                 RabbitMQ
        │                     ↓
        │                Booking API
        │                     ↓
        │              ┌──────┴──────┐
        │              │             │
        │         Confirm      Send Email
        │         Booking      (Async)
        │              │             │
        │         SignalR Push       │
        │              │             │
        └──────────────┘             │
        │                            │
    Hiển thị                    SMTP Server
    "Thành công"
```

### 4️⃣ Real-time Seat Updates (SignalR)
```
User A chọn ghế → Booking API
                      ↓
                 Lock seat (Redis)
                      ↓
              SignalR Hub Broadcast
                      ↓
        ┌─────────────┴─────────────┐
        ▼                           ▼
    User B                      User C
    (Ghế bị disable)           (Ghế bị disable)
```

---

## 🔐 AUTHENTICATION & AUTHORIZATION

### JWT Token Flow
```
1. User login → Identity API
2. Identity API validates credentials
3. Generate JWT with claims:
   - UserId
   - Email
   - Roles (Admin, Customer)
4. Return JWT to Frontend
5. Frontend stores in localStorage
6. Every request includes: Authorization: Bearer {token}
7. Gateway validates JWT
8. Forward to microservices
```

### JWT Configuration
- **Issuer**: `CinemaSystem.Identity`
- **Audience**: `CinemaSystem.Clients`
- **Secret Key**: Configured in appsettings.json
- **Expiry**: 60 minutes
- **Algorithm**: HS256

---

## 📡 COMMUNICATION PATTERNS

### 1. Synchronous (HTTP/REST)
- Frontend ↔ Gateway ↔ APIs
- API ↔ API (External Service Calls)

### 2. Asynchronous (Message Broker)
- **RabbitMQ** for inter-service communication
- **Event-driven architecture**
- **Publish/Subscribe pattern**

### 3. Real-time (SignalR)
- **WebSocket** for bi-directional communication
- **Redis backplane** for scaling
- **Group-based broadcasting**

---

## 🗄️ DATABASE ARCHITECTURE

### Database per Service Pattern
```
┌─────────────────┐
│  SQL Server     │
│  Port 11433     │
├─────────────────┤
│ Db_Identity     │ ← Identity API
│ CinemaDb        │ ← Cinema API
│ MovieDb         │ ← Movie API
│ BookingDb       │ ← Booking API
│ PaymentDb       │ ← Payment API
└─────────────────┘
```

---

## 🚀 DEPLOYMENT PORTS SUMMARY

| Component | Development | Production (Docker) |
|-----------|-------------|---------------------|
| Frontend | 5173 | 80 |
| Gateway | 5200 | 7100 |
| Identity | 5216 | 7012 |
| Cinema | 5073 | 7251 |
| Movie | 5153 | 7295 |
| Booking | 5250 | 7043 |
| Payment | 5145 | 7252 |
| Notification | 5112 | 7147 |
| SQL Server | 11433 | 11433 |
| Redis | 6379 | 6379 |
| RabbitMQ | 5672 | 5672 |
| RabbitMQ UI | 15672 | 15672 |
| Consul | 8500 | 8500 |

---

## 🔧 TECHNOLOGY STACK

### Frontend
- **React 19** - UI Framework
- **TypeScript** - Type Safety
- **Vite** - Build Tool
- **Ant Design** - UI Components
- **React Query** - Data Fetching
- **SignalR Client** - Real-time
- **Axios** - HTTP Client
- **Zustand** - State Management

### Backend
- **.NET 8** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **Ocelot** - API Gateway
- **RabbitMQ** - Message Broker
- **SignalR** - Real-time Communication
- **Redis** - Caching & Backplane
- **SQL Server** - Database
- **Consul** - Service Discovery

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Orchestration
- **Ngrok** - Tunneling (IPN)

---

## 📊 SCALABILITY CONSIDERATIONS

### Horizontal Scaling
- ✅ Stateless APIs (can scale horizontally)
- ✅ Redis for distributed caching
- ✅ Redis backplane for SignalR scaling
- ✅ RabbitMQ for async processing
- ✅ Database per service (independent scaling)

### Performance Optimization
- ✅ API Gateway caching
- ✅ Redis caching for frequently accessed data
- ✅ SignalR for real-time updates (no polling)
- ✅ Async message processing (RabbitMQ)
- ✅ Connection pooling (SQL Server, Redis)

---

## 🔒 SECURITY

### API Security
- ✅ JWT Authentication
- ✅ Role-based Authorization
- ✅ HTTPS/TLS
- ✅ CORS Policy
- ✅ Rate Limiting (Gateway)
- ✅ Input Validation
- ✅ SQL Injection Prevention (EF Core)

### Payment Security
- ✅ IPN Secret Key Validation
- ✅ HTTPS for payment callbacks
- ✅ Idempotent payment processing
- ✅ Transaction logging

---

## 📈 MONITORING & LOGGING

### Health Checks
- All APIs expose `/health` endpoint
- Consul monitors service health
- Auto-registration with Consul

### Logging
- Structured logging (Serilog)
- Log levels: Information, Warning, Error
- Correlation IDs for tracing

---

## 🎯 FUTURE ENHANCEMENTS

1. **Kubernetes** - Container orchestration
2. **Elasticsearch** - Centralized logging
3. **Prometheus + Grafana** - Metrics & monitoring
4. **API Versioning** - Backward compatibility
5. **Circuit Breaker** - Resilience (Polly)
6. **Distributed Tracing** - OpenTelemetry
7. **CDN** - Static asset delivery
8. **Load Balancer** - Nginx/HAProxy
