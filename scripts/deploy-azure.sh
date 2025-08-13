#!/bin/bash

# Azure Infrastructure Deployment Script
# This script deploys the production web app infrastructure to Azure

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TERRAFORM_DIR="$PROJECT_ROOT/infrastructure/terraform"
ENVIRONMENT="${1:-dev}"

echo -e "${BLUE}🚀 Azure Infrastructure Deployment${NC}"
echo -e "${BLUE}=====================================${NC}"
echo "Environment: $ENVIRONMENT"
echo "Project Root: $PROJECT_ROOT"
echo ""

# Function to print status messages
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI is not installed. Please install it first:"
        echo "  brew install azure-cli"
        echo "  # or visit: https://docs.microsoft.com/cli/azure/install-azure-cli"
        exit 1
    fi
    
    # Check if Terraform is installed
    if ! command -v terraform &> /dev/null; then
        log_error "Terraform is not installed. Please install it first:"
        echo "  brew install terraform"
        echo "  # or visit: https://terraform.io/downloads"
        exit 1
    fi
    
    # Check Azure CLI authentication
    if ! az account show &> /dev/null; then
        log_error "Azure CLI is not authenticated. Please login first:"
        echo "  az login"
        exit 1
    fi
    
    log_success "Prerequisites check passed"
    
    # Display current Azure account
    ACCOUNT_NAME=$(az account show --query name -o tsv)
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    log_info "Using Azure Account: $ACCOUNT_NAME ($SUBSCRIPTION_ID)"
}

# Function to initialize Terraform
init_terraform() {
    log_info "Initializing Terraform..."
    
    cd "$TERRAFORM_DIR"
    
    if terraform init; then
        log_success "Terraform initialized successfully"
    else
        log_error "Failed to initialize Terraform"
        exit 1
    fi
}

# Function to validate Terraform configuration
validate_terraform() {
    log_info "Validating Terraform configuration..."
    
    if terraform validate; then
        log_success "Terraform configuration is valid"
    else
        log_error "Terraform configuration validation failed"
        exit 1
    fi
}

# Function to plan deployment
plan_deployment() {
    log_info "Planning deployment for $ENVIRONMENT environment..."
    
    local tfvars_file="environments/${ENVIRONMENT}.tfvars"
    
    if [ ! -f "$tfvars_file" ]; then
        log_error "Environment file $tfvars_file not found"
        exit 1
    fi
    
    log_info "Using configuration file: $tfvars_file"
    
    if terraform plan -var-file="$tfvars_file" -out="tfplan-${ENVIRONMENT}"; then
        log_success "Terraform plan completed successfully"
        echo ""
        log_info "Review the plan above. Press Enter to continue with deployment or Ctrl+C to cancel..."
        read -r
    else
        log_error "Terraform plan failed"
        exit 1
    fi
}

# Function to apply deployment
apply_deployment() {
    log_info "Applying deployment..."
    
    if terraform apply "tfplan-${ENVIRONMENT}"; then
        log_success "Infrastructure deployed successfully!"
    else
        log_error "Deployment failed"
        exit 1
    fi
}

# Function to get cluster credentials
get_cluster_credentials() {
    log_info "Getting AKS cluster credentials..."
    
    local resource_group=$(terraform output -raw azure_resource_group_name 2>/dev/null)
    local cluster_name=$(terraform output -raw azure_aks_cluster_name 2>/dev/null)
    
    if [ -n "$resource_group" ] && [ -n "$cluster_name" ]; then
        log_info "Resource Group: $resource_group"
        log_info "Cluster Name: $cluster_name"
        
        if az aks get-credentials --resource-group "$resource_group" --name "$cluster_name" --overwrite-existing; then
            log_success "Cluster credentials configured successfully"
            
            # Test cluster connection
            if kubectl cluster-info &> /dev/null; then
                log_success "Successfully connected to AKS cluster"
                kubectl get nodes
            else
                log_warning "Cluster credentials configured but unable to connect. This might be expected if cluster is still starting up."
            fi
        else
            log_error "Failed to get cluster credentials"
        fi
    else
        log_warning "Could not retrieve cluster information from Terraform outputs"
    fi
}

