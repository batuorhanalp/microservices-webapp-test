#!/bin/bash

echo "🧪 Test Runner for WebApp Services"
echo ""

# Function to display help
show_help() {
    echo "Usage: $0 [OPTIONS] [SERVICE_NAME]"
    echo ""
    echo "Options:"
    echo "  -h, --help          Show this help message"
    echo "  -v, --verbose       Run tests with verbose output"
    echo "  -c, --coverage      Run tests with code coverage"
    echo "  -w, --watch         Run tests in watch mode"
    echo "  --unit              Run only unit tests"
    echo "  --integration       Run only integration tests"
    echo "  --all               Run all tests (default)"
    echo ""
    echo "Service Names:"
    echo "  auth               Auth Service tests"
    echo "  user               User Service tests"
    echo "  post               Post Service tests"
    echo "  notification       Notification Service tests"
    echo "  like               Like Service tests"
    echo "  comment            Comment Service tests"
    echo "  media-upload       Media Upload Service tests"
    echo "  media-processing   Media Processing Service tests"
    echo "  gateway            API Gateway tests"
    echo "  all                All service tests (default)"
    echo ""
    echo "Examples:"
    echo "  $0                           # Run all tests"
    echo "  $0 notification             # Run notification service tests"
    echo "  $0 -v -c notification       # Run notification tests with verbose output and coverage"
    echo "  $0 --unit auth              # Run only unit tests for auth service"
}

# Default values
SERVICE="all"
VERBOSE=false
COVERAGE=false
WATCH=false
TEST_TYPE="all"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -w|--watch)
            WATCH=true
            shift
            ;;
        --unit)
            TEST_TYPE="unit"
            shift
            ;;
        --integration)
            TEST_TYPE="integration"
            shift
            ;;
        --all)
            TEST_TYPE="all"
            shift
            ;;
        auth|user|post|notification|like|comment|media-upload|media-processing|gateway|all)
            SERVICE=$1
            shift
            ;;
        *)
            echo "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Build test arguments
TEST_ARGS=""

if [ "$VERBOSE" = true ]; then
    TEST_ARGS="$TEST_ARGS --verbosity normal"
fi

if [ "$COVERAGE" = true ]; then
    TEST_ARGS="$TEST_ARGS --collect-coverage --coverage-output-format cobertura"
fi

if [ "$WATCH" = true ]; then
    TEST_ARGS="$TEST_ARGS --watch"
fi

# Add test filters based on type
case $TEST_TYPE in
    unit)
        TEST_ARGS="$TEST_ARGS --filter Category!=Integration"
        ;;
    integration)
        TEST_ARGS="$TEST_ARGS --filter Category=Integration"
        ;;
esac

# Function to run tests for a specific service
run_service_tests() {
    local service_name=$1
    local test_project_path=$2
    
    echo "🧪 Running tests for ${service_name}..."
    echo "📁 Test project: ${test_project_path}"
    
    if [ ! -d "$test_project_path" ]; then
        echo "❌ Test project not found: $test_project_path"
        return 1
    fi
    
    cd "$test_project_path" || return 1
    
    echo "🏗️  Building test project..."
    dotnet build
    
    if [ $? -ne 0 ]; then
        echo "❌ Build failed for ${service_name} tests"
        cd - > /dev/null
        return 1
    fi
    
    echo "▶️  Running tests..."
    dotnet test $TEST_ARGS
    
    local test_result=$?
    cd - > /dev/null
    
    if [ $test_result -eq 0 ]; then
        echo "✅ ${service_name} tests passed"
    else
        echo "❌ ${service_name} tests failed"
    fi
    
    return $test_result
}

# Start infrastructure if not already running (for integration tests)
if [ "$TEST_TYPE" = "integration" ] || [ "$TEST_TYPE" = "all" ]; then
    echo "🔧 Checking if infrastructure is running for integration tests..."
    if ! docker-compose -f docker-compose.infrastructure.yml ps | grep -q "Up"; then
        echo "🚀 Starting infrastructure for integration tests..."
        ./scripts/start-infrastructure.sh
        echo "⏳ Waiting 30 seconds for services to be ready..."
        sleep 30
    fi
