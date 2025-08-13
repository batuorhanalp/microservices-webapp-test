# GCP module for production web app infrastructure
# Creates GKE cluster, Cloud SQL PostgreSQL, Memorystore Redis, Cloud Storage, Cloud CDN, and Pub/Sub

terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 4.0"
    }
    google-beta = {
      source  = "hashicorp/google-beta"
      version = "~> 4.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

# Configure the Google Cloud Provider
provider "google" {
  project = var.project_id
  region  = var.region
}

provider "google-beta" {
  project = var.project_id
  region  = var.region
}

# Generate secure random passwords when not provided via environment variables
resource "random_password" "db_password" {
  count   = var.db_admin_password == "" ? 1 : 0
  length  = 16
  special = true
  upper   = true
  lower   = true
  numeric = true
}

resource "random_password" "grafana_password" {
  count   = var.grafana_admin_password == "" ? 1 : 0
  length  = 16
  special = false  # Grafana has issues with some special characters
  upper   = true
  lower   = true
  numeric = true
}

# Local variables for password handling
locals {
  db_password       = var.db_admin_password != "" ? var.db_admin_password : random_password.db_password[0].result
  grafana_password  = var.grafana_admin_password != "" ? var.grafana_admin_password : random_password.grafana_password[0].result
  jwt_secret       = var.jwt_secret != "" ? var.jwt_secret : random_password.jwt_secret.result
}

resource "random_id" "suffix" {
  byte_length = 4
}

# === Enable Required APIs ===
resource "google_project_service" "required_apis" {
  for_each = toset([
    "compute.googleapis.com",
    "container.googleapis.com",
    "sql-component.googleapis.com",
    "sqladmin.googleapis.com",
    "redis.googleapis.com",
    "storage.googleapis.com",
    "pubsub.googleapis.com",
    "secretmanager.googleapis.com",
    "artifactregistry.googleapis.com",
    "logging.googleapis.com",
    "monitoring.googleapis.com",
    "cloudresourcemanager.googleapis.com"
  ])

  project = var.project_id
  service = each.key

  disable_dependent_services = true
}

# === VPC and Networking ===
resource "google_compute_network" "main" {
  name                    = "${var.resource_prefix}-vpc"
  auto_create_subnetworks = false
  mtu                     = 1460

  depends_on = [google_project_service.required_apis]
}

# Subnet for GKE cluster
resource "google_compute_subnetwork" "gke" {
  name          = "${var.resource_prefix}-gke-subnet"
  ip_cidr_range = var.subnets.private[0]
  region        = var.region
  network       = google_compute_network.main.id

  secondary_ip_range {
    range_name    = "gke-pods"
    ip_cidr_range = "192.168.0.0/16"
  }

  secondary_ip_range {
    range_name    = "gke-services"
    ip_cidr_range = "172.16.0.0/16"
  }

  private_ip_google_access = true
}

# Subnet for private services (Cloud SQL, etc.)
resource "google_compute_subnetwork" "private" {
  name          = "${var.resource_prefix}-private-subnet"
  ip_cidr_range = var.subnets.data[0]
  region        = var.region
  network       = google_compute_network.main.id

  private_ip_google_access = true
}

# Cloud Router for NAT Gateway
resource "google_compute_router" "router" {
  name    = "${var.resource_prefix}-router"
  region  = var.region
  network = google_compute_network.main.id

  bgp {
    asn = 64514
  }
}

# Cloud NAT for outbound internet access
resource "google_compute_router_nat" "nat" {
  name                               = "${var.resource_prefix}-nat"
  router                            = google_compute_router.router.name
  region                            = var.region
  nat_ip_allocate_option            = "AUTO_ONLY"
  source_subnetwork_ip_ranges_to_nat = "ALL_SUBNETWORKS_ALL_IP_RANGES"

  log_config {
    enable = true
    filter = "ERRORS_ONLY"
  }
}

# Firewall rules
resource "google_compute_firewall" "allow_internal" {
  name    = "${var.resource_prefix}-allow-internal"
  network = google_compute_network.main.name

  allow {
    protocol = "tcp"
    ports    = ["0-65535"]
  }

  allow {
    protocol = "udp"
    ports    = ["0-65535"]
  }

  allow {
    protocol = "icmp"
  }

  source_ranges = [var.vpc_cidr]
  target_tags   = ["internal"]
}

resource "google_compute_firewall" "allow_ssh" {
  name    = "${var.resource_prefix}-allow-ssh"
  network = google_compute_network.main.name

  allow {
    protocol = "tcp"
    ports    = ["22"]
  }

  source_ranges = var.allowed_ip_ranges
  target_tags   = ["ssh"]
}

resource "google_compute_firewall" "allow_http_https" {
  name    = "${var.resource_prefix}-allow-http-https"
  network = google_compute_network.main.name

  allow {
    protocol = "tcp"
    ports    = ["80", "443"]
  }

  source_ranges = ["0.0.0.0/0"]
  target_tags   = ["http-server", "https-server"]
}

# === GKE Cluster ===
resource "google_container_cluster" "primary" {
  name     = "${var.resource_prefix}-gke"
  location = var.region
  
  # Use regional cluster for high availability
  node_locations = []

  # We can't create a cluster with no node pool defined, but we want to only use
  # separately managed node pools. So we create the smallest possible default
  # node pool and immediately delete it.
  remove_default_node_pool = true
  initial_node_count       = 1

  network    = google_compute_network.main.name
  subnetwork = google_compute_subnetwork.gke.name

  # IP allocation policy for pods and services
  ip_allocation_policy {
    cluster_secondary_range_name  = "gke-pods"
    services_secondary_range_name = "gke-services"
  }

  # Private cluster configuration
  private_cluster_config {
    enable_private_nodes    = true
    enable_private_endpoint = false
    master_ipv4_cidr_block = "172.17.0.0/28"

    master_global_access_config {
      enabled = true
    }
  }

  # Network policy
  network_policy {
    enabled  = true
    provider = "CALICO"
  }

  # Workload Identity
  workload_identity_config {
    workload_pool = "${var.project_id}.svc.id.goog"
  }

  # Logging and monitoring
  logging_config {
    enable_components = [
      "SYSTEM_COMPONENTS",
      "WORKLOADS",
      "API_SERVER",
      "CONTROLLER_MANAGER",
      "SCHEDULER"
    ]
  }

  monitoring_config {
    enable_components = [
      "SYSTEM_COMPONENTS",
      "WORKLOADS",
      "DAEMONSET",
      "DEPLOYMENT",
      "HPA",
      "POD",
      "STATEFULSET",
      "STORAGE"
    ]

    managed_prometheus {
      enabled = true
    }
  }

  # Addons
  addons_config {
    http_load_balancing {
      disabled = false
    }

    horizontal_pod_autoscaling {
      disabled = false
    }

    network_policy_config {
      disabled = false
    }

    gcp_filestore_csi_driver_config {
      enabled = true
    }

    gcs_fuse_csi_driver_config {
      enabled = true
    }
  }

  # Security
  enable_shielded_nodes = true

  # Resource labels
  resource_labels = var.labels

  depends_on = [
    google_project_service.required_apis,
    google_compute_subnetwork.gke,
  ]

  timeouts {
    create = "30m"
    update = "40m"
  }
}

# Separately managed node pool
resource "google_container_node_pool" "primary_nodes" {
  name       = "${var.resource_prefix}-node-pool"
  location   = var.region
  cluster    = google_container_cluster.primary.name
  node_count = var.node_count

  node_config {
    preemptible  = var.environment != "prod"
    machine_type = var.node_machine_type

    # Google recommends custom service accounts that have cloud-platform scope and permissions granted via IAM Roles.
    service_account = google_service_account.gke_nodes.email
    oauth_scopes = [
      "https://www.googleapis.com/auth/cloud-platform"
    ]

    labels = merge(var.labels, {
      node-pool = "primary"
    })

    tags = ["gke-node", "internal"]

    shielded_instance_config {
      enable_secure_boot          = true
      enable_integrity_monitoring = true
    }

    # Workload Identity
    workload_metadata_config {
      mode = "GKE_METADATA"
    }

    disk_size_gb = 50
    disk_type    = "pd-standard"
    image_type   = "COS_CONTAINERD"
  }

  management {
    auto_repair  = true
    auto_upgrade = true
  }

  upgrade_settings {
    max_surge       = 1
    max_unavailable = 0
  }
}

# Service account for GKE nodes
resource "google_service_account" "gke_nodes" {
  account_id   = "${var.resource_prefix}-gke-nodes"
  display_name = "GKE Nodes Service Account"
  project      = var.project_id
}

# IAM bindings for GKE node service account
resource "google_project_iam_member" "gke_nodes" {
  for_each = toset([
    "roles/logging.logWriter",
    "roles/monitoring.metricWriter",
    "roles/monitoring.viewer",
    "roles/stackdriver.resourceMetadata.writer",
    "roles/storage.objectViewer",
    "roles/artifactregistry.reader"
  ])

  project = var.project_id
  role    = each.key
  member  = "serviceAccount:${google_service_account.gke_nodes.email}"
}

# === Cloud SQL PostgreSQL ===
resource "google_sql_database_instance" "postgres" {
  name             = "${var.resource_prefix}-postgres-${random_id.suffix.hex}"
  database_version = "POSTGRES_15"
  region           = var.region
  project          = var.project_id

  deletion_protection = var.environment == "prod"

  settings {
    tier              = var.db_tier
    availability_type = var.environment == "prod" ? "REGIONAL" : "ZONAL"
    disk_type         = "PD_SSD"
    disk_size         = var.db_disk_size
    disk_autoresize   = true

    backup_configuration {
      enabled                        = true
      start_time                     = "03:00"
      point_in_time_recovery_enabled = true
      backup_retention_settings {
        retained_backups = var.environment == "prod" ? 30 : 7
      }
    }

    ip_configuration {
      ipv4_enabled    = false
      private_network = google_compute_network.main.id
      require_ssl     = true

      dynamic "authorized_networks" {
        for_each = var.authorized_networks
        content {
          name  = authorized_networks.value.name
          value = authorized_networks.value.value
        }
      }
    }

    database_flags {
      name  = "log_statement"
      value = "all"
    }

    database_flags {
      name  = "log_min_duration_statement"
      value = "1000"
    }

    maintenance_window {
      day          = 7
      hour         = 4
      update_track = "stable"
    }

    user_labels = var.labels
  }

  depends_on = [
    google_service_networking_connection.private_vpc_connection,
    google_project_service.required_apis
  ]

  timeouts {
    create = "20m"
    update = "20m"
    delete = "20m"
  }
}

# Database
resource "google_sql_database" "database" {
  name     = replace("${var.app_name}_${var.environment}", "-", "_")
  instance = google_sql_database_instance.postgres.name
  project  = var.project_id
}

# Database user
resource "google_sql_user" "users" {
  name     = var.db_admin_username
  instance = google_sql_database_instance.postgres.name
  password = local.db_password
  project  = var.project_id
}

# Private service connection for Cloud SQL
resource "google_compute_global_address" "private_ip_address" {
  name          = "${var.resource_prefix}-private-ip"
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = google_compute_network.main.id
  project       = var.project_id
}

resource "google_service_networking_connection" "private_vpc_connection" {
  network                 = google_compute_network.main.id
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_ip_address.name]
}

