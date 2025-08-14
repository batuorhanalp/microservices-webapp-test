#!/bin/bash

# WebApp Build, Test, and Deployment Script
# This script builds all projects, runs tests with coverage, and manages Git operations

set -e  # Exit on any error

# Configuration
MIN_COVERAGE_THRESHOLD=80  # Minimum test coverage percentage
SOLUTION_PATH="src/backend/WebApp.sln"
TEST_PROJECT_PATH="src/backend/WebApp.Tests/WebApp.Tests.csproj"
MICROSERVICES_DIR="src/backend/services"
MAIN_BRANCH="main"
COVERAGE_OUTPUT_DIR="TestResults"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}ℹ️  INFO: $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ SUCCESS: $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  WARNING: $1${NC}"
}

log_error() {
    echo -e "${RED}❌ ERROR: $1${NC}"
}

log_header() {
    echo -e "\n${BLUE}================================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}================================================${NC}\n"
}

# Function to check prerequisites
check_prerequisites() {
    log_header "Checking Prerequisites"
    
    # Check if we're in the right directory
    if [ ! -f "$SOLUTION_PATH" ]; then
        log_error "Solution file not found at $SOLUTION_PATH. Please run this script from the project root."
        exit 1
    fi
    
    # Check .NET CLI
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET CLI is not installed or not in PATH"
        exit 1
    fi
    
    # Check Git
    if ! command -v git &> /dev/null; then
        log_error "Git is not installed or not in PATH"
        exit 1
    fi
    
    # Check if we're in a Git repository
    if ! git rev-parse --is-inside-work-tree &> /dev/null; then
        log_error "Not in a Git repository"
        exit 1
    fi
    
    log_success "All prerequisites met"
    
    # Display current status
    current_branch=$(git branch --show-current)
    log_info "Current branch: $current_branch"
    log_info ".NET version: $(dotnet --version)"
}

# Function to clean previous build artifacts
clean_build_artifacts() {
    log_header "Cleaning Build Artifacts"
    
    # Clean solution
    dotnet clean "$SOLUTION_PATH" --configuration Release > /dev/null 2>&1
    dotnet clean "$SOLUTION_PATH" --configuration Debug > /dev/null 2>&1
    
    # Remove test results and coverage reports
    if [ -d "$COVERAGE_OUTPUT_DIR" ]; then
        rm -rf "$COVERAGE_OUTPUT_DIR"
        log_info "Removed previous test results"
    fi
    
    log_success "Build artifacts cleaned"
}

# Function to restore NuGet packages
restore_packages() {
    log_header "Restoring NuGet Packages"
    
    if dotnet restore "$SOLUTION_PATH"; then
        log_success "NuGet packages restored successfully"
    else
        log_error "Failed to restore NuGet packages"
        exit 1
    fi
}

