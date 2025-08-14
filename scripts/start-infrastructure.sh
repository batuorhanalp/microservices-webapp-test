#!/bin/bash

echo "🚀 Starting infrastructure services for development..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Start infrastructure services
echo "📦 Starting PostgreSQL, Redis, RabbitMQ, and observability tools..."
docker compose -f docker-compose.infrastructure.yml up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to start..."
sleep 10

# Check service health
echo "🔍 Checking service health..."

# Check PostgreSQL
if docker exec webapp-postgres-dev pg_isready -U webapp_user -d webapp_dev > /dev/null 2>&1; then
    echo "✅ PostgreSQL is ready"
else
    echo "❌ PostgreSQL is not ready"
fi

# Check Redis
if docker exec webapp-redis-dev redis-cli ping > /dev/null 2>&1; then
    echo "✅ Redis is ready"
else
    echo "❌ Redis is not ready"
fi

# Check RabbitMQ
if docker exec webapp-rabbitmq-dev rabbitmq-diagnostics check_port_connectivity > /dev/null 2>&1; then
    echo "✅ RabbitMQ is ready"
else
    echo "❌ RabbitMQ is not ready"
fi

echo ""
echo "🎯 Infrastructure services are starting up!"
echo ""
echo "📋 Service URLs:"
echo "   🐘 PostgreSQL:      localhost:5432"
echo "   🔴 Redis:           localhost:6379"
echo "   🐰 RabbitMQ:        localhost:5672"
echo "   🐰 RabbitMQ UI:     http://localhost:15672 (webapp/webapp_dev_password)"
echo "   📊 Seq Logs:        http://localhost:5341"
echo "   🔍 Jaeger Tracing:  http://localhost:16686"
echo "   🗄️  MinIO Console:   http://localhost:9001 (minioadmin/minioadmin123)"
echo "   💾 Adminer:         http://localhost:8080"
echo ""
echo "💡 Use 'scripts/stop-infrastructure.sh' to stop all services"
echo "💡 Use 'scripts/run-services-locally.sh' to start .NET services locally"
