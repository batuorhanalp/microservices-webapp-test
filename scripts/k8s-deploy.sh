#!/bin/bash

# Kubernetes Deployment Script for WebApp Production
# Usage: ./scripts/k8s-deploy.sh [dev|staging|production] [action]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
K8S_DIR="$PROJECT_DIR/k8s"

# Default values
ENVIRONMENT="${1:-dev}"
ACTION="${2:-deploy}"
NAMESPACE="webapp-${ENVIRONMENT}"

# Supported environments
ENVIRONMENTS=("dev" "staging" "production")
ACTIONS=("deploy" "destroy" "status" "logs" "rollback" "scale")

print_usage() {
    echo "Usage: $0 [environment] [action]"
    echo ""
    echo "Environments:"
    echo "  dev         - Development environment"
    echo "  staging     - Staging environment"
    echo "  production  - Production environment"
    echo ""
    echo "Actions:"
    echo "  deploy      - Deploy/update the application (default)"
    echo "  destroy     - Remove all resources"
    echo "  status      - Show deployment status"
    echo "  logs        - Show application logs"
    echo "  rollback    - Rollback to previous version"
    echo "  scale       - Scale deployments"
    echo ""
    echo "Examples:"
    echo "  $0 dev deploy"
    echo "  $0 production status"
    echo "  $0 staging logs"
}

print_info() {
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

validate_environment() {
    if [[ ! " ${ENVIRONMENTS[@]} " =~ " ${ENVIRONMENT} " ]]; then
        print_error "Invalid environment: $ENVIRONMENT"
        print_usage
        exit 1
    fi
}

validate_action() {
    if [[ ! " ${ACTIONS[@]} " =~ " ${ACTION} " ]]; then
        print_error "Invalid action: $ACTION"
        print_usage
        exit 1
    fi
}

check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl is not installed or not in PATH"
        exit 1
    fi
    
    # Check kustomize (built into kubectl 1.14+)
    if ! kubectl version --short | grep -q "Client Version"; then
        print_error "kubectl version check failed"
        exit 1
    fi
    
    # Check if we can connect to cluster
    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster"
        print_info "Make sure your kubeconfig is properly configured"
        exit 1
    fi
    
    # Check if kustomization file exists
    if [[ ! -f "$K8S_DIR/overlays/$ENVIRONMENT/kustomization.yaml" ]]; then
        print_error "Kustomization file not found: $K8S_DIR/overlays/$ENVIRONMENT/kustomization.yaml"
        exit 1
    fi
    
    print_success "Prerequisites check passed"
}

confirm_production() {
    if [[ "$ENVIRONMENT" == "production" ]] && [[ "$ACTION" == "deploy" || "$ACTION" == "destroy" ]]; then
        print_warning "You are about to $ACTION to PRODUCTION environment!"
        read -p "Are you sure? (yes/no): " -r
        if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
            print_info "Operation cancelled"
            exit 0
        fi
    fi
}

create_namespace() {
    if ! kubectl get namespace "$NAMESPACE" &> /dev/null; then
        print_info "Creating namespace: $NAMESPACE"
        kubectl create namespace "$NAMESPACE"
        print_success "Namespace created: $NAMESPACE"
    else
        print_info "Namespace already exists: $NAMESPACE"
    fi
}

deploy_application() {
    print_info "Deploying to $ENVIRONMENT environment..."
    
    create_namespace
    
    # Dry run first to validate configuration
    print_info "Validating configuration (dry-run)..."
    if kubectl apply -k "$K8S_DIR/overlays/$ENVIRONMENT" --dry-run=client > /dev/null; then
        print_success "Configuration validation passed"
    else
        print_error "Configuration validation failed"
        exit 1
    fi
    
    # Apply the configuration
    print_info "Applying Kubernetes manifests..."
    kubectl apply -k "$K8S_DIR/overlays/$ENVIRONMENT"
    
    # Wait for deployments to be ready
    print_info "Waiting for deployments to be ready..."
    kubectl wait --for=condition=available --timeout=600s deployment --all -n "$NAMESPACE"
    
    print_success "Deployment completed successfully!"
    show_status
}

