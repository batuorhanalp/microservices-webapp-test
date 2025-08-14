# 🚀 Pull Request

## 📝 Description

**Brief summary of changes:**
<!-- Describe what this PR does and why -->

**Related Issue(s):**
<!-- Link to any related GitHub issues -->
- Fixes #
- Closes #
- Related to #

## 🔍 Type of Change

<!-- Check the relevant boxes -->
- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📚 Documentation update
- [ ] 🔧 Configuration change
- [ ] 🧪 Test improvements
- [ ] ♻️ Code refactoring (no functional changes)
- [ ] ⚡ Performance improvement

## 🧪 Testing & Quality Checklist

### ✅ **Mandatory Quality Gates** (CI will verify these)
- [ ] All unit tests pass locally (`dotnet test`)
- [ ] Code coverage ≥ 97% (`reportgenerator` output)
- [ ] Solution builds without errors (`dotnet build`)
- [ ] No obvious security vulnerabilities

### 📋 **Developer Checklist** 
- [ ] Added unit tests for new functionality
- [ ] Updated existing tests for modified functionality  
- [ ] Added integration tests where appropriate
- [ ] Code follows established patterns and conventions
- [ ] Added XML documentation for public APIs
- [ ] Updated README.md if needed
- [ ] Added/updated database migrations if applicable

### 🔒 **Security Checklist**
- [ ] No hardcoded secrets, passwords, or API keys
- [ ] Input validation for all user inputs
- [ ] SQL injection protection (using parameterized queries)
- [ ] Proper authentication/authorization checks
- [ ] No sensitive data in logs

### 📊 **Coverage Report**
<!-- Paste your local coverage report summary here -->
```
Current Coverage: X.X%
Previous Coverage: X.X%  
Coverage Change: +/-X.X%
```

## 📋 Test Results

<!-- Paste test results summary -->
```bash
Test run summary: 
  Total: XXX, Passed: XXX, Failed: 0, Skipped: 0
  Duration: X.Xs
```

## 🏗️ Database Changes

<!-- Check if applicable -->
- [ ] No database changes
- [ ] Migration files added
- [ ] Migration tested locally
- [ ] Seed data updated (if needed)
- [ ] Database indexes reviewed for performance

## 📱 API Changes

<!-- If this PR affects the API -->
- [ ] No API changes
- [ ] New endpoints added (documented with Swagger)
- [ ] Existing endpoints modified (backward compatible)
- [ ] Breaking API changes (version bump required)
- [ ] Updated API documentation

## 🚀 Deployment Notes

<!-- Any special deployment considerations -->
- [ ] No special deployment steps required
- [ ] Requires environment variable changes
- [ ] Requires configuration updates
- [ ] Requires database migration
- [ ] Requires service restart

## 📸 Screenshots/Evidence

<!-- Add screenshots, logs, or other evidence of your changes working -->
<!-- For UI changes, API changes, etc. -->

## 🔗 Additional Context

<!-- Add any other context about the PR here -->
<!-- Links to documentation, design docs, etc. -->

---

## 🤖 CI/CD Status

The following automated checks will run on this PR:

- 🏗️ **Build Check**: Ensures solution compiles
- 🧪 **Unit Tests**: All 166+ tests must pass  
- 📊 **Coverage Gate**: Must maintain ≥97% coverage
- 🛡️ **Quality Gate**: Security and code quality checks
- 📝 **Coverage Report**: Automatic coverage feedback

⚠️ **PR cannot be merged until all quality gates pass** ⚠️

## 👥 Reviewers

<!-- Tag relevant reviewers -->
@mention-reviewers

---

**By submitting this PR, I confirm that:**
- ✅ I have tested my changes locally
- ✅ I have added appropriate unit tests  
- ✅ My code follows the project's coding standards
- ✅ I have updated documentation as needed
- ✅ My changes do not introduce security vulnerabilities
