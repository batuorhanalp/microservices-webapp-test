# AWS Development Environment Configuration
# Multi-cloud deployment settings for AWS

# === Core Configuration ===
app_name         = "webapp-prod"
environment      = "dev"
cloud_provider   = "aws"
region          = "us-east-1"
owner           = "devops-team"

# === Network Configuration ===
vpc_cidr = "10.0.0.0/16"

# === Database Configuration ===
# Note: Database credentials should be set via environment variables or AWS Secrets Manager
# Export these before running terraform:
# export TF_VAR_db_admin_username="your-username"
# export TF_VAR_db_admin_password="$(aws secretsmanager get-secret-value --secret-id webapp-prod-dev-db-password --query SecretString --output text)"
db_admin_username    = "dbadmin"  # Default, override with TF_VAR_db_admin_username
# db_admin_password is managed by Secrets Manager - not stored in files
db_allocated_storage = 20
db_instance_class    = "db.t3.micro"

# === Redis Configuration ===
redis_node_type = "cache.t3.micro"

# === Kubernetes Configuration ===
k8s_version        = "1.28"
node_count         = 2
node_instance_type = "t3.medium"

# === Storage Configuration ===
storage_tier = "STANDARD"

# === Security Configuration ===
enable_network_security = true
allowed_ip_ranges      = ["0.0.0.0/0"]  # Restrict in production

# === Monitoring Configuration ===
log_retention_days     = 7
enable_monitoring      = true
# grafana_admin_password is managed by Secrets Manager - set via TF_VAR_grafana_admin_password

# === Security Secrets ===
# jwt_secret is managed by Secrets Manager - set via TF_VAR_jwt_secret
# All secrets should be stored in AWS Secrets Manager and referenced via environment variables

# === Feature Flags ===
deploy_k8s_resources = false
enable_auto_scaling  = true
enable_backup       = false

# === Cost Optimization ===
environment_size = "small"
