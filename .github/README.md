# 🔧 GitHub Actions CI/CD Setup

This directory contains automated workflows that enforce code quality, security, and testing standards for our social media backend API.

## 📋 Available Workflows

### 1. **CI - Build, Test, and Coverage** (`ci.yml`)
**Triggers:** Push to `main`/`develop`, Pull Requests
- ✅ Builds the .NET 8 solution
- 🧪 Runs all unit tests (166+ tests)
- 📊 Generates detailed coverage reports
- 🎯 **Enforces minimum 97% code coverage**
- 📝 Comments PR with coverage details
- ⬆️ Uploads test results and coverage artifacts

### 2. **Quality Gate** (`quality-gate.yml`)
**Triggers:** Pull Requests to `main`
- 🛡️ Enforces quality standards before merge
- 🔍 Basic security scans
- 📈 Mandatory coverage threshold (97%)
- 🚫 **Blocks PR merge if quality checks fail**

## 🛡️ Quality Standards Enforced

| Standard | Requirement | Action on Failure |
|----------|-------------|-------------------|
| **Unit Tests** | All tests must pass | ❌ Block PR merge |
| **Code Coverage** | ≥ 97% line coverage | ❌ Block PR merge |
| **Build** | Must compile without errors | ❌ Block PR merge |
| **Security** | Basic vulnerability scanning | ⚠️ Warning only |

## 📊 Coverage Reporting

Our CI pipeline provides multiple coverage reporting formats:

- **HTML Report**: Interactive coverage visualization
- **JSON Summary**: Machine-readable coverage data
- **Badges**: Coverage percentage badges
- **PR Comments**: Automatic coverage feedback

### Coverage Metrics Tracked:
- 📏 **Line Coverage**: Primary metric (≥97% required)
- 🌲 **Branch Coverage**: Decision points coverage
- 🔧 **Method Coverage**: Function-level coverage

## 🚀 Getting Started

### For Contributors:

1. **Fork the repository**
2. **Create a feature branch** from `main`
3. **Write your code** with appropriate tests
4. **Ensure coverage ≥ 97%** locally:
   ```bash
   cd src/backend
   dotnet test --collect:"XPlat Code Coverage"
   reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport"
   ```
5. **Create a Pull Request** to `main`
6. **Wait for CI checks** to pass ✅
7. **Address any failures** indicated by the workflows

### Local Testing Commands:

```bash
# Restore dependencies
dotnet restore src/backend/WebApp.sln

# Build solution
dotnet build src/backend/WebApp.sln --configuration Release

# Run tests with coverage
dotnet test src/backend/WebApp.Tests/WebApp.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"CoverageReport" \
  -reporttypes:"Html;TextSummary"
```

## 🔧 Branch Protection Rules

**Recommended GitHub repository settings:**

### Main Branch Protection:
- ✅ **Require pull request reviews before merging**
- ✅ **Require status checks to pass before merging**
  - `Build and Test` (from ci.yml)
  - `Enforce Quality Standards` (from quality-gate.yml)
- ✅ **Require up-to-date branches before merging**
- ✅ **Require linear history**
- ✅ **Include administrators** (enforce rules for everyone)

### Required Status Checks:
```
CI - Build, Test, and Coverage / test
Quality Gate / quality-gate
```

## 🎯 Quality Metrics

Our current project maintains:
- **166+ Unit Tests** with comprehensive coverage
- **96.6%+ Line Coverage** (target: 97%+)
- **95.5%+ Branch Coverage**
- **92.8%+ Method Coverage**

## 🔍 Troubleshooting

### Common Issues:

#### ❌ **Coverage Below 97%**
```bash
# Check which files need more tests
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"report"
open report/index.html  # View detailed coverage report
```

#### ❌ **Tests Failing**
```bash
# Run tests with detailed output
dotnet test --verbosity diagnostic
```

#### ❌ **Build Failures**
```bash
# Check for compilation errors
dotnet build --verbosity normal
```

## 📈 Continuous Improvement

### Monitoring Coverage Trends:
- 📊 Coverage reports archived as GitHub artifacts
- 📈 Historical coverage tracking via Codecov integration
- 🎯 Automatic coverage badges in README

### Security Scanning:
- 🔍 Basic pattern-based security checks
- 🛡️ Dependency vulnerability scanning (planned)
- 🔒 Secret detection (basic patterns)

## 📝 Contributing to CI/CD

To modify or enhance the CI/CD pipeline:

1. Update workflow files in `.github/workflows/`
2. Test changes in a feature branch
3. Ensure all existing quality gates still pass
4. Document any new requirements or changes

---

## 🤝 Support

If you encounter issues with the CI/CD pipeline:
1. Check the **Actions** tab in GitHub for detailed logs
2. Review the **Coverage Report** artifacts
3. Consult this documentation
4. Create an issue with relevant logs and context

**Remember**: These quality gates ensure our codebase remains maintainable, secure, and reliable! 🚀