fi

echo "🧪 Running tests with configuration:"
echo "   Service: $SERVICE"
echo "   Test Type: $TEST_TYPE"
echo "   Verbose: $VERBOSE"
echo "   Coverage: $COVERAGE"
echo "   Watch: $WATCH"
echo ""

# Track overall test results
OVERALL_RESULT=0

# Run tests based on service selection
case $SERVICE in
    auth)
        run_service_tests "Auth Service" "src/backend/services/auth-service/tests/WebApp.AuthService.Tests"
        OVERALL_RESULT=$?
        ;;
    user)
        run_service_tests "User Service" "src/backend/services/user-service/tests/WebApp.UserService.Tests"
        OVERALL_RESULT=$?
        ;;
    post)
        run_service_tests "Post Service" "src/backend/services/post-service/tests/WebApp.PostService.Tests"
        OVERALL_RESULT=$?
        ;;
    notification)
        run_service_tests "Notification Service" "src/backend/services/notification-service/tests/WebApp.NotificationService.Tests"
        OVERALL_RESULT=$?
        ;;
    like)
        run_service_tests "Like Service" "src/backend/services/like-service/tests/WebApp.LikeService.Tests"
        OVERALL_RESULT=$?
        ;;
    comment)
        run_service_tests "Comment Service" "src/backend/services/comment-service/tests/WebApp.CommentService.Tests"
        OVERALL_RESULT=$?
        ;;
    media-upload)
        run_service_tests "Media Upload Service" "src/backend/services/media-upload-service/tests/WebApp.MediaUploadService.Tests"
        OVERALL_RESULT=$?
        ;;
    media-processing)
        run_service_tests "Media Processing Service" "src/backend/services/media-processing-service/tests/WebApp.MediaProcessingService.Tests"
        OVERALL_RESULT=$?
        ;;
    gateway)
        run_service_tests "API Gateway" "src/backend/api-gateway/tests/WebApp.Gateway.Tests"
        OVERALL_RESULT=$?
        ;;
    all)
        echo "🧪 Running all service tests..."
        
        # Array of services to test
        declare -a services=(
            "Auth Service:src/backend/services/auth-service/tests/WebApp.AuthService.Tests"
            "User Service:src/backend/services/user-service/tests/WebApp.UserService.Tests"
            "Post Service:src/backend/services/post-service/tests/WebApp.PostService.Tests"
            "Notification Service:src/backend/services/notification-service/tests/WebApp.NotificationService.Tests"
            "Like Service:src/backend/services/like-service/tests/WebApp.LikeService.Tests"
            "Comment Service:src/backend/services/comment-service/tests/WebApp.CommentService.Tests"
            "Media Upload Service:src/backend/services/media-upload-service/tests/WebApp.MediaUploadService.Tests"
            "Media Processing Service:src/backend/services/media-processing-service/tests/WebApp.MediaProcessingService.Tests"
            "API Gateway:src/backend/api-gateway/tests/WebApp.Gateway.Tests"
        )
        
        # Track results
        declare -a failed_services=()
        
        for service_info in "${services[@]}"; do
            IFS=':' read -r service_name service_path <<< "$service_info"
            
            echo ""
            echo "========================================"
            
            run_service_tests "$service_name" "$service_path"
            if [ $? -ne 0 ]; then
                failed_services+=("$service_name")
                OVERALL_RESULT=1
            fi
        done
        
        echo ""
        echo "========================================"
        echo "📊 Test Summary:"
        
        if [ ${#failed_services[@]} -eq 0 ]; then
            echo "✅ All services tests passed!"
        else
            echo "❌ Failed services:"
            for service in "${failed_services[@]}"; do
                echo "   - $service"
            done
        fi
        ;;
esac

echo ""
if [ $OVERALL_RESULT -eq 0 ]; then
    echo "🎉 All tests completed successfully!"
else
    echo "💥 Some tests failed. Check the output above for details."
fi

exit $OVERALL_RESULT
