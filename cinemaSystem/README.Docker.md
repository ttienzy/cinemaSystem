# 🐳 Cinema Booking System - Docker Deployment Guide

## 📋 Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Services](#services)
- [Configuration](#configuration)
- [Usage](#usage)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

This Docker Compose setup provides a complete containerized environment for the Cinema Booking System with:
- **Backend API**: ASP.NET Core 8.0
- **Frontend**: React 19 + Vite
- **Database**: SQL Server 2022
- **Cache**: Redis 7
- **Email Testing**: Mailhog
- **Reverse Proxy**: Nginx (production)

## ✅ Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- 8GB RAM minimum
- 20GB free disk space

## 🚀 Quick Start

### 1. Clone and Setup
```bash
git clone <repository-url>
cd cinemaSystem
cp .env.example .env
```

### 2. Start All Services
```bash
# Using docker-compose
docker-compose up -d

# Or using Makefile
make up
```

### 3. Access Services
- **Frontend**: http://localhost:3000
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **Mailhog UI**: http://localhost:8025
- **Redis Commander**: http://localhost:8081 (with tools profile)

## 📦 Services

### API Service
- **Container**: `cinema_api`
- **Ports**: 8080 (HTTP), 8081 (HTTPS)
- **Health Check**: `/swagger/index.html`
- **Logs**: `./logs/api`

### Frontend Service
- **Container**: `cinema_frontend`
- **Port**: 3000
- **Technology**: React + Vite + Nginx
- **Health Check**: HTTP GET /

### SQL Server
- **Container**: `cinema_sqlserver`
- **Port**: 1433
- **Databases**: `Booking`, `Identity`
- **SA Password**: Set in `.env`
- **Volumes**: Data, Logs, Backups

### Redis
- **Container**: `cinema_redis`
- **Port**: 6379
- **Persistence**: AOF + RDB
- **Max Memory**: 512MB

### Mailhog
- **Container**: `cinema_mailhog`
- **SMTP Port**: 1025
- **Web UI**: 8025

### Nginx (Production)
- **Container**: `cinema_nginx`
- **Ports**: 80, 443
- **Profile**: `production`

## ⚙️ Configuration

### Environment Variables
Edit `.env` file:
```env
SA_PASSWORD=YourStrongPassword123!
JWT_SECRETKEY=YourSecretKey
VNPAY_TMNCODE=YourTmnCode
VNPAY_HASHSECRET=YourHashSecret
```

### Database Connection
Connections are automatically configured:
```
Server=sqlserver;Database=Booking;User=sa;Password=${SA_PASSWORD}
```

### Frontend API URL
Set in docker-compose.yml:
```yaml
environment:
  - VITE_API_URL=http://localhost:8080
```

## 🎮 Usage

### Using Makefile Commands
```bash
make help          # Show all commands
make build         # Build images
make up            # Start services
make down          # Stop services
make restart       # Restart services
make logs          # View all logs
make logs-api      # View API logs only
make ps            # Show running containers
make health        # Check service health
```

### Development Mode
```bash
# Start with hot reload
make dev

# Or manually
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### Production Mode
```bash
# Start with Nginx proxy
make prod

# Or manually
docker-compose --profile production up -d
```

### With Management Tools
```bash
# Start with Redis Commander
make tools

# Or manually
docker-compose --profile tools up -d
```

## 🔧 Common Tasks

### Database Operations

#### Backup Databases
```bash
make db-backup
```

#### Restore Databases
```bash
make db-restore
```

#### Access SQL Shell
```bash
make db-shell
# Or
docker exec -it cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -C
```

### Redis Operations

#### Access Redis CLI
```bash
make redis-cli
# Or
docker exec -it cinema_redis redis-cli
```

#### Flush Redis Cache
```bash
docker exec cinema_redis redis-cli FLUSHALL
```

### Container Shell Access

#### API Container
```bash
make api-shell
```

#### Frontend Container
```bash
make frontend-shell
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f sqlserver
```

### Check Service Health
```bash
make health
# Or
docker-compose ps
```

## 🐛 Troubleshooting

### Services Won't Start

**Check logs:**
```bash
docker-compose logs
```

**Check disk space:**
```bash
df -h
```

**Rebuild images:**
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### Database Connection Issues

**Check SQL Server health:**
```bash
docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "SELECT 1" -C
```

**Restart SQL Server:**
```bash
docker-compose restart sqlserver
```

### API Not Responding

**Check API logs:**
```bash
docker-compose logs api
```

**Restart API:**
```bash
docker-compose restart api
```

**Check health endpoint:**
```bash
curl http://localhost:8080/swagger/index.html
```

### Frontend Build Fails

**Check Node version:**
```bash
docker-compose exec frontend node --version
```

**Rebuild frontend:**
```bash
docker-compose build --no-cache frontend
docker-compose up -d frontend
```

### Redis Connection Issues

**Test Redis:**
```bash
docker exec cinema_redis redis-cli ping
```

**Check Redis logs:**
```bash
docker-compose logs redis
```

### Port Conflicts

**Check ports in use:**
```bash
# Windows
netstat -ano | findstr :8080
netstat -ano | findstr :3000
netstat -ano | findstr :1433

# Linux/Mac
lsof -i :8080
lsof -i :3000
lsof -i :1433
```

**Change ports in docker-compose.yml:**
```yaml
ports:
  - "8081:8080"  # Change host port
```

## 🧹 Cleanup

### Stop and Remove Containers
```bash
make down
```

### Remove Volumes (⚠️ Data Loss)
```bash
make clean
```

### Full System Cleanup
```bash
make prune
```

## 📊 Monitoring

### Resource Usage
```bash
docker stats
```

### Container Inspection
```bash
docker inspect cinema_api
docker inspect cinema_sqlserver
```

### Network Inspection
```bash
docker network inspect cinema_network
```

## 🔐 Security Notes

1. **Change default passwords** in `.env`
2. **Use secrets** for production (Docker Swarm/Kubernetes)
3. **Enable HTTPS** with SSL certificates
4. **Restrict network access** in production
5. **Regular security updates** for base images

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [SQL Server Docker](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker)

## 🤝 Support

For issues and questions:
1. Check logs: `make logs`
2. Check health: `make health`
3. Review this guide
4. Contact development team

---

**Last Updated**: 2024
**Version**: 1.0.0
