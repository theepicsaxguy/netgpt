.PHONY: help db-up db-down db-logs dev-up dev-down dev-logs prod-up prod-down prod-logs

help:
	@echo "NetGPT Docker Commands"
	@echo ""
	@echo "Database only (local dev):"
	@echo "  make db-up       - Start PostgreSQL container"
	@echo "  make db-down     - Stop PostgreSQL container"
	@echo "  make db-logs     - View PostgreSQL logs"
	@echo ""
	@echo "Development (with API in Docker):"
	@echo "  make dev-up      - Start PostgreSQL and API containers"
	@echo "  make dev-down    - Stop all containers"
	@echo "  make dev-logs    - View container logs"
	@echo ""
	@echo "Production:"
	@echo "  make prod-up     - Start PostgreSQL and API containers (production)"
	@echo "  make prod-down   - Stop all containers"
	@echo "  make prod-logs   - View container logs"

# Database only
db-up:
	docker compose up -d

db-down:
	docker compose down

db-logs:
	docker compose logs -f

# Development
dev-up:
	docker compose -f docker-compose.dev.yml up -d

dev-down:
	docker compose -f docker-compose.dev.yml down

dev-logs:
	docker compose -f docker-compose.dev.yml logs -f

# Production
prod-up:
	docker compose -f docker-compose.prod.yml up -d

prod-down:
	docker compose -f docker-compose.prod.yml down

prod-logs:
	docker compose -f docker-compose.prod.yml logs -f
