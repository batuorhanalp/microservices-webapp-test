#!/bin/bash

# Quick Build Script for Development
# This script builds and tests projects quickly without full deployment pipeline

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() { echo -e "${BLUE}ℹ️  $1${NC}"; }
log_success() { echo -e "${GREEN}✅ $1${NC}"; }
log_error() { echo -e "${RED}❌ $1${NC}"; }

echo -e "${BLUE}🚀 Quick Build & Test${NC}\n"

# Build solution
log_info "Building solution..."
if dotnet build src/backend/WebApp.sln --configuration Debug; then
    log_success "Solution built successfully"
else
    log_error "Build failed"
    exit 1
fi

# Build microservices
log_info "Building microservices..."
services_built=0
for service_dir in src/backend/services/*; do
    if [ -d "$service_dir" ]; then
        service_name=$(basename "$service_dir")
        csproj_file=$(find "$service_dir" -name "*.csproj" -type f | head -1)
        
        if [ -n "$csproj_file" ]; then
            echo -n "  Building $service_name... "
            if dotnet build "$csproj_file" --configuration Debug --no-restore --verbosity quiet; then
                echo -e "${GREEN}✓${NC}"
                ((services_built++))
            else
                echo -e "${RED}✗${NC}"
                log_error "Failed to build $service_name"
                exit 1
            fi
        fi
    fi
done

log_success "$services_built microservices built successfully"

# Run tests
if [ -f "src/backend/WebApp.Tests/WebApp.Tests.csproj" ]; then
    log_info "Running tests..."
    if dotnet test src/backend/WebApp.Tests/WebApp.Tests.csproj --configuration Debug --no-build --verbosity normal; then
        log_success "All tests passed"
    else
        log_error "Tests failed"
        exit 1
    fi
else
    log_info "No test project found, skipping tests"
fi

echo -e "\n${GREEN}🎉 Quick build completed successfully!${NC}"
