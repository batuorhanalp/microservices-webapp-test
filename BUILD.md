# Build Scripts Documentation

This document describes the automated build and deployment scripts for the WebApp project.

## Scripts Overview

### 🚀 `build-and-deploy.sh` - Full CI/CD Pipeline
The comprehensive script that performs a complete build, test, and deployment pipeline.

**Features:**
- ✅ Prerequisites checking (.NET CLI, Git, repository structure)
- 🧹 Clean build artifacts
- 📦 NuGet package restoration
- 🔨 Build main solution and all microservices
- 🧪 Run unit tests with code coverage
- 📊 Generate HTML coverage reports
- 🎯 Validate coverage threshold (default: 80%)
- 🔍 Code quality checks (formatting, security vulnerabilities)
- 📝 Git operations (commit, push, branch switching)
- 🎉 Comprehensive reporting

**Usage:**
```bash
# Basic usage
./build-and-deploy.sh

# With custom commit message
./build-and-deploy.sh -m "feat: implement notification service"

# With custom coverage threshold
./build-and-deploy.sh -t 75

# Combined options
./build-and-deploy.sh -m "fix: resolve build issues" -t 85

# Show help
./build-and-deploy.sh --help
```

**Options:**
- `-m, --message TEXT`: Custom commit message
- `-t, --threshold NUM`: Minimum coverage threshold (default: 80)
- `-h, --help`: Show help message

### ⚡ `quick-build.sh` - Development Build
A lightweight script for quick development builds and testing.

**Features:**
- 🔨 Build main solution
- 🔧 Build all microservices
- 🧪 Run unit tests
- 📊 Quick status reporting

**Usage:**
```bash
./quick-build.sh
```

## Pipeline Flow

### Full Pipeline (`build-and-deploy.sh`)
```
1. Check Prerequisites
   └─ Verify .NET CLI, Git, project structure
   
2. Clean Build Artifacts
   └─ Remove old builds and test results
   
3. Restore NuGet Packages
   └─ Download dependencies
   
4. Build Projects
   ├─ Build main solution
   └─ Build all microservices
   
5. Run Tests
   ├─ Execute unit tests
   └─ Generate code coverage
   
6. Check Coverage
   ├─ Generate HTML report
   └─ Validate threshold
   
7. Quality Checks
   ├─ Code formatting
   └─ Security vulnerabilities
   
8. Git Operations
   ├─ Commit changes
   ├─ Push current branch
   └─ Switch to main
   
9. Display Summary
   └─ Show results and coverage report
```

## Project Structure

The scripts expect the following project structure:
```
webapp-production/
├── src/backend/
│   ├── WebApp.sln                    # Main solution file
│   ├── WebApp.Tests/                 # Test project
│   │   └── WebApp.Tests.csproj
│   └── services/                     # Microservices
│       ├── auth-service/
│       ├── comment-service/
│       ├── like-service/
│       ├── notification-service/
│       ├── post-service/
│       └── user-service/
├── build-and-deploy.sh              # Full CI/CD script
├── quick-build.sh                   # Quick development build
└── BUILD.md                         # This documentation
```

## Prerequisites

### Required Tools
- **.NET 9.0 SDK or later**
- **Git**
- **bash** (Available on macOS/Linux, or WSL on Windows)

### Optional Tools (for enhanced functionality)
- **xmllint** (for coverage percentage parsing)
- **bc** (for mathematical calculations)

### macOS Installation
```bash
# Install .NET
brew install --cask dotnet

# xmllint and bc are usually pre-installed
# If missing, install with:
brew install libxml2
```

## Coverage Reports

When tests are run, coverage reports are generated in the `TestResults/` directory:

- **XML Report**: `TestResults/{guid}/coverage.cobertura.xml`
- **HTML Report**: `TestResults/html/index.html`

The HTML report provides detailed coverage information by file and method.

## Configuration

### Environment Variables
You can set these environment variables to customize behavior:

```bash
export MIN_COVERAGE_THRESHOLD=85    # Override default threshold
export MAIN_BRANCH="develop"        # Change main branch name
```

### Script Configuration
Edit the configuration section in `build-and-deploy.sh`:

```bash
# Configuration
MIN_COVERAGE_THRESHOLD=80           # Minimum test coverage percentage
SOLUTION_PATH="src/backend/WebApp.sln"
TEST_PROJECT_PATH="src/backend/WebApp.Tests/WebApp.Tests.csproj"
MICROSERVICES_DIR="src/backend/services"
MAIN_BRANCH="main"
COVERAGE_OUTPUT_DIR="TestResults"
```

## Troubleshooting

### Common Issues

**Script Permission Denied**
```bash
chmod +x build-and-deploy.sh
chmod +x quick-build.sh
```

**Solution File Not Found**
- Ensure you're running the script from the project root
- Verify the `SOLUTION_PATH` in the script configuration

**Coverage Tools Missing**
```bash
# Install ReportGenerator globally
dotnet tool install --global dotnet-reportgenerator-globaltool

# Install xmllint (if needed)
brew install libxml2  # macOS
apt-get install libxml2-utils  # Ubuntu
```

**Build Failures**
1. Check .NET version: `dotnet --version`
2. Restore packages manually: `dotnet restore`
3. Clean solution: `dotnet clean`
4. Check for compilation errors in individual projects

**Git Issues**
- Ensure you're in a Git repository
- Check remote configuration: `git remote -v`
- Verify branch exists: `git branch -a`

### Debug Mode
To run with more verbose output, modify the scripts:
```bash
# Add debug flag at the top of the script
set -x  # Enable debug mode
```

## Integration with CI/CD

### GitHub Actions Example
```yaml
name: Build and Deploy

on:
  push:
    branches: [ feature/* ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    - name: Run Build Pipeline
      run: ./build-and-deploy.sh -m "ci: automated build"
```

### Azure DevOps Pipeline
```yaml
trigger:
- feature/*

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '9.0.x'
- script: ./build-and-deploy.sh -m "ci: automated build"
  displayName: 'Build and Deploy'
```

## Best Practices

1. **Run quick-build.sh frequently** during development
2. **Use build-and-deploy.sh** before merging to main
3. **Maintain test coverage** above the threshold
4. **Review coverage reports** for untested code
5. **Fix formatting issues** before committing
6. **Keep dependencies updated** and secure

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review script output for detailed error messages
3. Ensure all prerequisites are installed
4. Verify project structure matches expectations

---

*Last updated: $(date '+%Y-%m-%d')*
