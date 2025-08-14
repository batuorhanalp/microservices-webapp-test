#!/bin/bash

echo "🛑 Stopping infrastructure services..."

# Stop all infrastructure services
docker compose -f docker-compose.infrastructure.yml down

echo "✅ Infrastructure services stopped."
echo ""
echo "💡 Use 'docker compose -f docker-compose.infrastructure.yml down -v' to also remove volumes"
echo "💡 Use 'scripts/start-infrastructure.sh' to start services again"
