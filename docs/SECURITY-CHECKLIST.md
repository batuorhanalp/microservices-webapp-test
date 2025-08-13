# 🔐 Repository Security Checklist

This checklist ensures your production web app repository maintains the highest security standards and never exposes sensitive information.

## ✅ **Pre-Commit Security Checklist**

Before committing any code, verify:

### **🚨 Critical Security Items**

- [ ] **No hardcoded passwords** in any files
- [ ] **No API keys or tokens** in code or configuration
- [ ] **No private keys or certificates** committed
- [ ] **No database connection strings** with credentials
- [ ] **No cloud provider credentials** (AWS keys, Azure SAS tokens, etc.)
- [ ] **No Kubernetes secrets or kubeconfig** files
- [ ] **No `.env` files** with actual values (only `.env.example`)
- [ ] **No Terraform state files** (`.tfstate`, `.tfstate.backup`)
- [ ] **No Terraform plan files** with sensitive data

### **🛡️ Infrastructure Security**

- [ ] All **Terraform variables** using secure defaults or Key Vault references
- [ ] **No hardcoded IPs or domains** in production configurations
- [ ] **No embedded certificates** in code
- [ ] **No service account keys** in repository
- [ ] **SSH keys** not committed to repository
- [ ] **Docker secrets** not in Dockerfiles or compose files

### **📝 Configuration Files**

- [ ] All **`.tfvars` files** are gitignored (except examples)
- [ ] **Environment-specific configs** use placeholders or Key Vault
- [ ] **Database migrations** don't contain production data
- [ ] **Log files** are gitignored
- [ ] **Backup files** are gitignored

## 🔍 **Security Scanning Commands**

Run these commands before each commit:

### **1. Search for Potential Secrets**
```bash
# Search for potential passwords
grep -r -i "password.*=" . --exclude-dir=.git --exclude-dir=node_modules

# Search for API keys
grep -r -E "(api[_-]?key|secret[_-]?key)" . --exclude-dir=.git --exclude-dir=node_modules

# Search for tokens
grep -r -E "(token|bearer)" . --exclude-dir=.git --exclude-dir=node_modules

# Search for connection strings
grep -r -E "(connection[_-]?string|jdbc:|mongodb:|redis://)" . --exclude-dir=.git --exclude-dir=node_modules
```

### **2. Check for Terraform State Files**
```bash
find . -name "*.tfstate*" -o -name "*.tfplan" -o -name "terraform.tfvars"
```

### **3. Validate .gitignore Coverage**
```bash
git status --ignored
```

### **4. Check for Large Files (might contain secrets)**
```bash
find . -size +1M -type f ! -path "./.git/*" ! -path "./node_modules/*"
```

## 🛠️ **Automated Security Tools**

### **Install Git Hooks for Security**

Create a pre-commit hook to prevent accidental secret commits:

```bash
#!/bin/sh
# .git/hooks/pre-commit

echo "🔐 Running security checks..."

# Check for common secrets patterns
if git diff --cached --name-only | xargs grep -l -E "(password|api[_-]?key|secret|token)" 2>/dev/null; then
    echo "❌ Potential secrets found! Please review and remove sensitive data."
    exit 1
fi

# Check for terraform state files
if git diff --cached --name-only | grep -E "\.(tfstate|tfplan)$"; then
    echo "❌ Terraform state/plan files detected! These should not be committed."
    exit 1
fi

echo "✅ Security checks passed."
```

### **Recommended Security Tools**

