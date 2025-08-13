# Variables for Kubernetes resources module

variable "app_name" {
  description = "Name of the application"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "namespace" {
  description = "Kubernetes namespace for the application"
  type        = string
}

variable "cloud_provider" {
  description = "Cloud provider (azure, aws, gcp)"
  type        = string
  validation {
    condition     = contains(["azure", "aws", "gcp"], var.cloud_provider)
    error_message = "Cloud provider must be azure, aws, or gcp."
  }
}

# === Cluster Connection ===
variable "cluster_name" {
  description = "Name of the Kubernetes cluster"
  type        = string
}

variable "cluster_endpoint" {
  description = "Kubernetes cluster endpoint"
  type        = string
}

variable "cluster_ca_certificate" {
  description = "Kubernetes cluster CA certificate"
  type        = string
}

variable "cluster_token" {
  description = "Kubernetes cluster token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "kubectl_command" {
  description = "Command to use for kubectl authentication"
  type        = string
  default     = "kubectl"
}

variable "kubectl_args" {
  description = "Arguments for kubectl authentication"
  type        = list(string)
  default     = []
}

# === Storage Configuration ===
variable "storage_class" {
  description = "Storage class for persistent volumes"
  type        = string
  default     = "default"
}

variable "create_storage_class" {
  description = "Whether to create a custom storage class"
  type        = bool
  default     = false
}

# === Monitoring Configuration ===
variable "grafana_admin_password" {
  description = "Grafana admin password"
  type        = string
  default     = "admin123!"
  sensitive   = true
}

variable "enable_monitoring" {
  description = "Enable Prometheus and Grafana monitoring stack"
  type        = bool
  default     = true
}

variable "prometheus_retention" {
  description = "Prometheus data retention period"
  type        = string
  default     = "30d"
}

variable "prometheus_storage_size" {
  description = "Prometheus storage size"
  type        = string
  default     = "20Gi"
}

variable "grafana_storage_size" {
  description = "Grafana storage size"
  type        = string
  default     = "10Gi"
}

# === Application Configuration ===
variable "log_level" {
  description = "Application log level"
  type        = string
  default     = "info"
}

# Connection strings and secrets
variable "database_connection_string" {
  description = "Database connection string"
  type        = string
  sensitive   = true
}

variable "redis_connection_string" {
  description = "Redis connection string"
  type        = string
  sensitive   = true
}

variable "storage_account_url" {
  description = "Storage account URL"
  type        = string
  default     = ""
}

variable "cdn_url" {
  description = "CDN URL"
  type        = string
  default     = ""
}

variable "event_hub_connection_string" {
  description = "Event Hub connection string"
  type        = string
  default     = ""
  sensitive   = true
}

# Secrets
variable "database_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "redis_password" {
  description = "Redis password"
  type        = string
  sensitive   = true
}

variable "storage_account_key" {
  description = "Storage account key"
  type        = string
  default     = ""
  sensitive   = true
}

variable "container_registry_password" {
  description = "Container registry password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT secret for authentication"
  type        = string
  default     = "your-jwt-secret-change-in-production"
  sensitive   = true
}

variable "event_hub_key" {
  description = "Event Hub access key"
  type        = string
  default     = ""
  sensitive   = true
}

# === Network Policies ===
variable "enable_network_policies" {
  description = "Enable Kubernetes network policies"
  type        = bool
  default     = false  # Disabled by default as not all clusters support them
}

# === Ingress Configuration ===
variable "enable_ingress" {
  description = "Enable NGINX Ingress Controller"
  type        = bool
  default     = true
}

variable "ingress_class" {
  description = "Ingress class name"
  type        = string
  default     = "nginx"
}

# === Resource Limits ===
variable "app_resource_limits" {
  description = "Resource limits for application namespace"
  type = object({
    requests_cpu    = string
    requests_memory = string
    limits_cpu      = string
    limits_memory   = string
  })
  default = {
    requests_cpu    = "4"
    requests_memory = "8Gi"
    limits_cpu      = "8"
    limits_memory   = "16Gi"
  }
}

# === Azure Key Vault Integration ===
variable "key_vault_name" {
  description = "Name of the Azure Key Vault"
  type        = string
  default     = ""
}

variable "azure_tenant_id" {
  description = "Azure tenant ID"
  type        = string
  default     = ""
}

variable "aks_kubelet_identity_client_id" {
  description = "AKS kubelet managed identity client ID"
  type        = string
  default     = ""
}

variable "create_example_secret_consumer" {
  description = "Create an example pod that demonstrates Key Vault secret usage"
  type        = bool
  default     = false
}
