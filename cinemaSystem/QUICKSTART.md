# ⚡ Quick Start Guide - Cinema Booking System

## 🎯 Get Running in 5 Minutes

### Step 1: Prerequisites Check
```bash
docker --version    # Should be 20.10+
docker-compose --version  # Should be 2.0+
```

### Step 2: Setup Environment
```bash
# Copy environment file
cp .env.example .env

# Edit if needed (optional)
nano .env
```

### Step 3: Start Everything
```bash
# Option A: Using Makefile (Recommended)
make install

# Option B: Using docker-compose directly
docker-compose up -d --build
```

### Step 4: Wait for Services (30-60 seconds)
```bash
# Check status
make health

# Or watch logs
make logs
```

### Step 5: Access Applications
- 🌐 **Frontend**: http://localhost:3000
- 🔧 **API/Swagger**: http://localhost:8080/swagger
- 📧 **Mailhog**: http://localhost:8025
- 💾 **SQL Server**: localhost:1433
- 🔴 **Redis**: localhost:6379

## 🎮 Common Commands

```bash
make up        # Start services
make down      # Stop services
make logs      # View logs
make restart   # Restart all
make health    # Check status
```

## 🔑 Default Credentials

### Admin Account
- **Email**: admin@cinema.com
- **Password**: Admin@Cinema2024!

### Database
- **Server**: localhost:1433
- **User**: sa
- **Password**: ComplexPassword123!

### Redis Commander (if enabled)
- **URL**: http://localhost:8081
- **User**: admin
- **Password**: admin

## 🐛 Troubleshooting

### Services won't start?
```bash
# Check logs
make logs

# Rebuild
make down
make build
make up
```

### Port conflicts?
```bash
# Check what's using ports
netstat -ano | findstr :8080
netstat -ano | findstr :3000
netstat -ano | findstr :1433
```

### Database issues?
```bash
# Restart SQL Server
docker-compose restart sqlserver

# Check SQL Server logs
make logs-db
```

## 📚 Full Documentation
See **README.Docker.md** for complete guide.

## 🆘 Need Help?
1. Check logs: `make logs`
2. Check health: `make health`
3. Read README.Docker.md
4. Contact team

---
**Ready to develop!** 🚀
