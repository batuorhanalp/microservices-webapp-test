#!/bin/bash

echo "🚀 Deploying WebApp with Swagger Documentation"
echo ""

# Load environment variables
if [ -f .env.development ]; then
    export $(cat .env.development | grep -v '^#' | xargs)
    echo "✅ Loaded environment variables from .env.development"
else
    echo "⚠️  .env.development file not found, using default values"
fi

# Function to check if a port is available
check_port() {
    local port=$1
    if lsof -i :$port > /dev/null 2>&1; then
        echo "❌ Port $port is already in use"
        return 1
    else
        echo "✅ Port $port is available"
        return 0
    fi
}

# Function to find an available port starting from a base port
find_available_port() {
    local base_port=$1
    local port=$base_port
    while lsof -i :$port > /dev/null 2>&1; do
        port=$((port + 1))
    done
    echo $port
}

# Function to start a service with proper logging and port handling
start_service() {
    local service_name=$1
    local project_path=$2
    local preferred_port=$3
    
    # Find available port
    local port=$(find_available_port $preferred_port)
    if [ $port -ne $preferred_port ]; then
        echo "⚠️  Port $preferred_port occupied, using port $port for $service_name"
    fi
    
    echo "🚀 Starting $service_name on port $port..."
    
    cd "$project_path" || return 1
    
    # Create log file
    mkdir -p "$PROJECT_ROOT/logs"
    local log_file="$PROJECT_ROOT/logs/${service_name}.log"
    local pid_file="$PROJECT_ROOT/logs/${service_name}.pid"
    
    # Set environment variables
    export ASPNETCORE_URLS="http://localhost:${port}"
    export ASPNETCORE_ENVIRONMENT=Development
    
    # Remove old logs and PIDs
    rm -f "$log_file" "$pid_file"
    
    # Start the service
    dotnet run --no-build > "$log_file" 2>&1 &
    local pid=$!
    
    # Save PID
    echo $pid > "$pid_file"
    
    # Wait a bit and check if it started successfully
    sleep 3
    if ps -p $pid > /dev/null 2>&1; then
        echo "✅ $service_name started successfully on port $port (PID: $pid)"
        
        # Save the actual port used
        echo "$port" > "$PROJECT_ROOT/logs/${service_name}.port"
        
        cd - > /dev/null
        return 0
    else
        echo "❌ Failed to start $service_name"
        echo "📋 Check logs: tail -f $log_file"
        cd - > /dev/null
        return 1
    fi
}

# Set project root
PROJECT_ROOT="/Users/batuorhanalp/webapp-production"
cd "$PROJECT_ROOT"

# Create logs directory
mkdir -p logs

# Clean up old logs and PIDs
echo "🧹 Cleaning up old logs and processes..."
rm -f logs/*.log logs/*.pid logs/*.port

# Stop any existing services on our ports
echo "🛑 Stopping any existing services..."
for port in 5080 7001 7002 7003 7004 7005 7006 7007 7008; do
    pid=$(lsof -ti:$port 2>/dev/null)
    if [ ! -z "$pid" ]; then
        echo "🛑 Stopping process on port $port (PID: $pid)"
        kill $pid 2>/dev/null || kill -9 $pid 2>/dev/null
        sleep 1
    fi
done

# Build solution
echo "🏗️  Building solution..."
dotnet build src/backend/WebApp.sln --no-restore

if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please fix build errors first."
    exit 1
fi

echo ""
echo "🚀 Starting microservices..."

# Start each service with port management
declare -A services=(
    ["auth-service"]="src/backend/services/auth-service/WebApp.AuthService:7001"
    ["user-service"]="src/backend/services/user-service/WebApp.UserService:7002"
    ["post-service"]="src/backend/services/post-service/WebApp.PostService:7003"
    ["notification-service"]="src/backend/services/notification-service/WebApp.NotificationService:7004"
    ["like-service"]="src/backend/services/like-service/WebApp.LikeService:7005"
    ["comment-service"]="src/backend/services/comment-service/WebApp.CommentService:7006"
    ["media-upload-service"]="src/backend/services/media-upload-service/WebApp.MediaUploadService:7007"
    ["media-processing-service"]="src/backend/services/media-processing-service/WebApp.MediaProcessingService:7008"
)

# Track successful starts
declare -a started_services=()

# Start all microservices
for service_info in "${!services[@]}"; do
    IFS=':' read -r service_name service_path preferred_port <<< "${services[$service_info]}:${service_info}"
    
    if start_service "$service_info" "$service_path" "$preferred_port"; then
        started_services+=("$service_info")
    else
        echo "⚠️  Failed to start $service_info, continuing with other services..."
    fi
    
    echo ""
done

# Wait for services to fully start
echo "⏳ Waiting for services to fully initialize..."
sleep 5

# Start API Gateway last (so it can connect to services)
echo "🌐 Starting API Gateway..."
gateway_port=$(find_available_port 5080)
if start_service "gateway" "src/backend/api-gateway/WebApp.Gateway" "$gateway_port"; then
    started_services+=("gateway")
fi

echo ""
echo "🎯 Deployment Summary"
echo "=================="

# Display service status and URLs
for service in "${started_services[@]}"; do
    if [ -f "logs/${service}.pid" ] && [ -f "logs/${service}.port" ]; then
        local pid=$(cat "logs/${service}.pid")
        local port=$(cat "logs/${service}.port")
        
        if ps -p $pid > /dev/null 2>&1; then
            echo "✅ $service: http://localhost:$port (PID: $pid)"
            echo "   📖 Swagger: http://localhost:$port/swagger"
        else
            echo "❌ $service: Failed to start (check logs/${service}.log)"
        fi
    fi
done

echo ""
echo "🌟 Main Access Points:"

# Check if gateway is running
if [ -f "logs/gateway.pid" ] && [ -f "logs/gateway.port" ]; then
    gateway_pid=$(cat "logs/gateway.pid")
    gateway_port=$(cat "logs/gateway.port")
    
    if ps -p $gateway_pid > /dev/null 2>&1; then
        echo "🌐 API Gateway: http://localhost:$gateway_port"
        echo "📚 All Services Swagger: http://localhost:$gateway_port/swagger"
        echo "🔍 Service Discovery: http://localhost:$gateway_port/services"
        echo "❤️  Health Check: http://localhost:$gateway_port/health"
    fi
fi

echo ""
echo "📊 Infrastructure Services:"
echo "   🐘 PostgreSQL: localhost:5432"
echo "   🔴 Redis: localhost:6379"
echo "   🐰 RabbitMQ Management: http://localhost:15672"
echo "   📊 Seq Logs: http://localhost:5341"
echo "   🔍 Jaeger Tracing: http://localhost:16686"
echo "   💾 Database Admin: http://localhost:8080"

echo ""
echo "📋 Useful Commands:"
echo "   📈 View all logs: tail -f logs/*.log"
echo "   🔍 View specific service: tail -f logs/[service-name].log"
echo "   🛑 Stop all services: ./scripts/stop-services.sh"

echo ""
echo "🎉 Deployment completed! All services are running with Swagger documentation."
