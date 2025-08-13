# Outputs for AWS module

# === VPC and Networking ===
output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.main.id
}

output "vpc_cidr" {
  description = "CIDR block of the VPC"
  value       = aws_vpc.main.cidr_block
}

output "public_subnet_ids" {
  description = "IDs of the public subnets"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "IDs of the private subnets"
  value       = aws_subnet.private[*].id
}

output "database_subnet_ids" {
  description = "IDs of the database subnets"
  value       = aws_subnet.database[*].id
}

# === EKS Cluster ===
output "eks_cluster_id" {
  description = "ID of the EKS cluster"
  value       = aws_eks_cluster.main.id
}

output "eks_cluster_name" {
  description = "Name of the EKS cluster"
  value       = aws_eks_cluster.main.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint for EKS control plane"
  value       = aws_eks_cluster.main.endpoint
}

output "eks_cluster_security_group_id" {
  description = "Security group ID attached to the EKS cluster"
  value       = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
}

output "eks_cluster_iam_role_name" {
  description = "IAM role name associated with EKS cluster"
  value       = aws_iam_role.eks_cluster.name
}

output "eks_cluster_iam_role_arn" {
  description = "IAM role ARN associated with EKS cluster"
  value       = aws_iam_role.eks_cluster.arn
}

output "eks_cluster_version" {
  description = "Kubernetes server version for the EKS cluster"
  value       = aws_eks_cluster.main.version
}

output "eks_cluster_ca_certificate" {
  description = "Base64 encoded certificate data required to communicate with the cluster"
  value       = aws_eks_cluster.main.certificate_authority[0].data
}

# === EKS Node Group ===
output "eks_node_group_arn" {
  description = "Amazon Resource Name (ARN) of the EKS Node Group"
  value       = aws_eks_node_group.main.arn
}

output "eks_node_group_status" {
  description = "Status of the EKS Node Group"
  value       = aws_eks_node_group.main.status
}

# === RDS Database ===
output "rds_instance_endpoint" {
  description = "RDS instance endpoint"
  value       = aws_db_instance.postgres.endpoint
  sensitive   = true
}

output "rds_instance_hosted_zone_id" {
  description = "Hosted zone ID of the RDS instance"
  value       = aws_db_instance.postgres.hosted_zone_id
}

output "rds_instance_id" {
  description = "RDS instance ID"
  value       = aws_db_instance.postgres.id
}

output "rds_instance_resource_id" {
  description = "RDS Resource ID of this instance"
  value       = aws_db_instance.postgres.resource_id
}

output "rds_instance_status" {
  description = "RDS instance status"
  value       = aws_db_instance.postgres.status
}

output "rds_instance_name" {
  description = "RDS instance database name"
  value       = aws_db_instance.postgres.db_name
}

output "rds_instance_username" {
  description = "RDS instance root username"
  value       = aws_db_instance.postgres.username
  sensitive   = true
}

output "rds_connection_string" {
  description = "PostgreSQL connection string"
  value       = "postgresql://${aws_db_instance.postgres.username}:${var.db_admin_password}@${aws_db_instance.postgres.endpoint}/${aws_db_instance.postgres.db_name}"
  sensitive   = true
}

# === ElastiCache Redis ===
output "redis_replication_group_id" {
  description = "ID of the ElastiCache replication group"
  value       = aws_elasticache_replication_group.redis.id
}

output "redis_replication_group_primary_endpoint_address" {
  description = "Address of the endpoint for the primary node in the replication group"
  value       = aws_elasticache_replication_group.redis.configuration_endpoint_address
}

output "redis_replication_group_configuration_endpoint_address" {
  description = "Configuration endpoint address of the replication group"
  value       = aws_elasticache_replication_group.redis.configuration_endpoint_address
}

output "redis_auth_token" {
  description = "Redis AUTH token"
  value       = random_password.redis_auth.result
  sensitive   = true
}

output "redis_connection_string" {
  description = "Redis connection string"
  value       = "redis://:${random_password.redis_auth.result}@${aws_elasticache_replication_group.redis.configuration_endpoint_address}:6379"
  sensitive   = true
}

# === S3 Storage ===
output "s3_bucket_id" {
  description = "ID of the S3 bucket"
  value       = aws_s3_bucket.storage.id
}

output "s3_bucket_arn" {
  description = "ARN of the S3 bucket"
  value       = aws_s3_bucket.storage.arn
}

output "s3_bucket_domain_name" {
  description = "Domain name of the S3 bucket"
  value       = aws_s3_bucket.storage.bucket_domain_name
}

