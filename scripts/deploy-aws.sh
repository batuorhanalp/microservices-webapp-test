#!/bin/bash

# AWS Multi-Cloud Deployment Script
# Deploy webapp infrastructure to AWS using Terraform

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [deploy|destroy|plan|output] [environment]"
    echo ""
    echo "Commands:"
    echo "  deploy     - Deploy infrastructure to AWS"
    echo "  destroy    - Destroy infrastructure in AWS"
    echo "  plan       - Show deployment plan"
    echo "  output     - Show infrastructure outputs"
    echo ""
    echo "Environments:"
    echo "  dev        - Development environment"
    echo "  staging    - Staging environment"
    echo "  prod       - Production environment"
    echo ""
    echo "Examples:"
    echo "  $0 deploy dev     - Deploy to AWS dev environment"
    echo "  $0 plan staging   - Show plan for AWS staging"
    echo "  $0 destroy dev    - Destroy AWS dev environment"
    echo "  $0 output prod    - Show outputs for AWS prod"
}

# Check arguments
if [ $# -ne 2 ]; then
    print_error "Invalid number of arguments"
    show_usage
    exit 1
fi

COMMAND=$1
ENVIRONMENT=$2
TERRAFORM_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../infrastructure/terraform" && pwd)"
VAR_FILE="${TERRAFORM_DIR}/environments/aws-${ENVIRONMENT}.tfvars"

# Validate command
case $COMMAND in
    deploy|destroy|plan|output)
        ;;
    *)
        print_error "Invalid command: $COMMAND"
        show_usage
        exit 1
        ;;
esac

# Validate environment
case $ENVIRONMENT in
    dev|staging|prod)
        ;;
    *)
        print_error "Invalid environment: $ENVIRONMENT"
        show_usage
        exit 1
        ;;
esac

# Check if Terraform variables file exists
if [ ! -f "$VAR_FILE" ]; then
    print_error "Terraform variables file not found: $VAR_FILE"
    print_error "Please create the environment configuration file"
    exit 1
fi

print_status "Starting AWS deployment for environment: $ENVIRONMENT"

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if Terraform is installed
    if ! command -v terraform &> /dev/null; then
        print_error "Terraform is not installed. Please install Terraform first."
        print_status "Install with: brew install terraform"
        exit 1
    fi
    
    # Check if AWS CLI is installed
    if ! command -v aws &> /dev/null; then
        print_error "AWS CLI is not installed. Please install AWS CLI first."
        print_status "Install with: brew install awscli"
        exit 1
    fi
    
    # Check AWS authentication
    if ! aws sts get-caller-identity &> /dev/null; then
        print_error "AWS authentication failed. Please run 'aws configure' or 'aws sso login'"
        exit 1
    fi
    
    # Check if kubectl is installed (for cluster management)
    if ! command -v kubectl &> /dev/null; then
        print_warning "kubectl is not installed. Install it to manage the Kubernetes cluster."
        print_status "Install with: brew install kubectl"
    fi
    
    print_success "Prerequisites check passed"
}

# Initialize Terraform
init_terraform() {
    print_status "Initializing Terraform..."
    cd "$TERRAFORM_DIR"
    
    # Initialize Terraform
    if terraform init; then
        print_success "Terraform initialized successfully"
    else
        print_error "Terraform initialization failed"
        exit 1
    fi
}

# Validate Terraform configuration
validate_terraform() {
    print_status "Validating Terraform configuration..."
    cd "$TERRAFORM_DIR"
    
    if terraform validate; then
        print_success "Terraform configuration is valid"
    else
        print_error "Terraform configuration validation failed"
        exit 1
    fi
}

# Plan deployment
plan_deployment() {
    print_status "Creating Terraform plan for AWS $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    terraform plan \
        -var-file="$VAR_FILE" \
        -var="cloud_provider=aws" \
        -out="terraform-aws-${ENVIRONMENT}.tfplan"
    
    print_success "Terraform plan created successfully"
}

# Deploy infrastructure
deploy_infrastructure() {
    print_status "Deploying infrastructure to AWS $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    # Show what will be deployed
    print_status "Review the deployment plan:"
    terraform plan -var-file="$VAR_FILE" -var="cloud_provider=aws"
    
    # Ask for confirmation
    echo ""
    read -p "Do you want to proceed with the deployment? (yes/no): " confirm
    
    if [ "$confirm" != "yes" ]; then
        print_warning "Deployment cancelled by user"
        exit 0
    fi
    
    # Apply the configuration
    if terraform apply -var-file="$VAR_FILE" -var="cloud_provider=aws" -auto-approve; then
        print_success "Infrastructure deployed successfully to AWS!"
        
        # Show connection information
        print_status "Getting connection information..."
        terraform output
        
        # Configure kubectl for EKS
        print_status "Configuring kubectl for EKS cluster..."
        
        CLUSTER_NAME=$(terraform output -raw eks_cluster_name 2>/dev/null || echo "")
        REGION=$(terraform output -raw region 2>/dev/null || echo "us-east-1")
        
        if [ -n "$CLUSTER_NAME" ] && command -v kubectl &> /dev/null; then
            if aws eks update-kubeconfig --region "$REGION" --name "$CLUSTER_NAME"; then
                print_success "kubectl configured for EKS cluster: $CLUSTER_NAME"
                
                # Verify cluster access
                print_status "Verifying cluster access..."
                kubectl get nodes
            else
                print_warning "Failed to configure kubectl for EKS cluster"
            fi
        fi
        
    else
        print_error "Infrastructure deployment failed"
        exit 1
    fi
}

# Destroy infrastructure
destroy_infrastructure() {
    print_error "WARNING: This will destroy ALL infrastructure in AWS $ENVIRONMENT environment!"
    print_error "This action cannot be undone!"
    echo ""
    
    # Double confirmation for destroy
    read -p "Are you absolutely sure you want to DESTROY the AWS $ENVIRONMENT environment? (type 'destroy' to confirm): " confirm
    
    if [ "$confirm" != "destroy" ]; then
        print_warning "Destruction cancelled by user"
        exit 0
    fi
    
    print_status "Destroying infrastructure in AWS $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    if terraform destroy -var-file="$VAR_FILE" -var="cloud_provider=aws" -auto-approve; then
        print_success "Infrastructure destroyed successfully"
    else
        print_error "Infrastructure destruction failed"
        exit 1
    fi
}

# Show outputs
show_outputs() {
    print_status "Showing outputs for AWS $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    terraform output
}

# Main execution
print_status "AWS Multi-Cloud Deployment Script"
print_status "================================="

case $COMMAND in
    plan)
        check_prerequisites
        init_terraform
        validate_terraform
        plan_deployment
        ;;
    deploy)
        check_prerequisites
        init_terraform
        validate_terraform
        deploy_infrastructure
        ;;
    destroy)
        check_prerequisites
        init_terraform
        destroy_infrastructure
        ;;
    output)
        show_outputs
        ;;
esac

print_success "Operation completed successfully!"
