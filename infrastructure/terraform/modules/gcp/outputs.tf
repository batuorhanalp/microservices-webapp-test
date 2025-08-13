# Outputs for GCP module

# === VPC and Networking ===
output "vpc_id" {
  description = "ID of the VPC"
  value       = google_compute_network.main.id
}

output "vpc_name" {
  description = "Name of the VPC"
  value       = google_compute_network.main.name
}

output "gke_subnet_id" {
  description = "ID of the GKE subnet"
  value       = google_compute_subnetwork.gke.id
}

output "private_subnet_id" {
  description = "ID of the private subnet"
  value       = google_compute_subnetwork.private.id
}

# === GKE Cluster ===
output "gke_cluster_id" {
  description = "ID of the GKE cluster"
  value       = google_container_cluster.primary.id
}

output "gke_cluster_name" {
  description = "Name of the GKE cluster"
  value       = google_container_cluster.primary.name
}

output "gke_cluster_endpoint" {
  description = "Endpoint for GKE control plane"
  value       = google_container_cluster.primary.endpoint
}

output "gke_cluster_location" {
  description = "Location of the GKE cluster"
  value       = google_container_cluster.primary.location
}

output "gke_cluster_ca_certificate" {
  description = "Base64 encoded certificate data required to communicate with the cluster"
  value       = google_container_cluster.primary.master_auth.0.cluster_ca_certificate
}

output "gke_cluster_master_version" {
  description = "Current master version of the GKE cluster"
  value       = google_container_cluster.primary.master_version
}

output "gke_node_pool_instance_group_urls" {
  description = "List of instance group URLs of the node pool"
  value       = google_container_node_pool.primary_nodes.instance_group_urls
}

# === Cloud SQL Database ===
output "sql_instance_name" {
  description = "Name of the Cloud SQL instance"
  value       = google_sql_database_instance.postgres.name
}

output "sql_instance_connection_name" {
  description = "Connection name of the Cloud SQL instance"
  value       = google_sql_database_instance.postgres.connection_name
}

output "sql_instance_private_ip_address" {
  description = "Private IP address of the Cloud SQL instance"
  value       = google_sql_database_instance.postgres.private_ip_address
  sensitive   = true
}

output "sql_instance_server_ca_cert" {
  description = "Server CA certificate of the Cloud SQL instance"
  value       = google_sql_database_instance.postgres.server_ca_cert
  sensitive   = true
}

output "sql_database_name" {
  description = "Name of the database"
  value       = google_sql_database.database.name
}

output "sql_connection_string" {
  description = "PostgreSQL connection string"
  value       = "postgresql://${var.db_admin_username}:${var.db_admin_password}@${google_sql_database_instance.postgres.private_ip_address}:5432/${google_sql_database.database.name}"
  sensitive   = true
}

# === Memorystore Redis ===
output "redis_instance_id" {
  description = "ID of the Redis instance"
  value       = google_redis_instance.cache.id
}

output "redis_instance_host" {
  description = "Host of the Redis instance"
  value       = google_redis_instance.cache.host
  sensitive   = true
}

output "redis_instance_port" {
  description = "Port of the Redis instance"
  value       = google_redis_instance.cache.port
}

output "redis_instance_current_location_id" {
  description = "Current location ID of the Redis instance"
  value       = google_redis_instance.cache.current_location_id
}

output "redis_auth_string" {
  description = "Redis AUTH string"
  value       = google_redis_instance.cache.auth_string
  sensitive   = true
}

output "redis_connection_string" {
  description = "Redis connection string"
  value       = "redis://:${google_redis_instance.cache.auth_string}@${google_redis_instance.cache.host}:${google_redis_instance.cache.port}"
  sensitive   = true
}

# === Cloud Storage ===
output "storage_bucket_name" {
  description = "Name of the storage bucket"
  value       = google_storage_bucket.storage.name
}

output "storage_bucket_url" {
  description = "URL of the storage bucket"
  value       = google_storage_bucket.storage.url
}

output "storage_bucket_self_link" {
  description = "Self link of the storage bucket"
  value       = google_storage_bucket.storage.self_link
}

output "storage_access_key" {
  description = "Storage access credentials (placeholder - use service account in production)"
  value       = "use-service-account-instead"
  sensitive   = true
}

output "logs_bucket_name" {
  description = "Name of the logs storage bucket"
  value       = google_storage_bucket.logs.name
}