destroy_application() {
    print_warning "Destroying all resources in $ENVIRONMENT environment..."
    
    if kubectl get namespace "$NAMESPACE" &> /dev/null; then
        kubectl delete -k "$K8S_DIR/overlays/$ENVIRONMENT" --ignore-not-found=true
        
        # Wait a bit for resources to be deleted
        sleep 10
        
        # Delete namespace if empty
        if [[ "$ENVIRONMENT" != "default" ]]; then
            read -p "Do you want to delete the namespace '$NAMESPACE'? (yes/no): " -r
            if [[ $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
                kubectl delete namespace "$NAMESPACE" --ignore-not-found=true
            fi
        fi
        
        print_success "Resources destroyed successfully"
    else
        print_warning "Namespace $NAMESPACE does not exist"
    fi
}

show_status() {
    print_info "Deployment status for $ENVIRONMENT environment:"
    echo ""
    
    # Check namespace
    if kubectl get namespace "$NAMESPACE" &> /dev/null; then
        print_success "Namespace: $NAMESPACE exists"
    else
        print_error "Namespace: $NAMESPACE does not exist"
        return 1
    fi
    
    echo ""
    print_info "Pods status:"
    kubectl get pods -n "$NAMESPACE" -o wide
    
    echo ""
    print_info "Services status:"
    kubectl get services -n "$NAMESPACE"
    
    echo ""
    print_info "Ingress status:"
    kubectl get ingress -n "$NAMESPACE" 2>/dev/null || print_warning "No ingress resources found"
    
    echo ""
    print_info "Persistent Volume Claims:"
    kubectl get pvc -n "$NAMESPACE" 2>/dev/null || print_info "No PVC resources found"
    
    # Check deployment readiness
    echo ""
    print_info "Deployment readiness:"
    deployments=$(kubectl get deployments -n "$NAMESPACE" -o name 2>/dev/null)
    if [[ -n "$deployments" ]]; then
        for deployment in $deployments; do
            name=$(echo "$deployment" | cut -d'/' -f2)
            ready=$(kubectl get "$deployment" -n "$NAMESPACE" -o jsonpath='{.status.readyReplicas}' 2>/dev/null || echo "0")
            desired=$(kubectl get "$deployment" -n "$NAMESPACE" -o jsonpath='{.spec.replicas}' 2>/dev/null || echo "0")
            
            if [[ "$ready" == "$desired" ]] && [[ "$ready" != "0" ]]; then
                print_success "$name: $ready/$desired ready"
            else
                print_warning "$name: $ready/$desired ready"
            fi
        done
    else
        print_warning "No deployments found"
    fi
}

show_logs() {
    print_info "Application logs for $ENVIRONMENT environment:"
    
    # Get all pods
    pods=$(kubectl get pods -n "$NAMESPACE" -o name 2>/dev/null)
    
    if [[ -z "$pods" ]]; then
        print_warning "No pods found in namespace $NAMESPACE"
        return 1
    fi
    
    echo ""
    echo "Available pods:"
    kubectl get pods -n "$NAMESPACE"
    
    echo ""
    read -p "Enter pod name (or 'all' for all pods): " -r pod_name
    
    if [[ "$pod_name" == "all" ]]; then
        for pod in $pods; do
            name=$(echo "$pod" | cut -d'/' -f2)
            echo ""
            print_info "Logs for $name:"
            kubectl logs "$pod" -n "$NAMESPACE" --tail=50
        done
    else
        kubectl logs "pod/$pod_name" -n "$NAMESPACE" --follow
    fi
}

rollback_deployment() {
    print_info "Rolling back deployments in $ENVIRONMENT environment..."
    
    deployments=$(kubectl get deployments -n "$NAMESPACE" -o name 2>/dev/null)
    
    if [[ -z "$deployments" ]]; then
        print_warning "No deployments found in namespace $NAMESPACE"
        return 1
    fi
    
    echo ""
    echo "Available deployments:"
    kubectl get deployments -n "$NAMESPACE"
    
    echo ""
    read -p "Enter deployment name (or 'all' for all deployments): " -r deployment_name
    
    if [[ "$deployment_name" == "all" ]]; then
        for deployment in $deployments; do
            name=$(echo "$deployment" | cut -d'/' -f2)
            print_info "Rolling back $name..."
            kubectl rollout undo "$deployment" -n "$NAMESPACE"
        done
    else
        print_info "Rolling back deployment/$deployment_name..."
        kubectl rollout undo "deployment/$deployment_name" -n "$NAMESPACE"
    fi
    
    print_success "Rollback initiated. Use 'status' action to check progress."
}

scale_deployments() {
    print_info "Scaling deployments in $ENVIRONMENT environment..."
    
    deployments=$(kubectl get deployments -n "$NAMESPACE" -o name 2>/dev/null)
    
    if [[ -z "$deployments" ]]; then
        print_warning "No deployments found in namespace $NAMESPACE"
        return 1
    fi
    
    echo ""
    echo "Current deployments:"
    kubectl get deployments -n "$NAMESPACE"
    
    echo ""
    read -p "Enter deployment name: " -r deployment_name
    read -p "Enter desired replica count: " -r replica_count
    
    if [[ ! "$replica_count" =~ ^[0-9]+$ ]]; then
        print_error "Invalid replica count: $replica_count"
        return 1
    fi
    
    print_info "Scaling deployment/$deployment_name to $replica_count replicas..."
    kubectl scale "deployment/$deployment_name" --replicas="$replica_count" -n "$NAMESPACE"
    
    print_success "Scaling initiated. Use 'status' action to check progress."
}

main() {
    if [[ "$1" == "-h" || "$1" == "--help" ]]; then
        print_usage
        exit 0
    fi
    
    validate_environment
    validate_action
    check_prerequisites
    confirm_production
    
    case "$ACTION" in
        deploy)
            deploy_application
            ;;
        destroy)
            destroy_application
            ;;
        status)
            show_status
            ;;
        logs)
            show_logs
            ;;
        rollback)
            rollback_deployment
            ;;
        scale)
            scale_deployments
            ;;
        *)
            print_error "Unknown action: $ACTION"
            print_usage
            exit 1
            ;;
    esac
}

# Run main function
main "$@"
