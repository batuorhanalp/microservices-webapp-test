# Outputs for Azure module

# === Core Infrastructure ===
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "location" {
  description = "Azure location"
  value       = azurerm_resource_group.main.location
}

# === Networking ===
output "vnet_id" {
  description = "ID of the virtual network"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Name of the virtual network"
  value       = azurerm_virtual_network.main.name
}

output "subnet_ids" {
  description = "IDs of the subnets"
  value = {
    aks      = azurerm_subnet.aks.id
    database = azurerm_subnet.database.id
    redis    = azurerm_subnet.redis.id
  }
}

# === Storage ===
output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.main.name
}

output "storage_account_primary_blob_endpoint" {
  description = "Primary blob endpoint of the storage account"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "storage_account_primary_access_key" {
  description = "Primary access key of the storage account"
  value       = azurerm_storage_account.main.primary_access_key
  sensitive   = true
}

# === Database ===
output "postgres_server_name" {
  description = "Name of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgres_server_fqdn" {
  description = "FQDN of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgres_database_name" {
  description = "Name of the PostgreSQL database"
  value       = azurerm_postgresql_flexible_server_database.main.name
}

output "postgres_connection_string" {
  description = "PostgreSQL connection string"
  value       = azurerm_key_vault_secret.database_connection_string.value
  sensitive   = true
}

# === Redis ===
output "redis_hostname" {
  description = "Redis cache hostname"
  value       = azurerm_redis_cache.main.hostname
}

output "redis_port" {
  description = "Redis cache port"
  value       = azurerm_redis_cache.main.ssl_port
}

output "redis_primary_access_key" {
  description = "Redis primary access key"
  value       = azurerm_redis_cache.main.primary_access_key
  sensitive   = true
}

output "redis_connection_string" {
  description = "Redis connection string"
  value       = "${azurerm_redis_cache.main.hostname}:${azurerm_redis_cache.main.ssl_port},password=${azurerm_redis_cache.main.primary_access_key},ssl=True,abortConnect=False"
  sensitive   = true
}

# === Key Vault ===
output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

# === Event Hub ===
output "event_hub_namespace_name" {
  description = "Name of the Event Hub namespace"
  value       = azurerm_eventhub_namespace.main.name
}

output "event_hub_name" {
  description = "Name of the Event Hub"
  value       = azurerm_eventhub.main.name
}

# === CDN ===
output "cdn_endpoint_url" {
  description = "CDN endpoint URL"
  value       = "https://${azurerm_cdn_endpoint.main.fqdn}"
}

output "cdn_endpoint_fqdn" {
  description = "CDN endpoint FQDN"
  value       = azurerm_cdn_endpoint.main.fqdn
}

# === Container Registry ===
output "container_registry_name" {
  description = "Name of the container registry"
  value       = azurerm_container_registry.main.name
}

output "container_registry_login_server" {
  description = "Login server of the container registry"
  value       = azurerm_container_registry.main.login_server
}

output "container_registry_admin_username" {
  description = "Admin username for the container registry"
  value       = azurerm_container_registry.main.admin_username
}

output "container_registry_admin_password" {
  description = "Admin password for the container registry"
  value       = azurerm_container_registry.main.admin_password
  sensitive   = true
}

# === AKS Cluster ===
output "aks_cluster_name" {
  description = "Name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.name
}

output "aks_cluster_id" {
  description = "ID of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.id
}

output "aks_cluster_fqdn" {
  description = "FQDN of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.fqdn
}

output "aks_cluster_endpoint" {
  description = "Endpoint of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.kube_config[0].host
  sensitive   = true
}

output "aks_cluster_ca_certificate" {
  description = "CA certificate of the AKS cluster"
  value       = base64decode(azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate)
  sensitive   = true
}

output "aks_client_certificate" {
  description = "Client certificate for AKS cluster"
  value       = base64decode(azurerm_kubernetes_cluster.main.kube_config[0].client_certificate)
  sensitive   = true
}

output "aks_client_key" {
  description = "Client key for AKS cluster"
  value       = base64decode(azurerm_kubernetes_cluster.main.kube_config[0].client_key)
  sensitive   = true
}

output "aks_kube_config" {
  description = "AKS cluster kubeconfig"
  value       = azurerm_kubernetes_cluster.main.kube_config_raw
  sensitive   = true
}

output "aks_kubelet_identity_client_id" {
  description = "AKS kubelet managed identity client ID for Key Vault access"
  value       = azurerm_kubernetes_cluster.main.kubelet_identity[0].client_id
}

# === Monitoring ===
output "log_analytics_workspace_id" {
  description = "ID of the Log Analytics workspace"
  value       = azurerm_log_analytics_workspace.main.id
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

# === Key Vault Secrets ===
output "key_vault_secret_names" {
  description = "Names of all secrets stored in Key Vault"
  value = {
    database_password           = azurerm_key_vault_secret.database_password.name
    database_connection_string  = azurerm_key_vault_secret.database_connection_string.name
    redis_connection_string     = azurerm_key_vault_secret.redis_connection_string.name
    redis_password             = azurerm_key_vault_secret.redis_password.name
    storage_account_key        = azurerm_key_vault_secret.storage_account_key.name
    container_registry_password = azurerm_key_vault_secret.container_registry_password.name
    grafana_admin_password     = azurerm_key_vault_secret.grafana_admin_password.name
    jwt_secret                 = azurerm_key_vault_secret.jwt_secret.name
    encryption_key             = azurerm_key_vault_secret.encryption_key.name
    event_hub_connection_string = azurerm_key_vault_secret.event_hub_connection_string.name
    app_insights_key           = azurerm_key_vault_secret.app_insights_key.name
    cdn_endpoint               = azurerm_key_vault_secret.cdn_endpoint.name
    storage_connection_string  = azurerm_key_vault_secret.storage_connection_string.name
  }
}

# Grafana password for Kubernetes deployment
output "grafana_admin_password" {
  description = "Grafana admin password from Key Vault"
  value       = azurerm_key_vault_secret.grafana_admin_password.value
  sensitive   = true
}

# JWT secret for application
output "jwt_secret" {
  description = "JWT secret from Key Vault"
  value       = azurerm_key_vault_secret.jwt_secret.value
  sensitive   = true
}

# Database password for Kubernetes secrets
output "database_password" {
  description = "Database password from Key Vault"
  value       = azurerm_key_vault_secret.database_password.value
  sensitive   = true
}
