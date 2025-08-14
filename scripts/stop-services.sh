#!/bin/bash

echo "🛑 Stopping locally running .NET services..."

# Function to stop a service
stop_service() {
    local service_name=$1
    local pid_file="logs/${service_name}.pid"
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        if ps -p $pid > /dev/null 2>&1; then
            echo "🛑 Stopping ${service_name} (PID: ${pid})..."
            kill $pid
            # Wait a bit for graceful shutdown
            sleep 2
            # Force kill if still running
            if ps -p $pid > /dev/null 2>&1; then
                echo "⚡ Force stopping ${service_name}..."
                kill -9 $pid
            fi
        fi
        rm -f "$pid_file"
        echo "✅ ${service_name} stopped"
    else
        echo "ℹ️  No PID file found for ${service_name}"
    fi
}

# Stop all services
stop_service "auth-service"
stop_service "user-service"
stop_service "post-service"
stop_service "notification-service"
stop_service "like-service"
stop_service "comment-service"
stop_service "media-upload-service"
stop_service "media-processing-service"
stop_service "gateway"

# Also kill any remaining dotnet processes that might be running our services
echo "🧹 Cleaning up any remaining dotnet processes..."

# Kill processes using our specific ports
for port in 5000 5001 7001 7002 7003 7004 7005 7006 7007 7008; do
    pid=$(lsof -ti:$port 2>/dev/null)
    if [ ! -z "$pid" ]; then
        echo "🛑 Stopping process on port $port (PID: $pid)..."
        kill $pid 2>/dev/null || kill -9 $pid 2>/dev/null
    fi
done

echo ""
echo "✅ All services stopped!"

# Optionally clean up logs
read -p "🗑️  Do you want to clean up log files? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -f logs/*.log
    echo "✅ Log files cleaned up"
fi
