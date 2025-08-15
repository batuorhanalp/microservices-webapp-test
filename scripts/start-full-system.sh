#!/bin/bash

echo "🚀 Starting Complete WebApp System..."
echo "====================================="

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Load environment variables
if [ -f .env.development ]; then
    export $(cat .env.development | grep -v '^#' | xargs)
    echo -e "✅ ${GREEN}Loaded environment variables${NC}"
else
    echo -e "⚠️ ${YELLOW}.env.development file not found${NC}"
fi

# Create logs directory
mkdir -p logs

# Function to start a service in background
start_dotnet_service() {
    local service_name=$1
    local project_path=$2
    local port=$3
    
    echo -e "🔧 Starting ${BLUE}${service_name}${NC} on port ${port}..."
    
    cd "${project_path}" || exit
    export ASPNETCORE_URLS="http://localhost:${port}"
    export ASPNETCORE_ENVIRONMENT=Development
    
    # Start the service in background
    nohup dotnet run --no-build > "../../../../logs/${service_name}.log" 2>&1 &
    local pid=$!
    
    # Save PID for cleanup
    echo $pid > "../../../../logs/${service_name}.pid"
    
    cd - > /dev/null
    echo -e "  ✅ ${service_name} started with PID ${pid}"
    
    # Brief pause to let service initialize
    sleep 1
}

# Step 1: Infrastructure (if not already running)
echo ""
echo -e "🐳 ${BLUE}Step 1: Infrastructure Services${NC}"
echo "--------------------------------"
if ! docker ps | grep -q webapp-postgres-dev; then
    echo "Starting infrastructure..."
    ./scripts/start-infrastructure.sh
else
    echo -e "✅ ${GREEN}Infrastructure already running${NC}"
fi

# Step 2: Build .NET solution
echo ""
echo -e "🏗️ ${BLUE}Step 2: Building .NET Solution${NC}"
echo "--------------------------------"
dotnet build src/backend/WebApp.sln
if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please fix build errors first."
    exit 1
fi

# Step 3: Start .NET Microservices
echo ""
echo -e "🔧 ${BLUE}Step 3: Starting .NET Microservices${NC}"
echo "------------------------------------"

start_dotnet_service "auth-service" "src/backend/services/auth-service/WebApp.AuthService" 7001
start_dotnet_service "user-service" "src/backend/services/user-service/WebApp.UserService" 7002
start_dotnet_service "post-service" "src/backend/services/post-service/WebApp.PostService" 7003
start_dotnet_service "notification-service" "src/backend/services/notification-service/WebApp.NotificationService" 7004
start_dotnet_service "like-service" "src/backend/services/like-service/WebApp.LikeService" 7005
start_dotnet_service "comment-service" "src/backend/services/comment-service/WebApp.CommentService" 7006
start_dotnet_service "media-upload-service" "src/backend/services/media-upload-service/WebApp.MediaUploadService" 7007
start_dotnet_service "media-processing-service" "src/backend/services/media-processing-service/WebApp.MediaProcessingService" 7008

# Step 4: Start API Gateway
echo ""
echo -e "🚪 ${BLUE}Step 4: Starting API Gateway${NC}"
echo "-----------------------------"
cd src/backend/api-gateway/WebApp.Gateway || exit
export ASPNETCORE_URLS="http://localhost:7009;https://localhost:7049"
export ASPNETCORE_ENVIRONMENT=Development
nohup dotnet run --no-build > ../../../../logs/gateway.log 2>&1 &
gateway_pid=$!
echo $gateway_pid > ../../../../logs/gateway.pid
cd - > /dev/null
echo -e "✅ ${GREEN}API Gateway started with PID ${gateway_pid}${NC}"

# Step 5: Start Frontend
echo ""
echo -e "🌐 ${BLUE}Step 5: Starting Frontend${NC}"
echo "-------------------------"
cd src/frontend/webapp-frontend || exit
nohup npm run dev > ../../../logs/frontend.log 2>&1 &
frontend_pid=$!
echo $frontend_pid > ../../../logs/frontend.pid
cd - > /dev/null
echo -e "✅ ${GREEN}Frontend started with PID ${frontend_pid}${NC}"

# Wait for services to start
echo ""
echo -e "⏳ ${YELLOW}Waiting for services to initialize...${NC}"
sleep 15

# Final status check
echo ""
echo -e "🔍 ${BLUE}System Status Check${NC}"
echo "-------------------"
./scripts/check-system-status.sh

echo ""
echo -e "🎉 ${GREEN}System startup complete!${NC}"
echo ""
echo -e "📋 ${BLUE}Quick Access:${NC}"
echo -e "  🌐 Frontend:      ${YELLOW}http://localhost:3000${NC}"
echo -e "  🚪 API Gateway:   ${YELLOW}http://localhost:7009${NC}"
echo -e "  📊 Swagger:       ${YELLOW}http://localhost:7009/swagger${NC}"
echo -e "  🐘 Database:      ${YELLOW}http://localhost:8080${NC}"
echo ""
echo -e "🛑 ${BLUE}To stop all services:${NC} ./scripts/stop-services.sh"
