#!/bin/bash

echo "🔧 Running .NET services locally for debugging..."

# Load environment variables
if [ -f .env.development ]; then
    export $(cat .env.development | grep -v '^#' | xargs)
    echo "✅ Loaded environment variables from .env.development"
else
    echo "⚠️  .env.development file not found, using default values"
fi

# Function to start a service in background
start_service() {
    local service_name=$1
    local project_path=$2
    local port=$3
    
    echo "🚀 Starting ${service_name} on port ${port}..."
    
    cd "${project_path}" || exit
    export ASPNETCORE_URLS="http://localhost:${port}"
    export ASPNETCORE_ENVIRONMENT=Development
    
    # Start the service in background
    dotnet run --no-build > "../../../logs/${service_name}.log" 2>&1 &
    local pid=$!
    
    # Save PID for cleanup
    echo $pid > "../../../logs/${service_name}.pid"
    
    cd - > /dev/null
    echo "✅ ${service_name} started with PID ${pid}"
}

# Create logs directory
mkdir -p logs

# Clear existing logs
rm -f logs/*.log
rm -f logs/*.pid

echo "🏗️  Building solution..."
dotnet build src/backend/WebApp.sln

if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please fix build errors first."
    exit 1
fi

echo ""
echo "🚀 Starting microservices..."

# Start each service
start_service "auth-service" "src/backend/services/auth-service/WebApp.AuthService" 7001
start_service "user-service" "src/backend/services/user-service/WebApp.UserService" 7002
start_service "post-service" "src/backend/services/post-service/WebApp.PostService" 7003
start_service "notification-service" "src/backend/services/notification-service/WebApp.NotificationService" 7004
start_service "like-service" "src/backend/services/like-service/WebApp.LikeService" 7005
start_service "comment-service" "src/backend/services/comment-service/WebApp.CommentService" 7006
start_service "media-upload-service" "src/backend/services/media-upload-service/WebApp.MediaUploadService" 7007
start_service "media-processing-service" "src/backend/services/media-processing-service/WebApp.MediaProcessingService" 7008

# Start API Gateway
echo "🚀 Starting API Gateway on ports 5000 (HTTP) and 5001 (HTTPS)..."
cd src/backend/api-gateway/WebApp.Gateway || exit
export ASPNETCORE_URLS="http://localhost:5000;https://localhost:5001"
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --no-build > "../../../logs/gateway.log" 2>&1 &
gateway_pid=$!
echo $gateway_pid > "../../../logs/gateway.pid"
cd - > /dev/null

echo ""
echo "🎯 All services are starting up!"
echo ""
echo "📋 Service URLs:"
echo "   🌐 API Gateway:           http://localhost:5000 | https://localhost:5001"
echo "   🔐 Auth Service:          http://localhost:7001"
echo "   👥 User Service:          http://localhost:7002"
echo "   📝 Post Service:          http://localhost:7003"
echo "   🔔 Notification Service:  http://localhost:7004"
echo "   ❤️  Like Service:          http://localhost:7005"
echo "   💬 Comment Service:       http://localhost:7006"
echo "   📁 Media Upload Service:  http://localhost:7007"
echo "   🎬 Media Processing:      http://localhost:7008"
echo ""
echo "📊 Monitoring:"
echo "   📋 Logs: tail -f logs/[service-name].log"
echo "   📈 All logs: tail -f logs/*.log"
echo ""
echo "🛑 To stop all services: scripts/stop-services.sh"
