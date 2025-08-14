---
name: 🐛 Bug Report
about: Create a report to help us improve
title: '[BUG] '
labels: ['bug', 'needs-triage']
assignees: ''
---

# 🐛 Bug Report

## 📝 Bug Description

**A clear and concise description of what the bug is.**

## 🔄 Steps to Reproduce

1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

## ✅ Expected Behavior

**A clear and concise description of what you expected to happen.**

## ❌ Actual Behavior

**A clear and concise description of what actually happened.**

## 📸 Screenshots/Logs

**If applicable, add screenshots or error logs to help explain your problem.**

```
Paste error logs here
```

## 💻 Environment

**Please complete the following information:**

- **OS**: [e.g. Windows 11, macOS 13, Ubuntu 22.04]
- **.NET Version**: [e.g. 8.0.5]
- **Browser** (if applicable): [e.g. Chrome 118, Firefox 119]
- **API Version**: [e.g. v1.0.0]
- **Database**: [e.g. PostgreSQL 15.2]

## 📋 API Request Details (if applicable)

**For API-related bugs:**

- **Endpoint**: `GET/POST/PUT/DELETE /api/...`
- **Request Headers**:
  ```json
  {
    "Authorization": "Bearer ...",
    "Content-Type": "application/json"
  }
  ```
- **Request Body**:
  ```json
  {
    "example": "data"
  }
  ```
- **Response Status**: [e.g. 400, 500]
- **Response Body**:
  ```json
  {
    "error": "error message"
  }
  ```

## 🔍 Additional Context

**Add any other context about the problem here.**

## 🧪 Test Coverage Impact

- [ ] This bug affects existing tests
- [ ] New tests needed to prevent regression
- [ ] No test coverage impact

## 🔒 Security Impact

- [ ] This bug has security implications
- [ ] This bug involves sensitive data
- [ ] No security impact

## 🎯 Priority

**How critical is this bug?**
- [ ] 🔥 Critical (system down, security issue)
- [ ] ⚡ High (major functionality broken)
- [ ] 📋 Medium (minor functionality affected)
- [ ] 🔧 Low (cosmetic, nice-to-have)

---

## ✅ Acceptance Criteria for Fix

**What needs to be done to consider this bug fixed?**

- [ ] Bug is reproduced and root cause identified
- [ ] Fix is implemented with proper tests
- [ ] All existing tests continue to pass
- [ ] Coverage remains ≥ 97%
- [ ] Fix is documented (if needed)