# === Memorystore Redis ===
resource "google_redis_instance" "cache" {
  name           = "${var.resource_prefix}-redis"
  tier           = "STANDARD_HA"
  memory_size_gb = var.redis_memory_size
  region         = var.region
  project        = var.project_id

  location_id             = "${var.region}-a"
  alternative_location_id = "${var.region}-b"

  authorized_network = google_compute_network.main.id

  redis_version     = "REDIS_7_0"
  display_name      = "Redis cache for ${var.app_name}"
  reserved_ip_range = "10.1.0.0/29"

  auth_enabled = true
  
  redis_configs = {
    maxmemory-policy = "allkeys-lru"
    notify-keyspace-events = "Ex"
  }

  maintenance_policy {
    weekly_maintenance_window {
      day = "SUNDAY"
      start_time {
        hours   = 4
        minutes = 0
      }
    }
  }

  labels = var.labels

  depends_on = [google_project_service.required_apis]

  timeouts {
    create = "20m"
    update = "20m"
    delete = "20m"
  }
}

# === Cloud Storage ===
resource "google_storage_bucket" "storage" {
  name     = "${var.resource_prefix}-storage-${random_id.suffix.hex}"
  location = var.region
  project  = var.project_id

  storage_class = "STANDARD"
  
  # Prevent accidental deletion
  lifecycle_rule {
    condition {
      age = var.environment == "prod" ? 365 : 30
    }
    action {
      type = "Delete"
    }
  }

  # Versioning
  versioning {
    enabled = var.environment == "prod"
  }

  # Encryption
  encryption {
    default_kms_key_name = google_kms_crypto_key.storage.id
  }

  # Public access prevention
  public_access_prevention = "enforced"

  # Uniform bucket-level access
  uniform_bucket_level_access = true

  labels = var.labels

  depends_on = [google_project_service.required_apis]
}

