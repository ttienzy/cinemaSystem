#!/bin/bash
# Health check script for all services

echo "🏥 Cinema System Health Check"
echo "=============================="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if service is healthy
check_service() {
    local service=$1
    local url=$2
    
    if curl -f -s -o /dev/null "$url"; then
        echo -e "${GREEN}✓${NC} $service is healthy"
        return 0
    else
        echo -e "${RED}✗${NC} $service is unhealthy"
        return 1
    fi
}

# Check API
echo ""
echo "Checking API..."
check_service "API" "http://localhost:8080/swagger/index.html"

# Check Frontend
echo "Checking Frontend..."
check_service "Frontend" "http://localhost:3000"

# Check Mailhog
echo "Checking Mailhog..."
check_service "Mailhog" "http://localhost:8025"

# Check SQL Server
echo "Checking SQL Server..."
if docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "SELECT 1" -C &> /dev/null; then
    echo -e "${GREEN}✓${NC} SQL Server is healthy"
else
    echo -e "${RED}✗${NC} SQL Server is unhealthy"
fi

# Check Redis
echo "Checking Redis..."
if docker exec cinema_redis redis-cli ping &> /dev/null; then
    echo -e "${GREEN}✓${NC} Redis is healthy"
else
    echo -e "${RED}✗${NC} Redis is unhealthy"
fi

echo ""
echo "=============================="
echo "Health check completed!"
