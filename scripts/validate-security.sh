#!/bin/bash

# Security Validation Script
# Validates that no secrets are committed to the repository
# Run this before deployment and as part of CI/CD

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ERRORS_FOUND=0

echo -e "${BLUE}🔐 Security Validation for Production Web App${NC}"
echo -e "${BLUE}=============================================${NC}"
echo "Project Root: $PROJECT_ROOT"
echo ""

# Function to print status messages
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[✅ PASS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[⚠️  WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[❌ FAIL]${NC} $1"
    ERRORS_FOUND=$((ERRORS_FOUND + 1))
}

# Change to project root
cd "$PROJECT_ROOT"

log_info "Starting comprehensive security validation..."

# ===== CHECK 1: Search for potential passwords =====
log_info "Checking for hardcoded passwords..."
FOUND_PASSWORDS=$(grep -r -i -E "(password\s*=\s*['\"][^'\"]{3,}['\"]|password\s*:\s*['\"][^'\"]{3,}['\"])" . \
    --exclude-dir=.git \
    --exclude-dir=node_modules \
    --exclude-dir=.terraform \
    --exclude="*.log" \
    --exclude="*.md" \
    --exclude=".env.example" \
    --exclude="validate-security.sh" \
    2>/dev/null || true)

if [ -n "$FOUND_PASSWORDS" ]; then
    log_error "Potential hardcoded passwords found:"
    echo "$FOUND_PASSWORDS"
else
    log_success "No hardcoded passwords detected"
fi

# ===== CHECK 2: Search for API keys and tokens =====
log_info "Checking for API keys and tokens..."
FOUND_KEYS=$(grep -r -E "(api[_-]?key|secret[_-]?key|access[_-]?token|bearer[_-]?token)\s*[=:]\s*['\"][a-zA-Z0-9_-]{20,}['\"]" . \
    --exclude-dir=.git \
    --exclude-dir=node_modules \
    --exclude-dir=.terraform \
    --exclude="*.log" \
    --exclude="*.md" \
    --exclude=".env.example" \
    --exclude="validate-security.sh" \
    2>/dev/null || true)

if [ -n "$FOUND_KEYS" ]; then
    log_error "Potential API keys or tokens found:"
    echo "$FOUND_KEYS"
else
    log_success "No API keys or tokens detected"
fi

# ===== CHECK 3: Search for actual connection strings (not references) =====
log_info "Checking for hardcoded database connection strings..."
FOUND_CONNECTIONS=$(grep -r -E "(jdbc:[a-z]+://|mongodb://[a-zA-Z0-9]|redis://[a-zA-Z0-9]|postgres://[a-zA-Z0-9]|mysql://[a-zA-Z0-9])" . \
    --exclude-dir=.git \
    --exclude-dir=node_modules \
    --exclude-dir=.terraform \
    --exclude="*.log" \
    --exclude="*.md" \
    --exclude=".env.example" \
    --exclude="validate-security.sh" \
    2>/dev/null | grep -v "placeholder\|example\|template\|localhost" || true)

if [ -n "$FOUND_CONNECTIONS" ]; then
    log_error "Potential connection strings found:"
    echo "$FOUND_CONNECTIONS"
else
    log_success "No suspicious connection strings detected"
fi

# ===== CHECK 4: Check for Terraform state files =====
log_info "Checking for Terraform state files..."
FOUND_TFSTATE=$(find . -name "*.tfstate*" -o -name "*.tfplan" -o -name "terraform.tfvars" 2>/dev/null || true)

if [ -n "$FOUND_TFSTATE" ]; then
    log_error "Terraform state/plan files found (should not be committed):"
    echo "$FOUND_TFSTATE"
else
    log_success "No Terraform state files detected"
fi

# ===== CHECK 5: Check for private keys and certificates =====
log_info "Checking for private keys and certificates..."
FOUND_KEYS=$(find . -name "*.key" -o -name "*.pem" -o -name "*.p12" -o -name "*.pfx" -o -name "id_rsa*" -o -name "id_ed25519*" 2>/dev/null | grep -v ".git" || true)

if [ -n "$FOUND_KEYS" ]; then
    log_error "Private keys or certificates found:"
    echo "$FOUND_KEYS"
else
    log_success "No private keys or certificates detected"
fi

# ===== CHECK 6: Check for kubeconfig files =====
log_info "Checking for kubeconfig files..."
FOUND_KUBECONFIG=$(find . -name "kubeconfig" -o -name "*.kubeconfig" -o -name "kube-config*" 2>/dev/null | grep -v ".git" || true)

if [ -n "$FOUND_KUBECONFIG" ]; then
    log_error "Kubernetes config files found:"
    echo "$FOUND_KUBECONFIG"
else
    log_success "No kubeconfig files detected"
fi

# ===== CHECK 7: Check for cloud provider credential files =====
log_info "Checking for cloud provider credentials..."
FOUND_CLOUD_CREDS=$(find . -name "*-key.json" -o -name "service-account*.json" -o -path "*/.aws/*" -o -path "*/.azure/*" -o -path "*/.gcloud/*" 2>/dev/null | grep -v ".git" || true)

if [ -n "$FOUND_CLOUD_CREDS" ]; then
    log_error "Cloud provider credential files found:"
    echo "$FOUND_CLOUD_CREDS"
else
    log_success "No cloud provider credentials detected"
fi

# ===== CHECK 8: Check for .env files with secrets =====
log_info "Checking for .env files with potential secrets..."
FOUND_ENV_FILES=$(find . -name ".env" -o -name ".env.*" ! -name ".env.example" ! -name ".env.template" 2>/dev/null | grep -v ".git" || true)

if [ -n "$FOUND_ENV_FILES" ]; then
    log_warning ".env files found (verify they don't contain real secrets):"
    echo "$FOUND_ENV_FILES"
    # Check if these files contain actual secrets
    for env_file in $FOUND_ENV_FILES; do
        if grep -q -E "(password|secret|key|token).*=.*[a-zA-Z0-9]{8,}" "$env_file" 2>/dev/null; then
            log_error "Potential secrets found in: $env_file"
        fi
    done
else
    log_success "No .env files with secrets detected"
fi

# ===== CHECK 9: Check for large files that might contain secrets =====
log_info "Checking for large files that might contain sensitive data..."
LARGE_FILES=$(find . -size +1M -type f ! -path "./.git/*" ! -path "./node_modules/*" ! -path "./.terraform/*" 2>/dev/null || true)

if [ -n "$LARGE_FILES" ]; then
    log_warning "Large files found (review for sensitive content):"
    echo "$LARGE_FILES"
else
    log_success "No suspicious large files detected"
fi

# ===== CHECK 10: Check .gitignore coverage =====
log_info "Validating .gitignore coverage..."
if [ -f ".gitignore" ]; then
    # Check if critical patterns are in .gitignore
    CRITICAL_PATTERNS=("*.tfstate" "*.tfvars" ".env" "*.key" "*.pem" "kubeconfig")
    MISSING_PATTERNS=()
    
    for pattern in "${CRITICAL_PATTERNS[@]}"; do
        if ! grep -q "^$pattern" .gitignore 2>/dev/null; then
            MISSING_PATTERNS+=("$pattern")
        fi
    done
    
    if [ ${#MISSING_PATTERNS[@]} -gt 0 ]; then
        log_warning "Some critical patterns missing from .gitignore:"
        printf '%s\n' "${MISSING_PATTERNS[@]}"
    else
        log_success ".gitignore has critical security patterns"
    fi
else
    log_error ".gitignore file not found!"
fi

# ===== CHECK 11: Check for secrets in git history =====
log_info "Checking git history for potential secrets (recent commits)..."
if command -v git >/dev/null 2>&1 && [ -d ".git" ]; then
    # Check last 10 commits for potential secrets
    HISTORY_SECRETS=$(git log --oneline -10 | head -5 | while read commit; do
        git show "$commit" 2>/dev/null | grep -E "(password|api[_-]?key|secret|token)" | head -3 || true
    done)
    
    if [ -n "$HISTORY_SECRETS" ]; then
        log_warning "Potential secrets found in recent git history:"
        echo "$HISTORY_SECRETS"
    else
        log_success "No obvious secrets in recent git history"
    fi
else
    log_info "Skipping git history check (not a git repository or git not available)"
fi

# ===== CHECK 12: Validate Terraform variable files =====
log_info "Validating Terraform variable files..."
TFVAR_FILES=$(find infrastructure/terraform -name "*.tfvars" 2>/dev/null || true)

if [ -n "$TFVAR_FILES" ]; then
    log_warning "Terraform variable files found - ensure these don't contain real secrets:"
    echo "$TFVAR_FILES"
    
    # Check each tfvars file for potential secrets
    for tfvars_file in $TFVAR_FILES; do
        if grep -q -E "(password|secret|key|token)\s*=\s*['\"][^'\"]{8,}['\"]" "$tfvars_file" 2>/dev/null; then
            log_error "Potential secrets found in: $tfvars_file"
        fi
    done
fi

# ===== SUMMARY =====
echo ""
echo -e "${BLUE}===============================================${NC}"
echo -e "${BLUE}Security Validation Summary${NC}"
echo -e "${BLUE}===============================================${NC}"

if [ $ERRORS_FOUND -eq 0 ]; then
    log_success "All security checks passed! ✨"
    echo ""
    echo -e "${GREEN}🛡️  Repository is secure for deployment${NC}"
    echo -e "${GREEN}🔐 No hardcoded secrets detected${NC}"
    echo -e "${GREEN}✅ Ready for production deployment${NC}"
    echo ""
    exit 0
else
    echo ""
    log_error "Security validation failed with $ERRORS_FOUND error(s)"
    echo ""
    echo -e "${RED}❌ Security issues must be resolved before deployment${NC}"
    echo -e "${RED}🔒 Please review and fix the identified issues${NC}"
    echo ""
    echo -e "${YELLOW}Next steps:${NC}"
    echo "1. Remove or encrypt any hardcoded secrets"
    echo "2. Add sensitive files to .gitignore"
    echo "3. Use Azure Key Vault for secret management"
    echo "4. Re-run this script to validate fixes"
    echo ""
    exit 1
fi
