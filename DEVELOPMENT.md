# 🚀 WebApp Development Guide

This guide covers local development setup, running services, and testing for the WebApp microservices platform.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Development Setup](#development-setup)
- [Running Services](#running-services)
- [Testing](#testing)
- [Debugging](#debugging)
- [Database Management](#database-management)
- [Monitoring & Logging](#monitoring--logging)
- [Troubleshooting](#troubleshooting)

## 🔧 Prerequisites

Before you begin, ensure you have the following installed:

- **Docker & Docker Compose** (v20.10+)
- **.NET 8 SDK** (for local development)
- **Git**
- **bash** (for running scripts)

## ⚡ Quick Start

1. **Clone and setup the repository:**
   ```bash
   git clone <repository-url>
   cd webapp-production
   chmod +x scripts/*.sh
   ```

2. **Start infrastructure services:**
   ```bash
   ./scripts/start-infrastructure.sh
   ```

3. **Run services locally for development:**
   ```bash
   ./scripts/run-services-locally.sh
   ```

4. **Access the application:**
   - API Gateway: http://localhost:5000
   - Individual services: http://localhost:7001-7008

## 🏗️ Development Setup

### Infrastructure Services

The infrastructure stack includes:
- **PostgreSQL** (port 5432) - Main database
- **Redis** (port 6379) - Caching and sessions
- **RabbitMQ** (port 5672, management: 15672) - Message queue
- **MinIO** (port 9000, console: 9001) - Object storage
- **Jaeger** (port 16686) - Distributed tracing
- **Seq** (port 5341) - Structured logging
- **Adminer** (port 8080) - Database administration

### Environment Configuration

Environment variables are configured in `.env.development`:
```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=webapp_dev
DATABASE_USER=webapp_user
DATABASE_PASSWORD=webapp_password

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=webapp_user
RABBITMQ_PASSWORD=webapp_password

# MinIO
MINIO_ENDPOINT=localhost:9000
MINIO_ACCESS_KEY=webapp_access
MINIO_SECRET_KEY=webapp_secret
MINIO_BUCKET=webapp-bucket

# Tracing
JAEGER_ENDPOINT=http://localhost:14268/api/traces

# Logging
SEQ_ENDPOINT=http://localhost:5341
```

## 🎯 Running Services

### Option 1: Infrastructure + Local Services (Recommended for Development)

Start infrastructure in Docker and run .NET services locally:

```bash
# Start infrastructure
./scripts/start-infrastructure.sh

# Run services locally (for debugging)
./scripts/run-services-locally.sh

# Stop services when done
./scripts/stop-services.sh
```

This approach is ideal for:
- Debugging with IDE breakpoints
- Fast iteration and hot reload
- Easy access to logs and debugging tools

### Option 2: Full Docker Development

Run everything in Docker containers:

```bash
# Start everything including services
docker-compose -f docker-compose.dev.yml up --build

# Stop everything
docker-compose -f docker-compose.dev.yml down
```

### Option 3: Infrastructure Only

Run only infrastructure services, manually start individual services:

```bash
# Start infrastructure
./scripts/start-infrastructure.sh

# Manually run specific services
cd src/backend/services/notification-service/WebApp.NotificationService
dotnet run
```

## 🧪 Testing

### Running All Tests

```bash
# Run all tests
./scripts/run-tests.sh

# Run with verbose output
./scripts/run-tests.sh -v

# Run with code coverage
./scripts/run-tests.sh -c
```

### Running Specific Service Tests

```bash
# Run notification service tests
./scripts/run-tests.sh notification

# Run auth service tests with coverage
./scripts/run-tests.sh -c auth
```

### Test Types

```bash
# Run only unit tests
./scripts/run-tests.sh --unit

# Run only integration tests
./scripts/run-tests.sh --integration

# Watch mode for development
./scripts/run-tests.sh -w notification
```

## 🐛 Debugging

### Local Debugging with IDE

1. Start infrastructure:
   ```bash
   ./scripts/start-infrastructure.sh
   ```

2. In your IDE (VS Code, Visual Studio, Rider):
   - Open the service project
   - Set breakpoints
   - Start debugging (F5)
   - The service will start with debugger attached

### Debug Specific Service

```bash
# Start infrastructure
./scripts/start-infrastructure.sh

# Debug notification service
cd src/backend/services/notification-service/WebApp.NotificationService
dotnet run --configuration Debug
```

### Container Debugging

For debugging services in containers:

```bash
# Build with debug configuration
docker-compose -f docker-compose.dev.yml build

# Start with debug mode
docker-compose -f docker-compose.dev.yml up
```

## 💾 Database Management

### Database Access

Access databases using Adminer:
- URL: http://localhost:8080
- System: PostgreSQL
- Server: postgres
- Username: webapp_user
- Password: webapp_password

### Database Commands

```bash
# Run migrations for a specific service
cd src/backend/services/notification-service/WebApp.NotificationService
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# Reset database (caution!)
dotnet ef database drop --force
dotnet ef database update
```

### Multiple Databases

Each service has its own database:
- `auth_service_db`
- `user_service_db`
- `post_service_db`
- `notification_service_db`
- `like_service_db`
- `comment_service_db`
- `media_upload_service_db`
- `media_processing_service_db`

## 📊 Monitoring & Logging

### Accessing Monitoring Tools

- **Service Logs**: `tail -f logs/[service-name].log`
- **All Logs**: `tail -f logs/*.log`
- **Jaeger Tracing**: http://localhost:16686
- **Seq Logging**: http://localhost:5341
- **RabbitMQ Management**: http://localhost:15672 (webapp_user/webapp_password)
- **MinIO Console**: http://localhost:9001 (webapp_access/webapp_secret)

### Log Levels

Services use structured logging with the following levels:
- **Trace**: Detailed diagnostic information
- **Debug**: Debug information for development
- **Information**: General application flow
- **Warning**: Potentially harmful situations
- **Error**: Error events that don't stop the application
- **Critical**: Very severe error events

### Health Checks

All services expose health check endpoints:
- Individual service: `http://localhost:700X/health`
- Via API Gateway: `http://localhost:5000/health`

## 🔧 Troubleshooting

### Common Issues

#### Port Conflicts
```bash
# Check what's using a port
lsof -i :5000

# Kill process using port
kill -9 $(lsof -ti:5000)
```

#### Database Connection Issues
```bash
# Check if PostgreSQL is running
docker-compose -f docker-compose.infrastructure.yml ps

# Restart infrastructure
./scripts/stop-infrastructure.sh
./scripts/start-infrastructure.sh
```

#### Build Failures
```bash
# Clean solution
dotnet clean src/backend/WebApp.sln

# Restore packages
dotnet restore src/backend/WebApp.sln

# Rebuild
dotnet build src/backend/WebApp.sln
```

#### Container Issues
```bash
# Clean up containers
docker system prune -f

# Rebuild containers
docker-compose -f docker-compose.dev.yml build --no-cache
```

### Service-Specific Issues

#### Notification Service Tests
If notification service tests fail:
```bash
# Check test database
./scripts/run-tests.sh -v notification

# Reset test environment
docker-compose -f docker-compose.infrastructure.yml restart postgres
```

#### RabbitMQ Connection Issues
```bash
# Check RabbitMQ status
docker-compose -f docker-compose.infrastructure.yml logs rabbitmq

# Access management console
open http://localhost:15672
```

### Performance Issues

#### Slow Database Queries
- Check Seq logs: http://localhost:5341
- Look for long-running queries in PostgreSQL logs
- Use database indexes appropriately

#### Memory Issues
```bash
# Check container memory usage
docker stats

# Monitor .NET memory
dotnet-counters monitor --process-id <PID>
```

## 📝 Best Practices

### Development Workflow

1. **Start with infrastructure**: Always run `./scripts/start-infrastructure.sh`
2. **Use local services for development**: Run services with `./scripts/run-services-locally.sh`
3. **Test frequently**: Use `./scripts/run-tests.sh` during development
4. **Monitor logs**: Keep an eye on `logs/*.log` files
5. **Use health checks**: Monitor service health at `/health` endpoints

### Code Changes

1. **Run tests after changes**: `./scripts/run-tests.sh [service]`
2. **Check logs for errors**: `tail -f logs/[service].log`
3. **Update migrations**: Use Entity Framework migrations for schema changes
4. **Validate integration**: Test service interactions

### Debugging Tips

1. **Use structured logging**: Add correlation IDs and context
2. **Set appropriate log levels**: Use Debug for development, Information for production
3. **Use health checks**: Implement comprehensive health checks
4. **Monitor dependencies**: Check database, Redis, RabbitMQ connections

## 🆘 Getting Help

- **Logs**: Check `logs/` directory for service-specific logs
- **Health Status**: Visit `http://localhost:5000/health` for overall system status
- **Database Issues**: Use Adminer at `http://localhost:8080`
- **Message Queue**: Check RabbitMQ management at `http://localhost:15672`
- **Tracing**: Use Jaeger at `http://localhost:16686` for request tracing
- **Structured Logs**: Search logs in Seq at `http://localhost:5341`
