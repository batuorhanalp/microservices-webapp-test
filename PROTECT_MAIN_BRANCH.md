# 🚨 URGENT: Protect Main Branch Setup

## 🛡️ **Manual Setup (5 minutes)**

Since GitHub CLI is not installed, follow these steps to **immediately secure your main branch**:

### **Step 1: Go to Repository Settings**
1. **Open:** https://github.com/batuorhanalp/microservices-webapp-test
2. **Click:** "Settings" tab (top of repository)
3. **Select:** "Branches" from left sidebar

### **Step 2: Add Branch Protection Rule**
1. **Click:** "Add rule" button
2. **Branch name pattern:** `main`

### **Step 3: Enable These Settings** ✅

#### **Protect Against Pushes:**
```
✅ Require a pull request before merging
    ✅ Require approvals: 1
    ✅ Dismiss stale PR approvals when new commits are pushed
    ✅ Require last push approval
```

#### **Require Status Checks:**
```
✅ Require status checks to pass before merging
    ✅ Require branches to be up to date before merging
```

**Status checks to add (after first workflow runs):**
- `CI - Build, Test, and Coverage / test`
- `Quality Gate / quality-gate`

#### **Additional Protection:**
```
✅ Require conversation resolution before merging
✅ Require linear history  
✅ Include administrators (CRITICAL!)
✅ Restrict pushes that create files larger than 100MB

❌ Allow force pushes (keep UNCHECKED)
❌ Allow deletions (keep UNCHECKED)
```

### **Step 4: Save**
**Click:** "Create" button

---

## ✅ **Verification**

Test that protection is working:

```bash
# This should now FAIL:
git push origin main
# Expected error: "Push declined due to branch protection rule"
```

---

## 🚀 **Automated Setup Alternative**

If you want to use the automated script later:

```bash
# Install GitHub CLI first
brew install gh

# Authenticate
gh auth login

# Run our script
./scripts/setup-branch-protection.sh
```

---

## 📋 **What This Achieves**

✅ **No direct pushes to main** - All changes must go through PRs  
✅ **Mandatory code reviews** - Requires 1 approval minimum  
✅ **CI/CD enforcement** - All tests must pass, coverage ≥ 97%  
✅ **Applies to admins** - Even you can't bypass these rules  
✅ **No force pushes** - Prevents destructive changes  
✅ **No branch deletion** - Main branch cannot be deleted  

**Your main branch is now secure!** 🛡️

---

## ⚠️ **Important: New Workflow**

From now on, use this workflow:

```bash
# 1. Create feature branch
git checkout -b feature/your-feature-name

# 2. Make your changes
git add .
git commit -m "Your changes"

# 3. Push feature branch (allowed)
git push origin feature/your-feature-name

# 4. Create PR on GitHub
# 5. Wait for CI/CD to pass (97% coverage required)
# 6. Get 1 approval
# 7. Merge PR
```

**Main branch is now PROTECTED!** 🎉
