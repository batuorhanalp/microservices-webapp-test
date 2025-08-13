# Development Environment Configuration

# === Core Configuration ===
app_name     = "webapp-prod"
environment  = "dev"
owner        = "development-team"

# === Cloud Provider ===
cloud_provider = "azure"
region         = "East US"

# === Networking ===
vpc_cidr = "10.0.0.0/16"

# === Database Configuration ===
db_admin_username   = "dbadmin"
# Note: Database password is auto-generated and stored in Key Vault
db_allocated_storage = 20

# === Redis Configuration ===
redis_capacity    = 0  # C0 - 250MB (Azure)
redis_node_type   = "cache.t3.micro"  # AWS
redis_memory_size = 1  # GCP

# === Kubernetes Configuration ===
k8s_version        = "1.28"
node_count         = 2
node_vm_size       = "Standard_B2s"  # Azure
node_instance_type = "t3.medium"     # AWS
node_machine_type  = "e2-standard-2" # GCP

# === Storage ===
storage_tier = "Standard_LRS"

# === CDN ===
cdn_sku = "Standard_Microsoft"

# === Event Hub ===
event_hub_sku      = "Standard"
event_hub_capacity = 1

# === Security ===
enable_network_security = true
allowed_ip_ranges       = ["0.0.0.0/0"]  # Should be restricted in production

# === Monitoring ===
log_retention_days = 30
enable_monitoring  = true

# === Secrets Management ===
# Note: All passwords and secrets are auto-generated and stored in Azure Key Vault
# - Database password: auto-generated 16-character secure password
# - Grafana admin password: auto-generated 16-character secure password  
# - JWT secret: auto-generated 32-character alphanumeric string
# - Redis password: managed by Azure Cache for Redis
# - Storage keys: managed by Azure Storage Account
# - Container registry credentials: managed by Azure Container Registry

# === Feature Flags ===
deploy_k8s_resources = true   # Deploy K8s resources including Prometheus and Grafana
enable_auto_scaling  = true
enable_backup        = true

# === Cost Optimization ===
environment_size = "small"