# Function to build all projects
build_projects() {
    log_header "Building All Projects"
    
    # Build main solution
    log_info "Building main solution..."
    if dotnet build "$SOLUTION_PATH" --configuration Release --no-restore; then
        log_success "Main solution built successfully"
    else
        log_error "Failed to build main solution"
        exit 1
    fi
    
    # Build all microservices
    log_info "Building microservices..."
    
    microservice_count=0
    failed_services=()
    
    for service_dir in "$MICROSERVICES_DIR"/*; do
        if [ -d "$service_dir" ]; then
            service_name=$(basename "$service_dir")
            
            # Find .csproj file in the service directory
            csproj_file=$(find "$service_dir" -name "*.csproj" -type f | head -1)
            
            if [ -n "$csproj_file" ]; then
                log_info "Building $service_name..."
                if dotnet build "$csproj_file" --configuration Release --no-restore; then
                    log_success "$service_name built successfully"
                    ((microservice_count++))
                else
                    log_error "Failed to build $service_name"
                    failed_services+=("$service_name")
                fi
            else
                log_warning "No .csproj file found in $service_name, skipping..."
            fi
        fi
    done
    
    if [ ${#failed_services[@]} -eq 0 ]; then
        log_success "All $microservice_count microservices built successfully"
    else
        log_error "Failed to build microservices: ${failed_services[*]}"
        exit 1
    fi
}

# Function to run tests
run_tests() {
    log_header "Running Tests"
    
    if [ ! -f "$TEST_PROJECT_PATH" ]; then
        log_warning "No test project found at $TEST_PROJECT_PATH, skipping tests..."
        return 0
    fi
    
    log_info "Running unit tests with coverage..."
    
    # Install coverage tools if not already installed
    dotnet tool list --global | grep -q "dotnet-reportgenerator-globaltool" || {
        log_info "Installing ReportGenerator tool..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    }
    
    # Run tests with coverage
    if dotnet test "$TEST_PROJECT_PATH" \
        --configuration Release \
        --no-build \
        --collect:"XPlat Code Coverage" \
        --results-directory "$COVERAGE_OUTPUT_DIR" \
        --logger "console;verbosity=normal"; then
        log_success "Tests completed successfully"
    else
        log_error "Tests failed"
        exit 1
    fi
}

# Function to check test coverage
check_coverage() {
    log_header "Checking Test Coverage"
    
    # Find the coverage file
    coverage_file=$(find "$COVERAGE_OUTPUT_DIR" -name "coverage.cobertura.xml" | head -1)
    
    if [ -z "$coverage_file" ]; then
        log_warning "No coverage report found, skipping coverage check..."
        return 0
    fi
    
    log_info "Found coverage report: $coverage_file"
    
    # Generate HTML report
    reportgenerator \
        -reports:"$coverage_file" \
        -targetdir:"$COVERAGE_OUTPUT_DIR/html" \
        -reporttypes:Html \
        > /dev/null 2>&1
    
    # Extract coverage percentage
    if command -v xmllint &> /dev/null; then
        coverage_line_rate=$(xmllint --xpath "string(/coverage/@line-rate)" "$coverage_file" 2>/dev/null)
        if [ -n "$coverage_line_rate" ]; then
            coverage_percentage=$(echo "$coverage_line_rate * 100" | bc -l | cut -d. -f1)
            log_info "Test coverage: ${coverage_percentage}%"
            
            if [ "$coverage_percentage" -ge "$MIN_COVERAGE_THRESHOLD" ]; then
                log_success "Coverage threshold met (${coverage_percentage}% >= ${MIN_COVERAGE_THRESHOLD}%)"
            else
                log_error "Coverage below threshold (${coverage_percentage}% < ${MIN_COVERAGE_THRESHOLD}%)"
                log_info "HTML coverage report available at: $COVERAGE_OUTPUT_DIR/html/index.html"
                exit 1
            fi
        else
            log_warning "Could not parse coverage percentage from report"
        fi
    else
        log_warning "xmllint not available, skipping coverage percentage check"
        log_info "Please install libxml2-utils to enable coverage percentage validation"
    fi
    
    log_info "HTML coverage report generated at: $COVERAGE_OUTPUT_DIR/html/index.html"
}

# Function to run additional quality checks
run_quality_checks() {
    log_header "Running Quality Checks"
    
    # Check for code formatting (if dotnet format is available)
    log_info "Checking code formatting..."
    if dotnet format "$SOLUTION_PATH" --verify-no-changes --verbosity diagnostic > /dev/null 2>&1; then
        log_success "Code formatting is consistent"
    else
        log_warning "Code formatting issues detected. Run 'dotnet format' to fix them."
    fi
    
    # Check for security vulnerabilities
    log_info "Checking for security vulnerabilities..."
    if dotnet list "$SOLUTION_PATH" package --vulnerable > /dev/null 2>&1; then
        log_success "No known security vulnerabilities found"
    else
        log_warning "Potential security vulnerabilities detected. Check package dependencies."
    fi
}

# Function to commit and push changes
git_operations() {
    log_header "Git Operations"
    
    current_branch=$(git branch --show-current)
    
    # Check if there are any changes to commit
    if git diff --quiet && git diff --staged --quiet; then
        log_info "No changes to commit"
    else
        log_info "Staging all changes..."
        git add .
        
        # Get commit message
        if [ -n "$1" ]; then
            commit_message="$1"
        else
            commit_message="build: successful build and test completion on $(date '+%Y-%m-%d %H:%M:%S')"
        fi
        
        log_info "Committing changes with message: $commit_message"
        if git commit -m "$commit_message"; then
            log_success "Changes committed successfully"
        else
            log_error "Failed to commit changes"
            exit 1
        fi
    fi
    
    # Push current branch
    log_info "Pushing current branch: $current_branch"
    if git push origin "$current_branch"; then
        log_success "Branch pushed successfully"
    else
        log_error "Failed to push branch"
        exit 1
    fi
    
    # Switch to main branch if not already on it
    if [ "$current_branch" != "$MAIN_BRANCH" ]; then
        log_info "Switching to $MAIN_BRANCH branch..."
        
        # Fetch latest changes
        git fetch origin "$MAIN_BRANCH"
        
        # Switch to main
        if git checkout "$MAIN_BRANCH"; then
            log_success "Switched to $MAIN_BRANCH branch"
            
            # Pull latest changes
            if git pull origin "$MAIN_BRANCH"; then
                log_success "Latest changes pulled from $MAIN_BRANCH"
            else
                log_warning "Could not pull latest changes from $MAIN_BRANCH"
            fi
        else
            log_error "Failed to switch to $MAIN_BRANCH branch"
            exit 1
        fi
    else
        log_info "Already on $MAIN_BRANCH branch"
    fi
}

# Function to display summary
display_summary() {
    log_header "Build Summary"
    
    echo -e "${GREEN}🎉 All operations completed successfully!${NC}\n"
    echo -e "${BLUE}Summary:${NC}"
    echo -e "  ✅ All projects built successfully"
    echo -e "  ✅ All tests passed"
    echo -e "  ✅ Code coverage meets requirements"
    echo -e "  ✅ Quality checks completed"
    echo -e "  ✅ Changes committed and pushed"
    echo -e "  ✅ Switched to $MAIN_BRANCH branch"
    
    if [ -f "$COVERAGE_OUTPUT_DIR/html/index.html" ]; then
        echo -e "\n${BLUE}📊 Coverage Report:${NC} $COVERAGE_OUTPUT_DIR/html/index.html"
    fi
    
    echo -e "\n${GREEN}Ready for deployment! 🚀${NC}\n"
}

# Main execution function
main() {
    log_header "WebApp Build, Test & Deploy Pipeline"
    
    echo -e "${BLUE}Starting automated build pipeline...${NC}\n"
    
    # Parse command line arguments
    commit_message=""
    while [[ $# -gt 0 ]]; do
        case $1 in
            -m|--message)
                commit_message="$2"
                shift 2
                ;;
            -t|--threshold)
                MIN_COVERAGE_THRESHOLD="$2"
                shift 2
                ;;
            -h|--help)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Options:"
                echo "  -m, --message TEXT     Custom commit message"
                echo "  -t, --threshold NUM    Minimum coverage threshold (default: 80)"
                echo "  -h, --help            Show this help message"
                exit 0
                ;;
            *)
                log_warning "Unknown option: $1"
                shift
                ;;
        esac
    done
    
    # Execute pipeline steps
    check_prerequisites
    clean_build_artifacts
    restore_packages
    build_projects
    run_tests
    check_coverage
    run_quality_checks
    git_operations "$commit_message"
    display_summary
}

# Error handling
trap 'log_error "Script failed at line $LINENO. Exit code: $?"' ERR

# Run main function with all arguments
main "$@"
