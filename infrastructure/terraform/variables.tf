# Variables for multi-cloud web app infrastructure

# === Core Configuration ===
variable "app_name" {
  description = "Name of the application"
  type        = string
  default     = "webapp-prod"
  
  validation {
    condition     = length(var.app_name) <= 15 && can(regex("^[a-z0-9-]+$", var.app_name))
    error_message = "App name must be lowercase, alphanumeric with dashes only, max 15 characters."
  }
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
  
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "owner" {
  description = "Owner/team responsible for the infrastructure"
  type        = string
  default     = "devops-team"
}

# === Cloud Provider Selection ===
variable "cloud_provider" {
  description = "Cloud provider to use (azure, aws, gcp)"
  type        = string
  default     = "azure"
  
  validation {
    condition     = contains(["azure", "aws", "gcp"], var.cloud_provider)
    error_message = "Cloud provider must be one of: azure, aws, gcp."
  }
}

variable "region" {
  description = "Cloud region for resource deployment"
  type        = string
  default     = "East US"
}

variable "gcp_project_id" {
  description = "GCP Project ID (required when using GCP)"
  type        = string
  default     = ""
}

# === Network Configuration ===
variable "vpc_cidr" {
  description = "CIDR block for VPC/VNet"
  type        = string
  default     = "10.0.0.0/16"
}

# === Database Configuration ===
variable "db_admin_username" {
  description = "Database administrator username"
  type        = string
  default     = "dbadmin"
  sensitive   = true
}

variable "db_admin_password" {
  description = "Database administrator password"
  type        = string
  default     = "TempPassword123!"
  sensitive   = true
  
  validation {
    condition     = length(var.db_admin_password) >= 12
    error_message = "Database password must be at least 12 characters long."
  }
}

variable "db_allocated_storage" {
  description = "Database allocated storage in GB"
  type        = number
  default     = 20
}

variable "db_instance_class" {
  description = "Database instance class/tier"
  type        = string
  default     = "db.t3.micro"  # AWS default, overridden per provider
}

# === Redis Configuration ===
variable "redis_capacity" {
  description = "Redis cache capacity (Azure)"
  type        = number
  default     = 0  # C0 - 250MB
}

variable "redis_node_type" {
  description = "Redis node type (AWS)"
  type        = string
  default     = "cache.t3.micro"
}

variable "redis_memory_size" {
  description = "Redis memory size in GB (GCP)"
  type        = number
  default     = 1
}

# === Kubernetes Configuration ===
variable "k8s_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "node_count" {
  description = "Number of Kubernetes worker nodes"
  type        = number
  default     = 2
  
  validation {
    condition     = var.node_count >= 1 && var.node_count <= 10
    error_message = "Node count must be between 1 and 10."
  }
}

# Azure-specific node configuration
variable "node_vm_size" {
  description = "Azure VM size for AKS nodes"
  type        = string
  default     = "Standard_B2s"
}

# AWS-specific node configuration
variable "node_instance_type" {
  description = "AWS instance type for EKS nodes"
  type        = string
  default     = "t3.medium"
}

# GCP-specific node configuration
variable "node_machine_type" {
  description = "GCP machine type for GKE nodes"
  type        = string
  default     = "e2-standard-2"
}

# === Storage Configuration ===
variable "storage_tier" {
  description = "Storage tier (Standard_LRS, Premium_LRS, etc.)"
  type        = string
  default     = "Standard_LRS"
}

# === CDN Configuration ===
variable "cdn_sku" {
  description = "CDN SKU/tier"
  type        = string
  default     = "Standard_Microsoft"  # Azure default
}

# === Event Hub Configuration ===
variable "event_hub_sku" {
  description = "Event Hub SKU tier"
  type        = string
  default     = "Standard"
}

variable "event_hub_capacity" {
  description = "Event Hub throughput units"
  type        = number
  default     = 1
}

# === Security Configuration ===
variable "enable_network_security" {
  description = "Enable network security groups and firewalls"
  type        = bool
  default     = true
}

variable "allowed_ip_ranges" {
  description = "IP ranges allowed to access resources"
  type        = list(string)
  default     = ["0.0.0.0/0"]  # Restrict in production
}

# === Monitoring Configuration ===
variable "log_retention_days" {
  description = "Log retention period in days"
  type        = number
  default     = 30
}

variable "enable_monitoring" {
  description = "Enable monitoring and alerting"
  type        = bool
  default     = true
}

# === Monitoring Configuration ===
variable "grafana_admin_password" {
  description = "Grafana admin password"
  type        = string
  default     = "admin123!"
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT secret for application authentication"
  type        = string
  default     = "change-this-jwt-secret-in-production"
  sensitive   = true
}

# === Feature Flags ===
variable "deploy_k8s_resources" {
  description = "Deploy Kubernetes resources after cluster creation"
  type        = bool
  default     = false
}

variable "enable_auto_scaling" {
  description = "Enable cluster auto-scaling"
  type        = bool
  default     = true
}

variable "enable_backup" {
  description = "Enable automated backups"
  type        = bool
  default     = true
}

# === Cost Optimization ===
variable "environment_size" {
  description = "Environment size (small, medium, large) for resource sizing"
  type        = string
  default     = "small"
  
  validation {
    condition     = contains(["small", "medium", "large"], var.environment_size)
    error_message = "Environment size must be one of: small, medium, large."
  }
}

# Size mappings for different environments
locals {
  size_config = {
    small = {
      db_instance_class = "db.t3.micro"
      node_count       = 2
      redis_capacity   = 0
    }
    medium = {
      db_instance_class = "db.t3.small"
      node_count       = 3
      redis_capacity   = 1
    }
    large = {
      db_instance_class = "db.t3.medium"
      node_count       = 5
      redis_capacity   = 2
    }
  }
}