# KMS key for storage encryption
resource "google_kms_key_ring" "key_ring" {
  name     = "${var.resource_prefix}-keyring"
  location = var.region
  project  = var.project_id
}

resource "google_kms_crypto_key" "storage" {
  name     = "${var.resource_prefix}-storage-key"
  key_ring = google_kms_key_ring.key_ring.id
  project  = var.project_id

  rotation_period = "7776000s" # 90 days

  lifecycle {
    prevent_destroy = true
  }
}

# Grant Cloud Storage service account access to KMS key
resource "google_kms_crypto_key_iam_member" "storage" {
  crypto_key_id = google_kms_crypto_key.storage.id
  role          = "roles/cloudkms.cryptoKeyEncrypterDecrypter"
  member        = "serviceAccount:service-${data.google_project.project.number}@gs-project-accounts.iam.gserviceaccount.com"
}

data "google_project" "project" {
  project_id = var.project_id
}

# === Artifact Registry ===
resource "google_artifact_registry_repository" "app" {
  location      = var.region
  repository_id = "${var.resource_prefix}-app"
  description   = "Container registry for ${var.app_name}"
  format        = "DOCKER"
  project       = var.project_id

  labels = var.labels

  depends_on = [google_project_service.required_apis]
}

# === Cloud CDN with Load Balancer ===
# Backend bucket for static content
resource "google_compute_backend_bucket" "static_backend" {
  name        = "${var.resource_prefix}-static-backend"
  bucket_name = google_storage_bucket.storage.name
  enable_cdn  = true
  project     = var.project_id

  cdn_policy {
    cache_mode                   = "CACHE_ALL_STATIC"
    default_ttl                  = 3600
    client_ttl                   = 7200
    max_ttl                      = 86400
    negative_caching             = true
    serve_while_stale            = 86400
    
    cache_key_policy {
      include_host           = true
      include_protocol       = true
      include_query_string   = false
    }
  }
}

