# Main Terraform outputs
# Dynamically outputs based on the selected cloud provider

# === Cloud Provider Information ===
output "cloud_provider" {
  description = "Selected cloud provider"
  value       = var.cloud_provider
}

output "region" {
  description = "Deployment region"
  value       = var.region
}

output "environment" {
  description = "Environment name"
  value       = var.environment
}

output "resource_prefix" {
  description = "Resource naming prefix"
  value       = local.resource_prefix
}

# === Azure Outputs ===
output "azure_resource_group_name" {
  description = "Azure resource group name"
  value       = var.cloud_provider == "azure" ? module.azure[0].resource_group_name : null
}

output "azure_aks_cluster_name" {
  description = "Azure AKS cluster name"
  value       = var.cloud_provider == "azure" ? module.azure[0].aks_cluster_name : null
}

output "azure_postgres_server_name" {
  description = "Azure PostgreSQL server name"
  value       = var.cloud_provider == "azure" ? module.azure[0].postgres_server_name : null
}

output "azure_storage_account_name" {
  description = "Azure storage account name"
  value       = var.cloud_provider == "azure" ? module.azure[0].storage_account_name : null
}

output "azure_container_registry_name" {
  description = "Azure Container Registry name"
  value       = var.cloud_provider == "azure" ? module.azure[0].container_registry_name : null
}

output "azure_cdn_endpoint_url" {
  description = "Azure CDN endpoint URL"
  value       = var.cloud_provider == "azure" ? module.azure[0].cdn_endpoint_url : null
}

# === AWS Outputs (placeholder - will be implemented next) ===
output "aws_eks_cluster_name" {
  description = "AWS EKS cluster name"
  value       = var.cloud_provider == "aws" ? module.aws[0].eks_cluster_name : null
}

output "aws_rds_instance_endpoint" {
  description = "AWS RDS instance endpoint"
  value       = var.cloud_provider == "aws" ? module.aws[0].rds_instance_endpoint : null
}

# === GCP Outputs (placeholder - will be implemented next) ===
output "gcp_gke_cluster_name" {
  description = "GCP GKE cluster name"
  value       = var.cloud_provider == "gcp" ? module.gcp[0].gke_cluster_name : null
}

output "gcp_sql_instance_name" {
  description = "GCP Cloud SQL instance name"
  value       = var.cloud_provider == "gcp" ? module.gcp[0].sql_instance_name : null
}

# === Common Connection Information ===
output "database_connection_info" {
  description = "Database connection information"
  value = {
    azure = var.cloud_provider == "azure" ? {
      server   = module.azure[0].postgres_server_fqdn
      database = module.azure[0].postgres_database_name
      username = var.db_admin_username
    } : null
    aws = var.cloud_provider == "aws" ? {
      endpoint = try(module.aws[0].rds_instance_endpoint, null)
      database = try(module.aws[0].rds_database_name, null)
      username = var.db_admin_username
    } : null
    gcp = var.cloud_provider == "gcp" ? {
      connection_name = try(module.gcp[0].sql_connection_name, null)
      database        = try(module.gcp[0].sql_database_name, null)
      username        = var.db_admin_username
    } : null
  }
  sensitive = true
}

output "redis_connection_info" {
  description = "Redis connection information"
  value = {
    azure = var.cloud_provider == "azure" ? {
      hostname = module.azure[0].redis_hostname
      port     = module.azure[0].redis_port
    } : null
    aws = var.cloud_provider == "aws" ? {
      endpoint = try(module.aws[0].redis_endpoint, null)
      port     = try(module.aws[0].redis_port, null)
    } : null
    gcp = var.cloud_provider == "gcp" ? {
      host = try(module.gcp[0].redis_host, null)
      port = try(module.gcp[0].redis_port, null)
    } : null
  }
  sensitive = true
}

output "container_registry_info" {
  description = "Container registry information"
  value = {
    azure = var.cloud_provider == "azure" ? {
      login_server = module.azure[0].container_registry_login_server
      username     = module.azure[0].container_registry_admin_username
    } : null
    aws = var.cloud_provider == "aws" ? {
      repository_url = try(module.aws[0].ecr_repository_url, null)
      registry_id    = try(module.aws[0].ecr_registry_id, null)
    } : null
    gcp = var.cloud_provider == "gcp" ? {
      hostname   = try(module.gcp[0].artifact_registry_hostname, null)
      repository = try(module.gcp[0].artifact_registry_name, null)
    } : null
  }
  sensitive = true
}