output "s3_bucket_regional_domain_name" {
  description = "Regional domain name of the S3 bucket"
  value       = aws_s3_bucket.storage.bucket_regional_domain_name
}

output "s3_access_key" {
  description = "S3 access credentials (placeholder - use IAM roles in production)"
  value       = "use-iam-roles-instead"
  sensitive   = true
}

# === ECR Container Registry ===
output "ecr_repository_arn" {
  description = "Full ARN of the ECR repository"
  value       = aws_ecr_repository.app.arn
}

output "ecr_repository_name" {
  description = "Name of the ECR repository"
  value       = aws_ecr_repository.app.name
}

output "ecr_repository_registry_id" {
  description = "Registry ID where the repository was created"
  value       = aws_ecr_repository.app.registry_id
}

output "ecr_repository_url" {
  description = "URL of the ECR repository"
  value       = aws_ecr_repository.app.repository_url
}

output "ecr_token" {
  description = "ECR authentication token (placeholder - use aws ecr get-login-token)"
  value       = "use-aws-cli-get-login-token"
  sensitive   = true
}

# === CloudFront CDN ===
output "cloudfront_distribution_id" {
  description = "ID of the CloudFront distribution"
  value       = aws_cloudfront_distribution.cdn.id
}

output "cloudfront_distribution_arn" {
  description = "ARN of the CloudFront distribution"
  value       = aws_cloudfront_distribution.cdn.arn
}

output "cloudfront_distribution_caller_reference" {
  description = "Internal value used by CloudFront to allow future updates to the distribution configuration"
  value       = aws_cloudfront_distribution.cdn.caller_reference
}

output "cloudfront_distribution_status" {
  description = "Current status of the distribution"
  value       = aws_cloudfront_distribution.cdn.status
}

output "cloudfront_domain_name" {
  description = "Domain name corresponding to the distribution"
  value       = aws_cloudfront_distribution.cdn.domain_name
}

output "cloudfront_hosted_zone_id" {
  description = "CloudFront Route 53 zone ID"
  value       = aws_cloudfront_distribution.cdn.hosted_zone_id
}

# === EventBridge ===
output "eventbridge_bus_name" {
  description = "Name of the EventBridge custom bus"
  value       = aws_cloudwatch_event_bus.app.name
}

output "eventbridge_bus_arn" {
  description = "ARN of the EventBridge custom bus"
  value       = aws_cloudwatch_event_bus.app.arn
}

# === Secrets Manager ===
output "secrets_manager_secret_id" {
  description = "ID of the Secrets Manager secret"
  value       = aws_secretsmanager_secret.app_secrets.id
}

output "secrets_manager_secret_arn" {
  description = "ARN of the Secrets Manager secret"
  value       = aws_secretsmanager_secret.app_secrets.arn
}

# === CloudWatch Logs ===
output "cloudwatch_log_group_name" {
  description = "Name of the CloudWatch log group"
  value       = aws_cloudwatch_log_group.app.name
}

output "cloudwatch_log_group_arn" {
  description = "ARN of the CloudWatch log group"
  value       = aws_cloudwatch_log_group.app.arn
}

# === Connection Information Summary ===
output "connection_info" {
  description = "Summary of connection information"
  value = {
    cluster_name     = aws_eks_cluster.main.name
    cluster_endpoint = aws_eks_cluster.main.endpoint
    database_endpoint = aws_db_instance.postgres.endpoint
    redis_endpoint   = aws_elasticache_replication_group.redis.configuration_endpoint_address
    s3_bucket       = aws_s3_bucket.storage.id
    cdn_domain      = aws_cloudfront_distribution.cdn.domain_name
    ecr_repository  = aws_ecr_repository.app.repository_url
  }
}

# === Deployment Information ===
output "deployment_info" {
  description = "Information needed for application deployment"
  value = {
    # Kubernetes
    cluster_name = aws_eks_cluster.main.name
    cluster_ca   = aws_eks_cluster.main.certificate_authority[0].data
    cluster_endpoint = aws_eks_cluster.main.endpoint
    
    # Container Registry
    registry_url = aws_ecr_repository.app.repository_url
    
    # Storage
    storage_bucket = aws_s3_bucket.storage.id
    cdn_domain    = aws_cloudfront_distribution.cdn.domain_name
    
    # Secrets
    secrets_arn = aws_secretsmanager_secret.app_secrets.arn
    
    # EventBridge
    event_bus_name = aws_cloudwatch_event_bus.app.name
    
    # Logs
    log_group = aws_cloudwatch_log_group.app.name
  }
  sensitive = true
}
