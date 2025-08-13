# GCP Development Environment Configuration
# Multi-cloud deployment settings for GCP

# === Core Configuration ===
app_name         = "webapp-prod"
environment      = "dev"
cloud_provider   = "gcp"
region          = "us-central1"
gcp_project_id  = ""  # Set your GCP project ID here
owner           = "devops-team"

# === Network Configuration ===
vpc_cidr = "10.0.0.0/16"

# === Database Configuration ===
# Note: Database credentials should be set via environment variables or GCP Secret Manager
# Export these before running terraform:
# export TF_VAR_db_admin_username="your-username"
# export TF_VAR_db_admin_password="$(gcloud secrets versions access latest --secret=webapp-prod-dev-db-password)"
db_admin_username = "dbadmin"  # Default, override with TF_VAR_db_admin_username
# db_admin_password is managed by Secret Manager - not stored in files

# === Redis Configuration ===
redis_memory_size = 1

# === Kubernetes Configuration ===
k8s_version       = "1.28"
node_count        = 2
node_machine_type = "e2-standard-2"

# === Storage Configuration ===
storage_tier = "STANDARD"

# === Security Configuration ===
enable_network_security = true
allowed_ip_ranges      = ["0.0.0.0/0"]  # Restrict in production

# === Monitoring Configuration ===
log_retention_days     = 7
enable_monitoring      = true
# grafana_admin_password is managed by Secret Manager - set via TF_VAR_grafana_admin_password

# === Security Secrets ===
# jwt_secret is managed by Secret Manager - set via TF_VAR_jwt_secret
# All secrets should be stored in GCP Secret Manager and referenced via environment variables

# === Feature Flags ===
deploy_k8s_resources = false
enable_auto_scaling  = true
enable_backup       = false

# === Cost Optimization ===
environment_size = "small"