output "kubernetes_cluster_info" {
  description = "Kubernetes cluster information"
  value = {
    azure = var.cloud_provider == "azure" ? {
      name     = module.azure[0].aks_cluster_name
      fqdn     = module.azure[0].aks_cluster_fqdn
      endpoint = module.azure[0].aks_cluster_endpoint
    } : null
    aws = var.cloud_provider == "aws" ? {
      name     = try(module.aws[0].eks_cluster_name, null)
      endpoint = try(module.aws[0].eks_cluster_endpoint, null)
    } : null
    gcp = var.cloud_provider == "gcp" ? {
      name     = try(module.gcp[0].gke_cluster_name, null)
      endpoint = try(module.gcp[0].gke_cluster_endpoint, null)
    } : null
  }
  sensitive = true
}

# === Kubernetes and Monitoring Outputs ===
output "kubernetes_namespaces" {
  description = "Kubernetes namespaces created"
  value = var.deploy_k8s_resources ? {
    app        = module.k8s_setup[0].app_namespace
    monitoring = module.k8s_setup[0].monitoring_namespace
    ingress    = module.k8s_setup[0].ingress_namespace
  } : null
}

output "monitoring_info" {
  description = "Monitoring stack information"
  value = var.deploy_k8s_resources ? module.k8s_setup[0].monitoring_stack_info : null
}

output "monitoring_access_commands" {
  description = "Commands to access monitoring services"
  value = var.deploy_k8s_resources ? module.k8s_setup[0].monitoring_urls : null
}

output "grafana_credentials" {
  description = "Grafana login credentials"
  value = var.deploy_k8s_resources ? {
    username = "admin"
    password = var.grafana_admin_password
    url      = "http://localhost:3000 (after port-forward)"
  } : null
  sensitive = true
}

# === Quick Start Commands ===
output "quick_start_commands" {
  description = "Commands to get started with the deployed infrastructure"
  value = var.deploy_k8s_resources ? module.k8s_setup[0].quick_start_commands : {
    get_kubeconfig = var.cloud_provider == "azure" ? "az aks get-credentials --resource-group ${try(module.azure[0].resource_group_name, "")} --name ${try(module.azure[0].aks_cluster_name, "")}" : "Configure kubectl for your cluster"
    check_nodes    = "kubectl get nodes"
    check_pods     = "kubectl get pods -A"
  }
}

# === Deployment Commands ===
output "next_steps" {
  description = "Next steps for deployment"
  value = {
    kubeconfig_command = var.cloud_provider == "azure" ? "az aks get-credentials --resource-group ${try(module.azure[0].resource_group_name, "")} --name ${try(module.azure[0].aks_cluster_name, "")}" : (
      var.cloud_provider == "aws" ? "aws eks update-kubeconfig --region ${var.region} --name ${try(module.aws[0].eks_cluster_name, "")}" :
      var.cloud_provider == "gcp" ? "gcloud container clusters get-credentials ${try(module.gcp[0].gke_cluster_name, "")} --region ${var.region}" : "N/A"
    )
    container_registry_login = var.cloud_provider == "azure" ? "az acr login --name ${try(module.azure[0].container_registry_name, "")}" : (
      var.cloud_provider == "aws" ? "aws ecr get-login-password --region ${var.region} | docker login --username AWS --password-stdin ${try(module.aws[0].ecr_repository_url, "")}" :
      var.cloud_provider == "gcp" ? "gcloud auth configure-docker ${var.region}-docker.pkg.dev" : "N/A"
    )
    access_grafana = var.deploy_k8s_resources ? "kubectl port-forward -n monitoring svc/prometheus-operator-grafana 3000:80" : "Deploy K8s resources first"
    access_prometheus = var.deploy_k8s_resources ? "kubectl port-forward -n monitoring svc/prometheus-operator-prometheus 9090:9090" : "Deploy K8s resources first"
  }
}
