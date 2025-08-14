# 🛡️ Branch Protection Setup Guide

This guide explains how to set up strict branch protection rules to prevent accidental direct pushes to the main branch.

## 🎯 **Goal**
- **Block all direct pushes to main branch**
- **Force all changes through Pull Requests**
- **Ensure CI/CD checks pass before merge**
- **Apply rules to administrators too**

## 🚀 **Method 1: Automated Setup (Recommended)**

### Prerequisites:
- GitHub CLI installed: `brew install gh` (macOS) or visit https://cli.github.com/
- Authenticated with GitHub: `gh auth login`

### Run the Setup Script:
```bash
# Make script executable
chmod +x scripts/setup-branch-protection.sh

# Run the setup
./scripts/setup-branch-protection.sh
```

The script will configure:
- ✅ Require PR reviews (1 approval minimum)
- ✅ Dismiss stale reviews on new commits  
- ✅ Require status checks to pass
- ✅ Enforce linear history
- ✅ Include administrators
- ✅ Block force pushes and deletions

---

## 🖱️ **Method 2: Manual Setup via GitHub Web Interface**

### Step 1: Navigate to Settings
1. Go to: `https://github.com/batuorhanalp/microservices-webapp-test`
2. Click **"Settings"** tab
3. Select **"Branches"** from left sidebar

### Step 2: Create Branch Protection Rule
1. Click **"Add rule"** button
2. Enter branch name pattern: `main`

### Step 3: Configure Protection Settings

#### **General Protection:**
```
✅ Restrict pushes that create files larger than 100MB
```

#### **Pull Request Requirements:**
```
✅ Require a pull request before merging
    ✅ Require approvals: 1 (minimum)
    ✅ Dismiss stale PR approvals when new commits are pushed
    ✅ Require review from code owners (optional)
    ✅ Require last push approval
```

#### **Status Check Requirements:**
```
✅ Require status checks to pass before merging
    ✅ Require branches to be up to date before merging
    
Required status checks (select these):
- "CI - Build, Test, and Coverage / test"
- "Quality Gate / quality-gate"
```

#### **Additional Restrictions:**
```
✅ Require conversation resolution before merging
✅ Require linear history
✅ Include administrators (CRITICAL)
❌ Allow force pushes (keep DISABLED)
❌ Allow deletions (keep DISABLED)
```

### Step 4: Save Configuration
Click **"Create"** to apply the rules.

---

## 🔍 **Verification**

After setup, verify the protection is working:

### Test 1: Try Direct Push (Should Fail)
```bash
# This should be blocked
echo "test" > test.txt
git add test.txt
git commit -m "test direct push"
git push origin main
# Expected: Error - push declined due to branch protection
```

### Test 2: Proper Workflow (Should Work)
```bash
# Create feature branch
git checkout -b feature/test-protection
echo "test" > test.txt
git add test.txt
git commit -m "test via PR"
git push origin feature/test-protection

# Then create PR on GitHub - should work fine
```

---

## 📋 **What Happens Now**

### ✅ **Protected Workflow:**
1. **Create feature branch** from main
2. **Make changes** in feature branch
3. **Push to feature branch** (allowed)
4. **Create Pull Request** to main
5. **CI/CD runs automatically**:
   - All tests must pass (166+ tests)
   - Coverage must be ≥ 97%
   - Build must succeed
   - Security checks must pass
6. **Require 1 approval** from team member
7. **Merge only after all checks pass**

### ❌ **Blocked Actions:**
- Direct pushes to main
- Force pushes to main
- Deleting main branch
- Bypassing CI/CD checks
- Merging without approvals

---

## 🚨 **Important Notes**

### **For Repository Administrators:**
Even with admin privileges, you **cannot bypass** these rules. This is intentional to maintain code quality.

### **Emergency Situations:**
If you need to bypass protection temporarily:
1. Go to Settings → Branches
2. Temporarily disable protection
3. Make emergency fix
4. Re-enable protection immediately

### **Status Check Names:**
The status check names must match exactly:
- `"CI - Build, Test, and Coverage / test"`
- `"Quality Gate / quality-gate"`

These names come from your workflow files.

---

## 🔧 **Troubleshooting**

### **Issue: Status Checks Not Appearing**
**Solution:** Status checks only appear after workflows run at least once. Create a test PR first.

### **Issue: Script Fails with Permission Error**
**Solution:** 
```bash
# Ensure you're authenticated
gh auth status

# Re-authenticate if needed
gh auth login --with-token
```

### **Issue: Cannot Push to Feature Branches**
**Solution:** Branch protection only applies to `main`. Feature branches are unrestricted.

### **Issue: PR Checks Not Running**
**Solution:** Ensure workflow files are in the main branch and properly configured.

---

## 📊 **Quality Standards Enforced**

With branch protection enabled, every change must meet:

| Standard | Requirement | Enforced By |
|----------|-------------|-------------|
| **Unit Tests** | All tests pass | CI Workflow |
| **Code Coverage** | ≥ 97% line coverage | Quality Gate |
| **Build** | Clean compilation | CI Workflow |
| **Security** | Basic vulnerability scan | Quality Gate |
| **Review** | 1+ approval required | Branch Protection |
| **Linear History** | No merge commits | Branch Protection |

---

## 🎉 **Benefits**

✅ **Prevents accidental direct pushes**  
✅ **Maintains code quality standards**  
✅ **Forces peer review process**  
✅ **Ensures all changes are tested**  
✅ **Maintains high test coverage**  
✅ **Provides audit trail of changes**  
✅ **Prevents breaking changes**  

Your main branch is now **enterprise-grade secure**! 🚀
