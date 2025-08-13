# Main Terraform configuration for production web app
# Supports multi-cloud deployment with provider-specific modules

terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    aws = {
      source  = "hashicorp/aws"
      version = "~>5.0"
    }
    google = {
      source  = "hashicorp/google"
      version = "~>4.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~>2.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~>2.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.0"
    }
  }
}

# Azure client configuration for multi-cloud support
data "azurerm_client_config" "current" {
  count = var.cloud_provider == "azure" ? 1 : 0
}

# Local variables for common configuration
locals {
  app_name    = var.app_name
  environment = var.environment
  region      = var.region
  
  common_tags = {
    Environment = var.environment
    Project     = var.app_name
    ManagedBy   = "Terraform"
    Owner       = var.owner
  }
  
  # Resource naming convention
  resource_prefix = "${local.app_name}-${local.environment}"
  
  # Network configuration
  vpc_cidr = var.vpc_cidr
  subnets = {
    public  = ["10.0.1.0/24", "10.0.2.0/24"]
    private = ["10.0.10.0/24", "10.0.11.0/24"]
    data    = ["10.0.20.0/24", "10.0.21.0/24"]
  }
}

# Cloud provider configurations
module "azure" {
  count  = var.cloud_provider == "azure" ? 1 : 0
  source = "./modules/azure"
  
  app_name         = local.app_name
  environment      = local.environment
  location         = var.region
  resource_prefix  = local.resource_prefix
  tags            = local.common_tags
  vpc_cidr        = local.vpc_cidr
  subnets         = local.subnets
  
  # Database configuration
  db_admin_username = var.db_admin_username
  db_admin_password = var.db_admin_password
  
  # Redis configuration
  redis_capacity = var.redis_capacity
  
  # AKS configuration
  k8s_version    = var.k8s_version
  node_count     = var.node_count
  node_vm_size   = var.node_vm_size
}

module "aws" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  source = "./modules/aws"
  
  app_name         = local.app_name
  environment      = local.environment
  region           = var.region
  resource_prefix  = local.resource_prefix
  tags            = local.common_tags
  vpc_cidr        = local.vpc_cidr
  subnets         = local.subnets
  
  # Database configuration
  db_admin_username = var.db_admin_username
  db_admin_password = var.db_admin_password
  
  # Redis configuration
  redis_node_type = var.redis_node_type
  
  # EKS configuration
  k8s_version   = var.k8s_version
  node_count    = var.node_count
  node_type     = var.node_instance_type
}

module "gcp" {
  count  = var.cloud_provider == "gcp" ? 1 : 0
  source = "./modules/gcp"
  
  app_name        = local.app_name
  environment     = local.environment
  region          = var.region
  project_id      = var.gcp_project_id
  resource_prefix = local.resource_prefix
  labels          = local.common_tags
  vpc_cidr        = local.vpc_cidr
  subnets         = local.subnets
  
  # Database configuration
  db_admin_username = var.db_admin_username
  db_admin_password = var.db_admin_password
  
  # Redis configuration
  redis_memory_size = var.redis_memory_size
  
  # GKE configuration
  k8s_version      = var.k8s_version
  node_count       = var.node_count
  node_machine_type = var.node_machine_type
}

# Kubernetes configuration (applied after cluster creation)
module "k8s_setup" {
  count  = var.deploy_k8s_resources ? 1 : 0
  source = "./modules/k8s"
  
  depends_on = [
    module.azure,
    module.aws,
    module.gcp
  ]
  
  app_name       = local.app_name
  environment    = local.environment
  namespace      = "${local.app_name}-${local.environment}"
  cloud_provider = var.cloud_provider
  
  # Get cluster credentials based on provider
  cluster_name = var.cloud_provider == "azure" ? module.azure[0].aks_cluster_name : (
    var.cloud_provider == "aws" ? module.aws[0].eks_cluster_name : 
    module.gcp[0].gke_cluster_name
  )
  