1. **git-secrets** (Amazon's tool):
   ```bash
   brew install git-secrets
   git secrets --install
   git secrets --register-aws
   ```

2. **truffleHog** (Search for secrets):
   ```bash
   pip install truffleHog
   trufflehog --regex --entropy=False .
   ```

3. **detect-secrets** (Yelp's tool):
   ```bash
   pip install detect-secrets
   detect-secrets scan --all-files .
   ```

## 📋 **File-by-File Security Review**

### **✅ Safe to Commit**
- `README.md`, `CHANGELOG.md`, `LICENSE`
- Example/template files (`*.example`, `*.template`)
- Documentation files (`docs/`)
- Infrastructure code (`.tf` files without secrets)
- Application source code (without embedded secrets)
- Docker files (without embedded secrets)
- CI/CD configuration files (without secrets)

### **❌ Never Commit**
- `.env` files with actual values
- `*.tfvars` files with real credentials
- `*.tfstate*` (Terraform state files)
- `kubeconfig` or `*.kubeconfig`
- Private keys (`*.key`, `*.pem`, `id_rsa*`)
- Certificates with private keys (`*.p12`, `*.pfx`)
- Cloud provider credential files
- Database dumps with real data
- Log files with sensitive information

### **⚠️ Review Carefully**
- Configuration files (`config/`, `settings/`)
- Docker Compose files
- Kubernetes manifests
- Shell scripts
- Build scripts
- Test files (might contain test credentials)

## 🚨 **Emergency Response: Secrets Accidentally Committed**

If you accidentally commit secrets:

### **1. Immediate Actions**
```bash
# DO NOT just remove the file in a new commit - the secret is still in git history!

# Option A: Remove from last commit (if not pushed yet)
git reset --soft HEAD~1
git reset HEAD <file-with-secret>
# Edit the file to remove secret, then recommit

# Option B: Rewrite history (DANGEROUS - only if not pushed)
git filter-branch --force --index-filter 'git rm --cached --ignore-unmatch <file-with-secret>' --prune-empty --tag-name-filter cat -- --all

# Option C: Use BFG Repo-Cleaner (recommended)
java -jar bfg.jar --replace-text passwords.txt your-repo.git
```

### **2. Security Actions**
- **Immediately rotate** the compromised secret
- **Check access logs** for unauthorized usage
- **Notify security team** if applicable
- **Update all systems** using the old secret

### **3. Prevention**
- Add the secret pattern to `.gitignore`
- Set up git hooks to prevent future accidents
- Use environment variables or Key Vault for secrets

## 📊 **Security Metrics to Monitor**

Track these security indicators:

### **Repository Health**
- [ ] **Zero secrets** detected in repository scans
- [ ] **All sensitive files** properly gitignored
- [ ] **Pre-commit hooks** active and functional
- [ ] **Security scanning** integrated in CI/CD
- [ ] **Regular security audits** scheduled

### **Access Control**
- [ ] **Branch protection** enabled on main branches
- [ ] **Required reviews** for all changes
- [ ] **Limited admin access** to repository
- [ ] **Two-factor authentication** required
- [ ] **Regular access review** conducted

## 🎯 **Security Best Practices**

### **Development Workflow**
1. **Always use examples** for configuration templates
2. **Store secrets in Key Vault** or environment variables
3. **Use placeholders** in documentation
4. **Review commits** before pushing
5. **Rotate secrets regularly**

### **Code Review Guidelines**
1. **Check for embedded secrets** in every PR
2. **Verify .gitignore coverage** for new file types
3. **Test with fake data** only
4. **Document secret management** approach
5. **Validate security assumptions**

### **Production Deployment**
1. **Secrets injected at runtime** only
2. **No secrets in container images**
3. **Environment-based configuration**
4. **Audit all secret access**
5. **Monitor for secret exposure**

## 🔗 **Additional Resources**

- **OWASP Secrets Management**: https://owasp.org/www-project-secrets-management/
- **GitHub Security Best Practices**: https://docs.github.com/en/code-security
- **Azure Key Vault Best Practices**: https://docs.microsoft.com/azure/key-vault/general/best-practices
- **Terraform Security Best Practices**: https://blog.gitguardian.com/terraform-secrets/
- **Docker Security Best Practices**: https://docs.docker.com/develop/security-best-practices/

## 🎖️ **Security Certification**

Once you've implemented all these security measures, your repository will meet:
- ✅ **OWASP Top 10** security requirements
- ✅ **SOC 2 Type II** compliance standards  
- ✅ **PCI DSS** security requirements
- ✅ **Enterprise security** best practices
- ✅ **Zero Trust** architecture principles

---

**Remember**: Security is everyone's responsibility! When in doubt, ask for a security review. 🛡️
