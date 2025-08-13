#!/bin/bash

# GCP Multi-Cloud Deployment Script
# Deploy webapp infrastructure to Google Cloud Platform using Terraform

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
    echo "  deploy     - Deploy infrastructure to GCP"
    echo "  destroy    - Destroy infrastructure in GCP"
    echo "  plan       - Show deployment plan"
    echo "  output     - Show infrastructure outputs"
    echo ""
    echo "Environments:"
    echo "  dev        - Development environment"
    echo "  staging    - Staging environment"
    echo "  prod       - Production environment"
    echo ""
    echo "Examples:"
    echo "  $0 deploy dev     - Deploy to GCP dev environment"
    echo "  $0 plan staging   - Show plan for GCP staging"
    echo "  $0 destroy dev    - Destroy GCP dev environment"
    echo "  $0 output prod    - Show outputs for GCP prod"
    echo ""
    echo "Prerequisites:"
    echo "  - Set GCP_PROJECT_ID environment variable or edit gcp-dev.tfvars"
    echo "  - Run 'gcloud auth login' and 'gcloud auth application-default login'"
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
VAR_FILE="${TERRAFORM_DIR}/environments/gcp-${ENVIRONMENT}.tfvars"

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

print_status "Starting GCP deployment for environment: $ENVIRONMENT"

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if Terraform is installed
    if ! command -v terraform &> /dev/null; then
        print_error "Terraform is not installed. Please install Terraform first."
        print_status "Install with: brew install terraform"
        exit 1
    fi
    
    # Check if gcloud CLI is installed
    if ! command -v gcloud &> /dev/null; then
        print_error "Google Cloud CLI is not installed. Please install gcloud first."
        print_status "Install with: brew install google-cloud-sdk"
        exit 1
    fi
    
    # Check GCP authentication
    if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" | head -n1 > /dev/null; then
        print_error "GCP authentication failed. Please run 'gcloud auth login'"
        exit 1
    fi
    
    # Check application default credentials
    if ! gcloud auth application-default print-access-token &> /dev/null; then
        print_warning "Application Default Credentials not set. Setting them up..."
        if ! gcloud auth application-default login; then
            print_error "Failed to set up Application Default Credentials"
            exit 1
        fi
    fi
    
    # Check if kubectl is installed (for cluster management)
    if ! command -v kubectl &> /dev/null; then
        print_warning "kubectl is not installed. Install it to manage the Kubernetes cluster."
        print_status "Install with: brew install kubectl"
    fi
    
    # Check if gke-gcloud-auth-plugin is installed
    if ! gcloud components list --filter="id:gke-gcloud-auth-plugin" --format="value(state.name)" | grep -q "Installed"; then
        print_warning "gke-gcloud-auth-plugin is not installed. Installing..."
        gcloud components install gke-gcloud-auth-plugin
    fi
    
    print_success "Prerequisites check passed"
}

# Validate GCP project configuration
validate_gcp_config() {
    print_status "Validating GCP configuration..."
    
    # Check if GCP project ID is set
    PROJECT_ID=$(grep 'gcp_project_id' "$VAR_FILE" | cut -d'"' -f4)
    
    if [ -z "$PROJECT_ID" ] || [ "$PROJECT_ID" = "" ]; then
        print_error "GCP project ID is not set in $VAR_FILE"
        print_error "Please edit the file and set your GCP project ID"
        exit 1
    fi
    
    # Set the project for gcloud
    gcloud config set project "$PROJECT_ID"
    
    # Verify project access
    if ! gcloud projects describe "$PROJECT_ID" &> /dev/null; then
        print_error "Cannot access GCP project: $PROJECT_ID"
        print_error "Please check the project ID and your permissions"
        exit 1
    fi
    
    print_success "GCP project configuration is valid: $PROJECT_ID"
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
    print_status "Creating Terraform plan for GCP $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    terraform plan \
        -var-file="$VAR_FILE" \
        -var="cloud_provider=gcp" \
        -out="terraform-gcp-${ENVIRONMENT}.tfplan"
    
    print_success "Terraform plan created successfully"
}

# Deploy infrastructure
deploy_infrastructure() {
    print_status "Deploying infrastructure to GCP $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    # Show what will be deployed
    print_status "Review the deployment plan:"
    terraform plan -var-file="$VAR_FILE" -var="cloud_provider=gcp"
    
    # Ask for confirmation
    echo ""
    read -p "Do you want to proceed with the deployment? (yes/no): " confirm
    
    if [ "$confirm" != "yes" ]; then
        print_warning "Deployment cancelled by user"
        exit 0
    fi
    
    # Apply the configuration
    if terraform apply -var-file="$VAR_FILE" -var="cloud_provider=gcp" -auto-approve; then
        print_success "Infrastructure deployed successfully to GCP!"
        
        # Show connection information
        print_status "Getting connection information..."
        terraform output
        
        # Configure kubectl for GKE
        print_status "Configuring kubectl for GKE cluster..."
        
        CLUSTER_NAME=$(terraform output -raw gke_cluster_name 2>/dev/null || echo "")
        REGION=$(terraform output -raw region 2>/dev/null || echo "us-central1")
        PROJECT_ID=$(terraform output -raw project_id 2>/dev/null || grep 'gcp_project_id' "$VAR_FILE" | cut -d'"' -f4)
        
        if [ -n "$CLUSTER_NAME" ] && command -v kubectl &> /dev/null; then
            if gcloud container clusters get-credentials "$CLUSTER_NAME" --region="$REGION" --project="$PROJECT_ID"; then
                print_success "kubectl configured for GKE cluster: $CLUSTER_NAME"
                
                # Verify cluster access
                print_status "Verifying cluster access..."
                kubectl get nodes
            else
                print_warning "Failed to configure kubectl for GKE cluster"
            fi
        fi
        
    else
        print_error "Infrastructure deployment failed"
        exit 1
    fi
}

# Destroy infrastructure
destroy_infrastructure() {
    print_error "WARNING: This will destroy ALL infrastructure in GCP $ENVIRONMENT environment!"
    print_error "This action cannot be undone!"
    echo ""
    
    # Double confirmation for destroy
    read -p "Are you absolutely sure you want to DESTROY the GCP $ENVIRONMENT environment? (type 'destroy' to confirm): " confirm
    
    if [ "$confirm" != "destroy" ]; then
        print_warning "Destruction cancelled by user"
        exit 0
    fi
    
    print_status "Destroying infrastructure in GCP $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    if terraform destroy -var-file="$VAR_FILE" -var="cloud_provider=gcp" -auto-approve; then
        print_success "Infrastructure destroyed successfully"
    else
        print_error "Infrastructure destruction failed"
        exit 1
    fi
}

# Show outputs
show_outputs() {
    print_status "Showing outputs for GCP $ENVIRONMENT environment..."
    cd "$TERRAFORM_DIR"
    
    terraform output
}

# Main execution
print_status "GCP Multi-Cloud Deployment Script"
print_status "================================="

case $COMMAND in
    plan)
        check_prerequisites
        validate_gcp_config
        init_terraform
        validate_terraform
        plan_deployment
        ;;
    deploy)
        check_prerequisites
        validate_gcp_config
        init_terraform
        validate_terraform
        deploy_infrastructure
        ;;
    destroy)
        check_prerequisites
        validate_gcp_config
        init_terraform
        destroy_infrastructure
        ;;
    output)
        show_outputs
        ;;
esac

print_success "Operation completed successfully!"
