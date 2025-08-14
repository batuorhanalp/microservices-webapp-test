#!/bin/bash

echo "🔍 Checking System Status..."
echo "=============================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if a service is responding
check_service() {
    local name=$1
    local url=$2
    local expected_status=${3:-200}
    
    if curl -s -o /dev/null -w "%{http_code}" "$url" | grep -q "$expected_status"; then
        echo -e "  ✅ ${GREEN}$name${NC}: $url"
        return 0
    else
        echo -e "  ❌ ${RED}$name${NC}: $url (Not responding)"
        return 1
    fi
}

# Function to check if a port is listening
check_port() {
    local name=$1
    local port=$2
    
    if netstat -an | grep LISTEN | grep -q ":$port"; then
        echo -e "  ✅ ${GREEN}$name${NC}: Port $port is listening"
        return 0
    else
        echo -e "  ❌ ${RED}$name${NC}: Port $port is not listening"
        return 1
    fi
}

# Function to check Docker containers
check_docker() {
    local name=$1
    local container_name=$2
    
    if docker ps | grep -q "$container_name"; then
        local status=$(docker ps --format "table {{.Names}}\t{{.Status}}" | grep "$container_name" | awk '{print $2}')
        echo -e "  ✅ ${GREEN}$name${NC}: $container_name ($status)"
        return 0
    else
        echo -e "  ❌ ${RED}$name${NC}: $container_name (Not running)"
        return 1
    fi
}

echo ""
echo "🐳 Infrastructure Services:"
echo "------------------------"
check_docker "PostgreSQL" "webapp-postgres-dev"
check_docker "Redis" "webapp-redis-dev"
check_docker "RabbitMQ" "webapp-rabbitmq-dev"
check_docker "MinIO" "webapp-minio-dev"
check_docker "Jaeger" "webapp-jaeger-dev"
check_docker "Seq" "webapp-seq-dev"
check_docker "Adminer" "webapp-adminer-dev"

echo ""
echo "🔌 Port Status:"
echo "-------------"
check_port "API Gateway" "5080"
check_port "Auth Service" "7001"
check_port "User Service" "7002"
check_port "Post Service" "7003"
check_port "Notification Service" "7004"
check_port "Like Service" "7005"
check_port "Comment Service" "7006"
check_port "Media Upload Service" "7007"
check_port "Media Processing Service" "7008"
check_port "Frontend" "3000"

echo ""
echo "🌐 Service Health Checks:"
echo "------------------------"
check_service "API Gateway" "http://localhost:5080/health"
check_service "Auth Service" "http://localhost:7001" "404"
check_service "User Service" "http://localhost:7002" "404"
check_service "Post Service" "http://localhost:7003" "404"
check_service "Notification Service" "http://localhost:7004" "404"
check_service "Like Service" "http://localhost:7005" "404"
check_service "Comment Service" "http://localhost:7006" "404"
check_service "Media Upload Service" "http://localhost:7007" "404"
check_service "Media Processing Service" "http://localhost:7008" "404"
check_service "Frontend" "http://localhost:3000"

echo ""
echo "📊 Infrastructure Health:"
echo "------------------------"
check_service "PostgreSQL" "http://localhost:8080" "200"  # Adminer
check_service "RabbitMQ UI" "http://localhost:15672" "200"
check_service "MinIO Console" "http://localhost:9001" "200"
check_service "Jaeger UI" "http://localhost:16686" "200"
check_service "Seq Logs" "http://localhost:5341" "200"

echo ""
echo "🎯 Quick Access URLs:"
echo "===================="
echo -e "  🌐 ${BLUE}Frontend:${NC}           http://localhost:3000"
echo -e "  🚪 ${BLUE}API Gateway:${NC}        http://localhost:5080"
echo -e "  📊 ${BLUE}Swagger Docs:${NC}       http://localhost:5080/swagger"
echo -e "  🐘 ${BLUE}Database Admin:${NC}     http://localhost:8080"
echo -e "  🐰 ${BLUE}RabbitMQ:${NC}           http://localhost:15672"
echo -e "  🗄️  ${BLUE}MinIO Console:${NC}      http://localhost:9001"
echo -e "  🔍 ${BLUE}Jaeger Tracing:${NC}     http://localhost:16686"
echo -e "  📋 ${BLUE}Seq Logs:${NC}           http://localhost:5341"

echo ""
echo "🔧 Management Commands:"
echo "======================"
echo "  📋 View logs:           tail -f logs/*.log"
echo "  🛑 Stop services:       ./scripts/stop-services.sh"
echo "  🔄 Restart:             ./scripts/stop-services.sh && ./scripts/run-services-locally.sh"
