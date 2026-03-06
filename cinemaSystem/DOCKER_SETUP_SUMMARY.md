# 🐳 Docker Setup Summary - Cinema Booking System

## 📁 Files Created

### Core Docker Files
1. **docker-compose.yml** - Main orchestration file with all services
2. **docker-compose.dev.yml** - Development overrides with hot reload
3. **Dockerfile** - Backend API container (already existed, kept as is)
4. **Web/cinema-ui/Dockerfile** - Frontend React container (NEW)
5. **.env** - Environment variables
6. **.dockerignore** - Docker build exclusions (already existed)
7. **Web/cinema-ui/.dockerignore** - Frontend build exclusions (NEW)

### Nginx Configuration
8. **nginx/nginx.conf** - Main Nginx configuration
9. **nginx/conf.d/cinema.conf** - Cinema app server configuration
10. **Web/cinema-ui/nginx.conf** - Frontend Nginx configuration

### Automation & Scripts
11. **Makefile** - Docker management commands
12. **scripts/init-db.sql** - Database initialization
13. **scripts/health-check.sh** - Health check script

### Documentation
14. **README.Docker.md** - Complete Docker deployment guide
15. **.gitignore** - Git exclusions

## 🎯 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Nginx (Port 80)                      │
│                    (Production Profile)                      │
└────────────┬────────────────────────────┬───────────────────┘
             │                            │
    ┌────────▼────────┐          ┌───────▼────────┐
    │   Frontend      │          │   Backend API   │
    │   React + Vite  │          │  ASP.NET Core   │
    │   Port: 3000    │          │   Port: 8080    │
    └─────────────────┘          └────────┬────────┘
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    │                     │                     │
           ┌────────▼────────┐   ┌───────▼────────┐   ┌───────▼────────┐
           │   SQL Server    │   │     Redis      │   │    Mailhog     │
           │   Port: 1433    │   │   Port: 6379   │   │  Port: 8025    │
           └─────────────────┘   └────────────────┘   └────────────────┘
```

## 🚀 Quick Start Commands

### Initial Setup
```bash
# 1. Copy environment file
cp .env.example .env

# 2. Build and start all services
make install
# Or: docker-compose up -d --build

# 3. Check health
make health
```

### Daily Development
```bash
# Start services
make up

# View logs
make logs

# Stop services
make down
```

### Development Mode (Hot Reload)
```bash
make dev
# Frontend: http://localhost:5173
# API: http://localhost:8080
```

### Production Mode
```bash
make prod
# Access via Nginx: http://localhost
```

## 📦 Services Configuration

### 1. Backend API (cinema_api)
- **Image**: Custom build from Dockerfile
- **Ports**: 8080 (HTTP), 8081 (HTTPS)
- **Environment**: Docker
- **Health Check**: Swagger endpoint
- **Dependencies**: SQL Server, Redis

### 2. Frontend (cinema_frontend)
- **Image**: Custom build (Node 20 + Nginx Alpine)
- **Port**: 3000
- **Build**: Multi-stage (build + production)
- **Serves**: Static React SPA

### 3. SQL Server (cinema_sqlserver)
- **Image**: mcr.microsoft.com/mssql/server:2022-latest
- **Port**: 1433
- **Databases**: Booking, Identity
- **Volumes**: Data, Logs, Backups
- **Memory**: 2GB limit

### 4. Redis (cinema_redis)
- **Image**: redis:7-alpine
- **Port**: 6379
- **Persistence**: AOF + RDB snapshots
- **Max Memory**: 512MB with LRU eviction

### 5. Mailhog (cinema_mailhog)
- **Image**: mailhog/mailhog:latest
- **SMTP Port**: 1025
- **Web UI**: 8025
- **Purpose**: Email testing in development

### 6. Nginx (cinema_nginx) - Optional
- **Image**: nginx:alpine
- **Ports**: 80, 443
- **Profile**: production
- **Purpose**: Reverse proxy, load balancing

### 7. Redis Commander - Optional
- **Port**: 8081
- **Profile**: tools
- **Purpose**: Redis management UI

## 🔧 Configuration Details

### Environment Variables (.env)
```env
# Database
SA_PASSWORD=ComplexPassword123!

# JWT
JWT_SECRETKEY=your-secret-key
JWT_ACCESSTOKENEXPIRATION=20
JWT_REFRESHTOKENEXPIRATION=7

# VnPay
VNPAY_TMNCODE=RCQ192AT
VNPAY_HASHSECRET=your-hash-secret