# Function to access monitoring services
access_monitoring() {
    log_info "Setting up monitoring access..."
    
    # Check if K8s resources are deployed
    local k8s_deployed=$(terraform output -json | jq -r '.kubernetes_namespaces.value')
    
    if [ "$k8s_deployed" != "null" ]; then
        log_success "Monitoring stack is deployed!"
        
        echo ""
        log_info "Access Grafana (in a new terminal):"
        echo "kubectl port-forward -n monitoring svc/prometheus-operator-grafana 3000:80"
        echo "Then open: http://localhost:3000"
        echo "Username: admin"
        echo "Password: $(terraform output -json | jq -r '.grafana_credentials.value.password')"
        
        echo ""
        log_info "Access Prometheus (in a new terminal):"
        echo "kubectl port-forward -n monitoring svc/prometheus-operator-prometheus 9090:9090"
        echo "Then open: http://localhost:9090"
        
        echo ""
        log_info "Check monitoring pods:"
        kubectl get pods -n monitoring 2>/dev/null || log_warning "Run kubectl commands after configuring kubeconfig"
    else
        log_info "Kubernetes resources not deployed. To enable monitoring:"
        echo "1. Set deploy_k8s_resources = true in your tfvars file"
        echo "2. Run terraform apply again"
    fi
}

# Function to display deployment summary
show_summary() {
    log_info "Deployment Summary"
    echo "=================="
    
    echo ""
    echo "Infrastructure Deployed:"
    terraform output cloud_provider
    terraform output environment
    terraform output resource_prefix
    
    echo ""
    echo "Key Resources:"
    terraform output azure_aks_cluster_name
    terraform output azure_postgres_server_name
    terraform output azure_storage_account_name
    terraform output azure_container_registry_name
    
    echo ""
    log_info "Kubernetes Namespaces:"
    terraform output kubernetes_namespaces
    
    echo ""
    log_info "Monitoring Information:"
    terraform output monitoring_info
    
    access_monitoring
    
    echo ""
    log_info "Next Steps:"
    echo "1. Verify cluster connection: kubectl get nodes"
    echo "2. Access Grafana: kubectl port-forward -n monitoring svc/prometheus-operator-grafana 3000:80"
    echo "3. Access Prometheus: kubectl port-forward -n monitoring svc/prometheus-operator-prometheus 9090:9090"
    echo "4. Build and deploy your application"
    
    echo ""
    log_info "Quick Start Commands:"
    terraform output quick_start_commands
    
    echo ""
    log_success "Deployment completed successfully! Your production infrastructure with monitoring is ready! 🚀"
}

# Function to cleanup on failure
cleanup() {
    if [ -f "tfplan-${ENVIRONMENT}" ]; then
        log_info "Cleaning up plan file..."
        rm -f "tfplan-${ENVIRONMENT}"
    fi
}

# Set trap for cleanup
trap cleanup EXIT

# Main execution
main() {
    case "${1:-deploy}" in
        "plan")
            check_prerequisites
            init_terraform
            validate_terraform
            plan_deployment
            ;;
        "apply")
            check_prerequisites
            init_terraform
            apply_deployment
            get_cluster_credentials
            show_summary
            ;;
        "deploy")
            check_prerequisites
            init_terraform
            validate_terraform
            plan_deployment
            apply_deployment
            get_cluster_credentials
            show_summary
            ;;
        "destroy")
            log_warning "This will destroy all infrastructure!"
            echo "Type 'yes' to confirm destruction:"
            read -r confirmation
            if [ "$confirmation" = "yes" ]; then
                terraform destroy -var-file="environments/${ENVIRONMENT}.tfvars"
            else
                log_info "Destruction cancelled"
            fi
            ;;
        *)
            echo "Usage: $0 [deploy|plan|apply|destroy] [environment]"
            echo ""
            echo "Commands:"
            echo "  deploy   - Plan and apply infrastructure (default)"
            echo "  plan     - Show what will be created"
            echo "  apply    - Apply previously planned changes"
            echo "  destroy  - Destroy all infrastructure"
            echo ""
            echo "Environment: dev (default), staging, prod"
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"