  cluster_endpoint = var.cloud_provider == "azure" ? module.azure[0].aks_cluster_endpoint : (
    var.cloud_provider == "aws" ? module.aws[0].eks_cluster_endpoint :
    module.gcp[0].gke_cluster_endpoint
  )
  
  cluster_ca_certificate = var.cloud_provider == "azure" ? module.azure[0].aks_cluster_ca_certificate : (
    var.cloud_provider == "aws" ? module.aws[0].eks_cluster_ca_certificate :
    module.gcp[0].gke_cluster_ca_certificate
  )
  
  # Authentication configuration
  kubectl_command = var.cloud_provider == "azure" ? "az" : (
    var.cloud_provider == "aws" ? "aws" : "gcloud"
  )
  
  kubectl_args = var.cloud_provider == "azure" ? [
    "aks", "get-credentials",
    "--resource-group", module.azure[0].resource_group_name,
    "--name", module.azure[0].aks_cluster_name,
    "--format", "exec"
  ] : var.cloud_provider == "aws" ? [
    "eks", "get-token",
    "--cluster-name", module.aws[0].eks_cluster_name,
    "--region", var.region
  ] : [
    "container", "clusters", "get-credentials",
    module.gcp[0].gke_cluster_name,
    "--region", var.region,
    "--format", "json"
  ]
  
  # Storage configuration
  storage_class = var.cloud_provider == "azure" ? "managed-csi" : (
    var.cloud_provider == "aws" ? "gp2" : "standard"
  )
  create_storage_class = true
  
  # Monitoring configuration
  grafana_admin_password = var.cloud_provider == "azure" ? module.azure[0].grafana_admin_password : var.grafana_admin_password
  enable_monitoring      = var.enable_monitoring
  
  # Azure Key Vault integration
  key_vault_name                  = var.cloud_provider == "azure" ? module.azure[0].key_vault_name : ""
  azure_tenant_id                = var.cloud_provider == "azure" ? data.azurerm_client_config.current[0].tenant_id : ""
  aks_kubelet_identity_client_id  = var.cloud_provider == "azure" ? module.azure[0].aks_kubelet_identity_client_id : ""
  
  # Connection strings from cloud resources
  database_connection_string = var.cloud_provider == "azure" ? module.azure[0].postgres_connection_string : (
    var.cloud_provider == "aws" ? module.aws[0].rds_connection_string :
    module.gcp[0].sql_connection_string
  )
  
  redis_connection_string = var.cloud_provider == "azure" ? module.azure[0].redis_connection_string : (
    var.cloud_provider == "aws" ? module.aws[0].redis_connection_string :
    module.gcp[0].redis_connection_string
  )
  
  storage_account_url = var.cloud_provider == "azure" ? module.azure[0].storage_account_primary_blob_endpoint : (
    var.cloud_provider == "aws" ? module.aws[0].s3_bucket_domain_name :
    module.gcp[0].storage_bucket_url
  )
  
  cdn_url = var.cloud_provider == "azure" ? module.azure[0].cdn_endpoint_url : (
    var.cloud_provider == "aws" ? module.aws[0].cloudfront_domain_name :
    module.gcp[0].cdn_url
  )
  
  # Secrets
  database_password = var.db_admin_password
  redis_password = var.cloud_provider == "azure" ? module.azure[0].redis_primary_access_key : (
    var.cloud_provider == "aws" ? module.aws[0].redis_auth_token :
    module.gcp[0].redis_auth_string
  )
  
  storage_account_key = var.cloud_provider == "azure" ? module.azure[0].storage_account_primary_access_key : (
    var.cloud_provider == "aws" ? module.aws[0].s3_access_key :
    module.gcp[0].storage_access_key
  )
  
  container_registry_password = var.cloud_provider == "azure" ? module.azure[0].container_registry_admin_password : (
    var.cloud_provider == "aws" ? module.aws[0].ecr_token :
    module.gcp[0].artifact_registry_key
  )
  
  jwt_secret = var.jwt_secret
}