# === Artifact Registry ===
output "artifact_registry_repository_id" {
  description = "ID of the Artifact Registry repository"
  value       = google_artifact_registry_repository.app.repository_id
}

output "artifact_registry_repository_name" {
  description = "Name of the Artifact Registry repository"
  value       = google_artifact_registry_repository.app.name
}

output "artifact_registry_repository_url" {
  description = "URL of the Artifact Registry repository"
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.app.repository_id}"
}

output "artifact_registry_key" {
  description = "Artifact Registry authentication key (placeholder - use gcloud auth configure-docker)"
  value       = "use-gcloud-auth-configure-docker"
  sensitive   = true
}

# === Cloud CDN ===
output "cdn_backend_bucket_name" {
  description = "Name of the CDN backend bucket"
  value       = google_compute_backend_bucket.static_backend.name
}

output "cdn_url_map_id" {
  description = "ID of the URL map"
  value       = google_compute_url_map.cdn.id
}

output "cdn_https_proxy_id" {
  description = "ID of the HTTPS proxy"
  value       = google_compute_target_https_proxy.cdn.id
}

output "cdn_ssl_certificate_id" {
  description = "ID of the SSL certificate"
  value       = google_compute_managed_ssl_certificate.cdn.id
}

output "cdn_forwarding_rule_ip" {
  description = "IP address of the forwarding rule"
  value       = google_compute_global_forwarding_rule.cdn.ip_address
}

output "cdn_url" {
  description = "CDN URL"
  value       = "https://${google_compute_global_forwarding_rule.cdn.ip_address}"
}

# === Pub/Sub ===
output "pubsub_topic_id" {
  description = "ID of the Pub/Sub topic"
  value       = google_pubsub_topic.app_events.id
}

output "pubsub_topic_name" {
  description = "Name of the Pub/Sub topic"
  value       = google_pubsub_topic.app_events.name
}

output "pubsub_subscription_id" {
  description = "ID of the Pub/Sub subscription"
  value       = google_pubsub_subscription.app_events.id
}

output "pubsub_subscription_name" {
  description = "Name of the Pub/Sub subscription"
  value       = google_pubsub_subscription.app_events.name
}

# === Secret Manager ===
output "secret_manager_secret_id" {
  description = "ID of the Secret Manager secret"
  value       = google_secret_manager_secret.app_secrets.secret_id
}

output "secret_manager_secret_name" {
  description = "Name of the Secret Manager secret"
  value       = google_secret_manager_secret.app_secrets.name
}

# === KMS ===
output "kms_key_ring_id" {
  description = "ID of the KMS key ring"
  value       = google_kms_key_ring.key_ring.id
}

output "kms_crypto_key_id" {
  description = "ID of the KMS crypto key"
  value       = google_kms_crypto_key.storage.id
}

# === Service Accounts ===
output "gke_service_account_email" {
  description = "Email of the GKE service account"
  value       = google_service_account.gke_nodes.email
}

# === Connection Information Summary ===
output "connection_info" {
  description = "Summary of connection information"
  value = {
    cluster_name      = google_container_cluster.primary.name
    cluster_endpoint  = google_container_cluster.primary.endpoint
    database_host     = google_sql_database_instance.postgres.private_ip_address
    redis_host        = google_redis_instance.cache.host
    storage_bucket    = google_storage_bucket.storage.name
    cdn_ip           = google_compute_global_forwarding_rule.cdn.ip_address
    registry_url     = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.app.repository_id}"
  }
}

# === Deployment Information ===
output "deployment_info" {
  description = "Information needed for application deployment"
  value = {
    # Kubernetes
    cluster_name     = google_container_cluster.primary.name
    cluster_ca       = google_container_cluster.primary.master_auth.0.cluster_ca_certificate
    cluster_endpoint = google_container_cluster.primary.endpoint
    cluster_location = google_container_cluster.primary.location
    
    # Container Registry
    registry_url = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.app.repository_id}"
    
    # Storage
    storage_bucket = google_storage_bucket.storage.name
    cdn_ip        = google_compute_global_forwarding_rule.cdn.ip_address
    
    # Secrets
    secrets_name = google_secret_manager_secret.app_secrets.name
    
    # Pub/Sub
    topic_name = google_pubsub_topic.app_events.name
    
    # Logs
    logs_bucket = google_storage_bucket.logs.name
    
    # Project Info
    project_id = var.project_id
    region     = var.region
  }
  sensitive = true
}
