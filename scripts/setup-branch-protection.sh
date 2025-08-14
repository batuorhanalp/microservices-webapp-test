#!/bin/bash

# GitHub Branch Protection Setup Script
# This script configures strict branch protection for the main branch

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🛡️  GitHub Branch Protection Setup${NC}"
echo -e "${BLUE}====================================${NC}\n"

# Check if GitHub CLI is installed
if ! command -v gh &> /dev/null; then
    echo -e "${RED}❌ GitHub CLI (gh) is not installed${NC}"
    echo -e "${YELLOW}Please install it from: https://cli.github.com/${NC}"
    echo -e "${YELLOW}Or use the manual method in the README${NC}"
    exit 1
fi

# Check if user is authenticated
if ! gh auth status &> /dev/null; then
    echo -e "${RED}❌ Not authenticated with GitHub CLI${NC}"
    echo -e "${YELLOW}Please run: gh auth login${NC}"
    exit 1
fi

# Get repository information
REPO_OWNER="batuorhanalp"
REPO_NAME="microservices-webapp-test"
BRANCH="main"

echo -e "${BLUE}Repository: ${REPO_OWNER}/${REPO_NAME}${NC}"
echo -e "${BLUE}Branch: ${BRANCH}${NC}\n"

echo -e "${YELLOW}⚠️  WARNING: This will set strict protection rules on the main branch!${NC}"
echo -e "${YELLOW}   - No direct pushes allowed${NC}"
echo -e "${YELLOW}   - All changes must go through PRs${NC}"
echo -e "${YELLOW}   - CI/CD checks must pass${NC}"
echo -e "${YELLOW}   - Rules apply to administrators too${NC}\n"

read -p "Do you want to continue? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${RED}❌ Cancelled by user${NC}"
    exit 1
fi

echo -e "\n${BLUE}🔧 Configuring branch protection rules...${NC}\n"

# Create branch protection rule
echo -e "${BLUE}Creating branch protection rule for '${BRANCH}'...${NC}"

# Note: GitHub CLI doesn't have full support for all branch protection settings
# We'll use the GitHub API directly
PROTECTION_JSON=$(cat <<EOF
{
  "required_status_checks": {
    "strict": true,
    "contexts": [
      "CI - Build, Test, and Coverage / test",
      "Quality Gate / quality-gate"
    ]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false,
    "require_last_push_approval": true
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_linear_history": true,
  "required_conversation_resolution": true
}
EOF
)

# Apply the protection rule using GitHub API
echo -e "${BLUE}Applying protection rules via GitHub API...${NC}"

if gh api \
  --method PUT \
  -H "Accept: application/vnd.github+json" \
  "/repos/${REPO_OWNER}/${REPO_NAME}/branches/${BRANCH}/protection" \
  --input <(echo "$PROTECTION_JSON") > /dev/null 2>&1; then
    
    echo -e "${GREEN}✅ Branch protection rules applied successfully!${NC}\n"
    
    echo -e "${GREEN}🎉 Main branch is now protected with:${NC}"
    echo -e "   ${GREEN}✅ Require pull request reviews (1 approval minimum)${NC}"
    echo -e "   ${GREEN}✅ Dismiss stale reviews on new commits${NC}"
    echo -e "   ${GREEN}✅ Require status checks to pass${NC}"
    echo -e "   ${GREEN}✅ Require up-to-date branches${NC}"
    echo -e "   ${GREEN}✅ Required status checks:${NC}"
    echo -e "      - CI - Build, Test, and Coverage / test"
    echo -e "      - Quality Gate / quality-gate"
    echo -e "   ${GREEN}✅ Enforce linear history${NC}"
    echo -e "   ${GREEN}✅ Require conversation resolution${NC}"
    echo -e "   ${GREEN}✅ Include administrators${NC}"
    echo -e "   ${GREEN}✅ Block force pushes${NC}"
    echo -e "   ${GREEN}✅ Block branch deletions${NC}"
    
else
    echo -e "${RED}❌ Failed to apply branch protection rules${NC}"
    echo -e "${YELLOW}Please use the manual method described in the documentation${NC}"
    exit 1
fi

echo -e "\n${BLUE}🔍 Verifying protection rules...${NC}"

# Verify the protection rules
if gh api "/repos/${REPO_OWNER}/${REPO_NAME}/branches/${BRANCH}/protection" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Branch protection verified successfully${NC}"
else
    echo -e "${YELLOW}⚠️  Could not verify protection rules (but they should be applied)${NC}"
fi

echo -e "\n${GREEN}🚀 Setup complete!${NC}"
echo -e "\n${BLUE}Next steps:${NC}"
echo -e "1. ${YELLOW}Create a feature branch for new work:${NC}"
echo -e "   git checkout -b feature/your-feature-name"
echo -e "\n2. ${YELLOW}Make your changes and push to the feature branch:${NC}"
echo -e "   git add . && git commit -m 'Your changes'"
echo -e "   git push origin feature/your-feature-name"
echo -e "\n3. ${YELLOW}Create a Pull Request on GitHub${NC}"
echo -e "   - CI/CD will run automatically"
echo -e "   - Coverage must be ≥ 97%"
echo -e "   - All tests must pass"
echo -e "   - Requires 1 approval"
echo -e "\n4. ${YELLOW}Merge only after all checks pass${NC}"

echo -e "\n${GREEN}🛡️  Your main branch is now secure!${NC}"
