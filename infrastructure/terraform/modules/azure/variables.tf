# Variables for Azure module

variable "app_name" {
  description = "Name of the application"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "location" {
  description = "Azure location"
  type        = string
}

variable "resource_prefix" {
  description = "Prefix for all resource names"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

variable "vpc_cidr" {
  description = "CIDR block for VNet"
  type        = string
}

variable "subnets" {
  description = "Subnet configuration"
  type = object({
    public  = list(string)
    private = list(string)
    data    = list(string)
  })
}

variable "db_admin_username" {
  description = "Database administrator username"
  type        = string
  sensitive   = true
}

variable "db_admin_password" {
  description = "Database administrator password"
  type        = string
  sensitive   = true
}

variable "db_allocated_storage" {
  description = "Database allocated storage in GB"
  type        = number
  default     = 20
}

variable "redis_capacity" {
  description = "Redis cache capacity"
  type        = number
  default     = 0
}

variable "k8s_version" {
  description = "Kubernetes version"
  type        = string
}

variable "node_count" {
  description = "Number of AKS worker nodes"
  type        = number
}

variable "node_vm_size" {
  description = "VM size for AKS nodes"
  type        = string
}

variable "log_retention_days" {
  description = "Log retention period in days"
  type        = number
  default     = 30
}

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

variable "cdn_sku" {
  description = "CDN SKU tier"
  type        = string
  default     = "Standard_Microsoft"
}
