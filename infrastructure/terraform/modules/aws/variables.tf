# Variables for AWS module

# === Core Configuration ===
variable "app_name" {
  description = "Name of the application"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "region" {
  description = "AWS region for resource deployment"
  type        = string
  default     = "us-east-1"
}

variable "resource_prefix" {
  description = "Prefix for resource names"
  type        = string
}

variable "tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default     = {}
}

# === Network Configuration ===
variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "subnets" {
  description = "Subnet configuration"
  type = object({
    public  = list(string)
    private = list(string)
    data    = list(string)
  })
  default = {
    public  = ["10.0.1.0/24", "10.0.2.0/24"]
    private = ["10.0.10.0/24", "10.0.11.0/24"]
    data    = ["10.0.20.0/24", "10.0.21.0/24"]
  }
}

# === Database Configuration ===
variable "db_admin_username" {
  description = "Database administrator username"
  type        = string
  sensitive   = true
}

variable "db_admin_password" {
  description = "Database administrator password"
  type        = string
  default     = ""  # Empty default - will be generated if not provided
  sensitive   = true
}

variable "grafana_admin_password" {
  description = "Grafana admin password"
  type        = string
  default     = ""  # Empty default - will be generated if not provided
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT secret for application authentication"
  type        = string
  default     = ""  # Empty default - will be generated if not provided
  sensitive   = true
}

variable "db_allocated_storage" {
  description = "Database allocated storage in GB"
  type        = number
  default     = 20
}

variable "db_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

# === Redis Configuration ===
variable "redis_node_type" {
  description = "ElastiCache node type"
  type        = string
  default     = "cache.t3.micro"
}

# === Kubernetes Configuration ===
variable "k8s_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "node_count" {
  description = "Number of EKS worker nodes"
  type        = number
  default     = 2
}

variable "node_type" {
  description = "EC2 instance type for EKS nodes"
  type        = string
  default     = "t3.medium"
}

# === Storage Configuration ===
variable "storage_tier" {
  description = "S3 storage tier"
  type        = string
  default     = "STANDARD"
}

# === Monitoring Configuration ===
variable "log_retention_days" {
  description = "CloudWatch log retention period in days"
  type        = number
  default     = 30
}

# === Security Configuration ===
variable "allowed_ip_ranges" {
  description = "IP ranges allowed to access resources"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}
