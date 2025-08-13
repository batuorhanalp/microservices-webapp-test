# Azure Key Vault Secrets Management
# This file handles all secrets securely using Azure Key Vault

# Random password generation for production security
resource "random_password" "database_password" {
  length  = 16
  special = true
  upper   = true
  lower   = true
  numeric = true
  
  # Ensure compatibility with PostgreSQL password requirements
  override_special = "!@#$%&*"
}

resource "random_password" "grafana_admin_password" {
  length  = 16
  special = true
  upper   = true
  lower   = true
  numeric = true
  
  override_special = "!@#$%&*"
}

resource "random_password" "jwt_secret" {
  length  = 32
  special = false  # JWT secrets should be alphanumeric
  upper   = true
  lower   = true
  numeric = true
}

resource "random_password" "redis_auth_key" {
  length  = 32
  special = true
  upper   = true
  lower   = true
  numeric = true
  
  override_special = "!@#$%&*"
}

resource "random_password" "encryption_key" {
  length  = 32
  special = false
  upper   = true
  lower   = true
  numeric = true
}

# Store secrets in Key Vault
resource "azurerm_key_vault_secret" "database_password" {
  name         = "database-admin-password"
  value        = random_password.database_password.result
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [azurerm_role_assignment.current_user_kv]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "database_connection_string" {
  name  = "database-connection-string"
  value = "postgresql://${var.db_admin_username}:${random_password.database_password.result}@${azurerm_postgresql_flexible_server.main.fqdn}:5432/${azurerm_postgresql_flexible_server_database.main.name}?sslmode=require"
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_postgresql_flexible_server.main,
    azurerm_postgresql_flexible_server_database.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "redis_connection_string" {
  name         = "redis-connection-string"
  value        = "${azurerm_redis_cache.main.hostname}:${azurerm_redis_cache.main.ssl_port},password=${azurerm_redis_cache.main.primary_access_key},ssl=True,abortConnect=False"
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_redis_cache.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "redis_password" {
  name         = "redis-password"
  value        = azurerm_redis_cache.main.primary_access_key
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_redis_cache.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "storage_account_key" {
  name         = "storage-account-key"
  value        = azurerm_storage_account.main.primary_access_key
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_storage_account.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "container_registry_password" {
  name         = "container-registry-password"
  value        = azurerm_container_registry.main.admin_password
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_container_registry.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "grafana_admin_password" {
  name         = "grafana-admin-password"
  value        = random_password.grafana_admin_password.result
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [azurerm_role_assignment.current_user_kv]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "jwt_secret" {
  name         = "jwt-secret"
  value        = random_password.jwt_secret.result
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [azurerm_role_assignment.current_user_kv]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "encryption_key" {
  name         = "app-encryption-key"
  value        = random_password.encryption_key.result
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [azurerm_role_assignment.current_user_kv]
  
  tags = var.tags
}

# Event Hub connection string (if created)
resource "azurerm_key_vault_secret" "event_hub_connection_string" {
  name         = "event-hub-connection-string"
  value        = azurerm_eventhub_namespace.main.default_primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_eventhub_namespace.main
  ]
  
  tags = var.tags
}

# Application Insights instrumentation key
resource "azurerm_key_vault_secret" "app_insights_key" {
  name         = "app-insights-instrumentation-key"
  value        = azurerm_application_insights.main.instrumentation_key
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_application_insights.main
  ]
  
  tags = var.tags
}

resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "app-insights-connection-string"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_application_insights.main
  ]
  
  tags = var.tags
}

# CDN endpoint for application configuration
resource "azurerm_key_vault_secret" "cdn_endpoint" {
  name         = "cdn-endpoint-url"
  value        = "https://${azurerm_cdn_endpoint.main.fqdn}"
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_cdn_endpoint.main
  ]
  
  tags = var.tags
}

# Storage account connection strings
resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "storage-connection-string"
  value        = azurerm_storage_account.main.primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
  
  depends_on = [
    azurerm_role_assignment.current_user_kv,
    azurerm_storage_account.main
  ]
  
  tags = var.tags
}

# RBAC assignments for Key Vault access
data "azurerm_client_config" "current" {}

# Current user/service principal access to Key Vault
resource "azurerm_role_assignment" "current_user_kv" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}

# AKS cluster managed identity access to Key Vault
resource "azurerm_role_assignment" "aks_kv_access" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_kubernetes_cluster.main.identity[0].principal_id
  
  depends_on = [azurerm_kubernetes_cluster.main]
}

# Key Vault access policy for AKS nodes (alternative method)
resource "azurerm_role_assignment" "aks_nodes_kv_access" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  
  depends_on = [azurerm_kubernetes_cluster.main]
}

# Create a Key Vault access policy for automated systems
resource "azurerm_key_vault_access_policy" "terraform" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore",
    "Purge"
  ]

  depends_on = [azurerm_key_vault.main]
}