# SMTP (Mailhog)
SMTP_SERVER=mailhog
SMTP_PORT=1025

# Identity
IDENTITY_DEFAULT_ADMIN_EMAIL=admin@cinema.com
IDENTITY_DEFAULT_ADMIN_PASSWORD=Admin@Cinema2024!
```

### Connection Strings
Automatically configured in docker-compose.yml:
```
Booking: Server=sqlserver;Database=Booking;User=sa;Password=${SA_PASSWORD}
Identity: Server=sqlserver;Database=Identity;User=sa;Password=${SA_PASSWORD}
Redis: redis:6379
```

## 📊 Resource Requirements

### Minimum
- **CPU**: 2 cores
- **RAM**: 8GB
- **Disk**: 20GB

### Recommended
- **CPU**: 4 cores
- **RAM**: 16GB
- **Disk**: 50GB SSD

### Per Service
- SQL Server: ~2GB RAM
- API: ~512MB RAM
- Frontend: ~256MB RAM
- Redis: ~512MB RAM
- Mailhog: ~128MB RAM

## 🎮 Makefile Commands

```bash
make help          # Show all available commands
make build         # Build all Docker images
make up            # Start all services
make down          # Stop all services
make restart       # Restart all services
make logs          # View all logs
make logs-api      # View API logs only
make logs-frontend # View frontend logs only
make logs-db       # View database logs only
make ps            # Show running containers
make health        # Check service health status

# Development
make dev           # Start with hot reload
make prod          # Start with production profile
make tools         # Start with management tools

# Database
make db-backup     # Backup databases
make db-restore    # Restore databases
make db-shell      # Open SQL shell

# Redis
make redis-cli     # Open Redis CLI

# Shell Access
make api-shell     # Open API container shell
make frontend-shell # Open frontend container shell

# Cleanup
make clean         # Remove containers and volumes
make prune         # Full Docker system cleanup
```

## 🔍 Health Checks

All services have health checks configured:

### API
```bash
curl http://localhost:8080/swagger/index.html
```

### Frontend
```bash
curl http://localhost:3000
```

### SQL Server
```bash
docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "SELECT 1" -C
```

### Redis
```bash
docker exec cinema_redis redis-cli ping
```

### Mailhog
```bash
curl http://localhost:8025
```

## 🐛 Common Issues & Solutions

### 1. Port Already in Use
```bash
# Check what's using the port
netstat -ano | findstr :8080

# Change port in docker-compose.yml
ports:
  - "8081:8080"
```

### 2. SQL Server Won't Start
```bash
# Check logs
docker-compose logs sqlserver

# Increase memory if needed
environment:
  - MSSQL_MEMORY_LIMIT_MB=4096
```

### 3. Frontend Build Fails
```bash
# Rebuild without cache
docker-compose build --no-cache frontend
```

### 4. API Can't Connect to Database
```bash
# Wait for SQL Server to be healthy
docker-compose up -d sqlserver
# Wait 30 seconds
docker-compose up -d api
```

## 📈 Monitoring

### View Resource Usage
```bash
docker stats
```

### View Container Details
```bash
docker inspect cinema_api
```

### View Network
```bash
docker network inspect cinema_network
```

## 🔐 Security Considerations

1. **Change default passwords** in production
2. **Use Docker secrets** for sensitive data
3. **Enable HTTPS** with SSL certificates
4. **Restrict network access** with firewall rules
5. **Regular updates** of base images
6. **Non-root users** in containers (already configured)
7. **Read-only file systems** where possible

## 🚢 Deployment Profiles

### Development (Default)
```bash
docker-compose up -d
```
- Mailhog for email testing
- Debug logging enabled
- Hot reload support with dev override

### Production
```bash
docker-compose --profile production up -d
```
- Nginx reverse proxy
- Optimized builds
- Production logging
- SSL/TLS support

### With Tools
```bash
docker-compose --profile tools up -d
```
- Redis Commander UI
- Additional monitoring tools

## 📝 Next Steps

1. **Review** `.env` file and update credentials
2. **Run** `make install` to build and start
3. **Access** services at configured ports
4. **Check** `make health` for service status
5. **View** logs with `make logs`
6. **Read** README.Docker.md for detailed guide

## 🤝 Support

- **Documentation**: README.Docker.md
- **Health Check**: `make health`
- **Logs**: `make logs`
- **Issues**: Check troubleshooting section

---

**Created**: 2024
**Version**: 1.0.0
**Status**: Production Ready ✅
