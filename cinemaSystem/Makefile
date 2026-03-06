# Cinema System - Makefile for Docker Management

.PHONY: help build up down restart logs clean prune dev prod test

# Default target
.DEFAULT_GOAL := help

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

help: ## Show this help message
	@echo "$(BLUE)Cinema Booking System - Docker Commands$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'

build: ## Build all Docker images
	@echo "$(BLUE)Building Docker images...$(NC)"
	docker-compose build --no-cache

up: ## Start all services
	@echo "$(GREEN)Starting all services...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)Services started!$(NC)"
	@echo "API: http://localhost:8080"
	@echo "Frontend: http://localhost:3000"
	@echo "Mailhog: http://localhost:8025"

down: ## Stop all services
	@echo "$(YELLOW)Stopping all services...$(NC)"
	docker-compose down

restart: down up ## Restart all services

logs: ## Show logs from all services
	docker-compose logs -f

logs-api: ## Show API logs
	docker-compose logs -f api

logs-frontend: ## Show frontend logs
	docker-compose logs -f frontend

logs-db: ## Show database logs
	docker-compose logs -f sqlserver

ps: ## Show running containers
	docker-compose ps

dev: ## Start in development mode with hot reload
	@echo "$(BLUE)Starting in development mode...$(NC)"
	docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
	@echo "$(GREEN)Development environment started!$(NC)"

prod: ## Start with production profile (includes nginx)
	@echo "$(BLUE)Starting in production mode...$(NC)"
	docker-compose --profile production up -d
	@echo "$(GREEN)Production environment started!$(NC)"

tools: ## Start with tools profile (includes redis-commander)
	@echo "$(BLUE)Starting with management tools...$(NC)"
	docker-compose --profile tools up -d
	@echo "$(GREEN)Management tools started!$(NC)"
	@echo "Redis Commander: http://localhost:8081"

stop: ## Stop all services without removing containers
	docker-compose stop

clean: ## Remove all containers and volumes
	@echo "$(RED)Removing all containers and volumes...$(NC)"
	docker-compose down -v
	@echo "$(GREEN)Cleanup complete!$(NC)"

prune: ## Remove all unused Docker resources
	@echo "$(RED)Pruning Docker system...$(NC)"
	docker system prune -af --volumes
	@echo "$(GREEN)Prune complete!$(NC)"

db-backup: ## Backup SQL Server databases
	@echo "$(BLUE)Backing up databases...$(NC)"
	docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "BACKUP DATABASE [Booking] TO DISK = '/var/opt/mssql/backup/Booking.bak' WITH FORMAT" -C
	docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "BACKUP DATABASE [Identity] TO DISK = '/var/opt/mssql/backup/Identity.bak' WITH FORMAT" -C
	@echo "$(GREEN)Backup complete!$(NC)"

db-restore: ## Restore SQL Server databases from backup
	@echo "$(YELLOW)Restoring databases...$(NC)"
	docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "RESTORE DATABASE [Booking] FROM DISK = '/var/opt/mssql/backup/Booking.bak' WITH REPLACE" -C
	docker exec cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -Q "RESTORE DATABASE [Identity] FROM DISK = '/var/opt/mssql/backup/Identity.bak' WITH REPLACE" -C
	@echo "$(GREEN)Restore complete!$(NC)"

db-shell: ## Open SQL Server shell
	docker exec -it cinema_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ComplexPassword123!" -C

redis-cli: ## Open Redis CLI
	docker exec -it cinema_redis redis-cli

api-shell: ## Open API container shell
	docker exec -it cinema_api /bin/bash

frontend-shell: ## Open frontend container shell
	docker exec -it cinema_frontend /bin/sh

health: ## Check health status of all services
	@echo "$(BLUE)Checking service health...$(NC)"
	@docker-compose ps --format json | jq -r '.[] | "\(.Name): \(.Health)"'

test: ## Run tests in containers
	@echo "$(BLUE)Running tests...$(NC)"
	docker-compose exec api dotnet test

migrate: ## Run database migrations
	@echo "$(BLUE)Running migrations...$(NC)"
	docker-compose exec api dotnet ef database update --project /app/Infrastructure.csproj

seed: ## Seed database with initial data
	@echo "$(BLUE)Seeding database...$(NC)"
	docker-compose exec api dotnet run --project /app/Api.csproj -- seed

install: build up ## Initial setup - build and start all services
	@echo "$(GREEN)Installation complete!$(NC)"
	@echo "Please wait for services to be healthy..."
	@sleep 10
	@make health