# URL map for load balancer
resource "google_compute_url_map" "cdn" {
  name            = "${var.resource_prefix}-url-map"
  default_service = google_compute_backend_bucket.static_backend.id
  project         = var.project_id
}

# HTTP(S) proxy
resource "google_compute_target_https_proxy" "cdn" {
  name             = "${var.resource_prefix}-https-proxy"
  url_map          = google_compute_url_map.cdn.id
  ssl_certificates = [google_compute_managed_ssl_certificate.cdn.id]
  project          = var.project_id
}

# SSL certificate
resource "google_compute_managed_ssl_certificate" "cdn" {
  name     = "${var.resource_prefix}-ssl-cert"
  project  = var.project_id

  managed {
    domains = [var.domain_name != "" ? var.domain_name : "${var.resource_prefix}.example.com"]
  }
}

# Global forwarding rule
resource "google_compute_global_forwarding_rule" "cdn" {
  name                  = "${var.resource_prefix}-forwarding-rule"
  target                = google_compute_target_https_proxy.cdn.id
  port_range            = "443"
  ip_protocol           = "TCP"
  load_balancing_scheme = "EXTERNAL"
  project               = var.project_id
}

# === Pub/Sub (GCP equivalent of Event Hub/EventBridge) ===
resource "google_pubsub_topic" "app_events" {
  name    = "${var.resource_prefix}-events"
  project = var.project_id

  labels = var.labels

  message_retention_duration = "604800s" # 7 days

  depends_on = [google_project_service.required_apis]
}

resource "google_pubsub_subscription" "app_events" {
  name    = "${var.resource_prefix}-events-subscription"
  topic   = google_pubsub_topic.app_events.name
  project = var.project_id

  labels = var.labels

  # Delivery settings
  ack_deadline_seconds = 20
  
  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  expiration_policy {
    ttl = "300000.5s" # Never expires
  }
}

# === Secret Manager ===
resource "google_secret_manager_secret" "app_secrets" {
  secret_id = "${var.resource_prefix}-app-secrets"
  project   = var.project_id

  labels = var.labels

  replication {
    automatic = true
  }

  depends_on = [google_project_service.required_apis]
}

resource "google_secret_manager_secret_version" "app_secrets" {
  secret = google_secret_manager_secret.app_secrets.id

  secret_data = jsonencode({
    database_url = "postgresql://${var.db_admin_username}:${local.db_password}@${google_sql_database_instance.postgres.private_ip_address}:5432/${google_sql_database.database.name}"
    redis_url    = "redis://:${google_redis_instance.cache.auth_string}@${google_redis_instance.cache.host}:${google_redis_instance.cache.port}"
    jwt_secret   = local.jwt_secret
    grafana_password = local.grafana_password
  })
}

resource "random_password" "jwt_secret" {
  length  = 64
  special = true
}

# === Cloud Logging ===
resource "google_logging_log_sink" "app_sink" {
  name        = "${var.resource_prefix}-app-sink"
  destination = "storage.googleapis.com/${google_storage_bucket.logs.name}"
  filter      = "resource.type=\"gke_container\""
  project     = var.project_id

  unique_writer_identity = true
}

resource "google_storage_bucket" "logs" {
  name     = "${var.resource_prefix}-logs-${random_id.suffix.hex}"
  location = var.region
  project  = var.project_id

  storage_class = "NEARLINE"
  
  lifecycle_rule {
    condition {
      age = var.log_retention_days
    }
    action {
      type = "Delete"
    }
  }

  labels = var.labels
}

# Grant log sink writer access to the bucket
resource "google_storage_bucket_iam_member" "log_sink_writer" {
  bucket = google_storage_bucket.logs.name
  role   = "roles/storage.objectCreator"
  member = google_logging_log_sink.app_sink.writer_identity
}
