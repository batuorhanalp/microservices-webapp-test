# AWS module for production web app infrastructure
# Creates EKS cluster, RDS PostgreSQL, ElastiCache Redis, S3, CloudFront, and EventBridge

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

# Configure the AWS Provider
provider "aws" {
  region = var.region
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

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

# === VPC and Networking ===
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-vpc"
    "kubernetes.io/cluster/${var.resource_prefix}-eks" = "shared"
  })
}

resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-igw"
  })
}

# Public subnets for ALB and NAT Gateway
resource "aws_subnet" "public" {
  count = length(var.subnets.public)

  vpc_id                  = aws_vpc.main.id
  cidr_block              = var.subnets.public[count.index]
  availability_zone       = data.aws_availability_zones.available.names[count.index]
  map_public_ip_on_launch = true

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-public-subnet-${count.index + 1}"
    Type = "Public"
    "kubernetes.io/cluster/${var.resource_prefix}-eks" = "shared"
    "kubernetes.io/role/elb"                           = "1"
  })
}

# Private subnets for EKS worker nodes
resource "aws_subnet" "private" {
  count = length(var.subnets.private)

  vpc_id            = aws_vpc.main.id
  cidr_block        = var.subnets.private[count.index]
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-private-subnet-${count.index + 1}"
    Type = "Private"
    "kubernetes.io/cluster/${var.resource_prefix}-eks" = "shared"
    "kubernetes.io/role/internal-elb"                  = "1"
  })
}

# Database subnets
resource "aws_subnet" "database" {
  count = length(var.subnets.data)

  vpc_id            = aws_vpc.main.id
  cidr_block        = var.subnets.data[count.index]
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-database-subnet-${count.index + 1}"
    Type = "Database"
  })
}

# NAT Gateway for private subnet internet access
resource "aws_eip" "nat" {
  count = length(aws_subnet.public)

  domain     = "vpc"
  depends_on = [aws_internet_gateway.main]

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-nat-eip-${count.index + 1}"
  })
}

resource "aws_nat_gateway" "main" {
  count = length(aws_subnet.public)

  allocation_id = aws_eip.nat[count.index].id
  subnet_id     = aws_subnet.public[count.index].id

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-nat-${count.index + 1}"
  })

  depends_on = [aws_internet_gateway.main]
}

# Route tables
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-public-rt"
  })
}

resource "aws_route_table" "private" {
  count = length(aws_subnet.private)

  vpc_id = aws_vpc.main.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main[count.index].id
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-private-rt-${count.index + 1}"
  })
}

resource "aws_route_table" "database" {
  vpc_id = aws_vpc.main.id

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-database-rt"
  })
}

# Route table associations
resource "aws_route_table_association" "public" {
  count = length(aws_subnet.public)

  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_route_table_association" "private" {
  count = length(aws_subnet.private)

  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private[count.index].id
}

resource "aws_route_table_association" "database" {
  count = length(aws_subnet.database)

  subnet_id      = aws_subnet.database[count.index].id
  route_table_id = aws_route_table.database.id
}

# === Security Groups ===
resource "aws_security_group" "eks_cluster" {
  name_prefix = "${var.resource_prefix}-eks-cluster-"
  vpc_id      = aws_vpc.main.id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-eks-cluster-sg"
  })
}

resource "aws_security_group" "eks_nodes" {
  name_prefix = "${var.resource_prefix}-eks-nodes-"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "Self"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    self        = true
  }

  ingress {
    description     = "Cluster"
    from_port       = 1025
    to_port         = 65535
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_cluster.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-eks-nodes-sg"
  })
}

resource "aws_security_group" "rds" {
  name_prefix = "${var.resource_prefix}-rds-"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "PostgreSQL from EKS"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_nodes.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-rds-sg"
  })
}

resource "aws_security_group" "redis" {
  name_prefix = "${var.resource_prefix}-redis-"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "Redis from EKS"
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_nodes.id]
  }

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-redis-sg"
  })
}

# === IAM Roles for EKS ===
resource "aws_iam_role" "eks_cluster" {
  name = "${var.resource_prefix}-eks-cluster-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "eks.amazonaws.com"
        }
      },
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "eks_cluster_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSClusterPolicy"
  role       = aws_iam_role.eks_cluster.name
}

resource "aws_iam_role" "eks_nodes" {
  name = "${var.resource_prefix}-eks-nodes-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      },
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "eks_worker_node_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
  role       = aws_iam_role.eks_nodes.name
}

resource "aws_iam_role_policy_attachment" "eks_cni_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
  role       = aws_iam_role.eks_nodes.name
}

resource "aws_iam_role_policy_attachment" "eks_container_registry_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
  role       = aws_iam_role.eks_nodes.name
}

# === EKS Cluster ===
resource "aws_eks_cluster" "main" {
  name     = "${var.resource_prefix}-eks"
  role_arn = aws_iam_role.eks_cluster.arn
  version  = var.k8s_version

  vpc_config {
    subnet_ids              = concat(aws_subnet.public[*].id, aws_subnet.private[*].id)
    endpoint_private_access = true
    endpoint_public_access  = true
    security_group_ids      = [aws_security_group.eks_cluster.id]
  }

  enabled_cluster_log_types = ["api", "audit", "authenticator", "controllerManager", "scheduler"]

  depends_on = [
    aws_iam_role_policy_attachment.eks_cluster_policy,
  ]

  tags = var.tags
}

# === EKS Node Group ===
resource "aws_eks_node_group" "main" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "${var.resource_prefix}-nodes"
  node_role_arn   = aws_iam_role.eks_nodes.arn
  subnet_ids      = aws_subnet.private[*].id
  instance_types  = [var.node_type]

  scaling_config {
    desired_size = var.node_count
    max_size     = var.node_count * 2
    min_size     = 1
  }

  update_config {
    max_unavailable = 1
  }

  depends_on = [
    aws_iam_role_policy_attachment.eks_worker_node_policy,
    aws_iam_role_policy_attachment.eks_cni_policy,
    aws_iam_role_policy_attachment.eks_container_registry_policy,
  ]

  tags = var.tags
}

# === RDS PostgreSQL ===
resource "aws_db_subnet_group" "main" {
  name       = "${var.resource_prefix}-db-subnet-group"
  subnet_ids = aws_subnet.database[*].id

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-db-subnet-group"
  })
}

resource "aws_db_instance" "postgres" {
  identifier = "${var.resource_prefix}-postgres"

  # Database configuration
  engine         = "postgres"
  engine_version = "15.4"
  instance_class = var.db_instance_class
  
  allocated_storage     = var.db_allocated_storage
  max_allocated_storage = var.db_allocated_storage * 2
  storage_type         = "gp2"
  storage_encrypted    = true

  # Database credentials
  db_name  = replace("${var.app_name}_${var.environment}", "-", "_")
  username = var.db_admin_username
  password = local.db_password
  
  # Network configuration
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  
  # Backup configuration
  backup_retention_period = var.environment == "prod" ? 30 : 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "Sun:04:00-Sun:05:00"
  
  # Additional settings
  skip_final_snapshot       = var.environment != "prod"
  final_snapshot_identifier = var.environment == "prod" ? "${var.resource_prefix}-postgres-final-snapshot" : null
  deletion_protection       = var.environment == "prod"
  
  # Performance and monitoring
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]
  monitoring_interval             = 60
  monitoring_role_arn            = aws_iam_role.rds_monitoring.arn
  performance_insights_enabled   = true

  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-postgres"
  })
}

# RDS Enhanced Monitoring Role
resource "aws_iam_role" "rds_monitoring" {
  name = "${var.resource_prefix}-rds-monitoring-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "monitoring.rds.amazonaws.com"
        }
      },
    ]
  })
}

resource "aws_iam_role_policy_attachment" "rds_monitoring" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
  role       = aws_iam_role.rds_monitoring.name
}

# === ElastiCache Redis ===
resource "aws_elasticache_subnet_group" "main" {
  name       = "${var.resource_prefix}-cache-subnet-group"
  subnet_ids = aws_subnet.database[*].id

  tags = var.tags
}

resource "aws_elasticache_replication_group" "redis" {
  replication_group_id       = "${var.resource_prefix}-redis"
  description               = "Redis cache for ${var.app_name}"
  
  node_type            = var.redis_node_type
  port                = 6379
  parameter_group_name = "default.redis7"
  
  num_cache_clusters = 2
  
  subnet_group_name  = aws_elasticache_subnet_group.main.name
  security_group_ids = [aws_security_group.redis.id]
  
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  auth_token                = random_password.redis_auth.result
  
  # Backup configuration
  snapshot_retention_limit = var.environment == "prod" ? 7 : 1
  snapshot_window         = "03:00-05:00"
  
  # Automatic failover
  automatic_failover_enabled = true
  multi_az_enabled          = var.environment == "prod"

  tags = var.tags
}

resource "random_password" "redis_auth" {
  length  = 32
  special = false
}

# === S3 Storage ===
resource "aws_s3_bucket" "storage" {
  bucket = "${var.resource_prefix}-storage-${random_id.bucket_suffix.hex}"

  tags = var.tags
}

resource "random_id" "bucket_suffix" {
  byte_length = 4
}

resource "aws_s3_bucket_public_access_block" "storage" {
  bucket = aws_s3_bucket.storage.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_encryption" "storage" {
  bucket = aws_s3_bucket.storage.id

  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

resource "aws_s3_bucket_versioning" "storage" {
  bucket = aws_s3_bucket.storage.id
  versioning_configuration {
    status = var.environment == "prod" ? "Enabled" : "Suspended"
  }
}

# === ECR Container Registry ===
resource "aws_ecr_repository" "app" {
  name                 = "${var.resource_prefix}-app"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = var.tags
}

resource "aws_ecr_lifecycle_policy" "app" {
  repository = aws_ecr_repository.app.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 10 images"
        selection = {
          tagStatus     = "tagged"
          tagPrefixList = ["v"]
          countType     = "imageCountMoreThan"
          countNumber   = 10
        }
        action = {
          type = "expire"
        }
      },
      {
        rulePriority = 2
        description  = "Delete untagged images older than 1 day"
        selection = {
          tagStatus   = "untagged"
          countType   = "sinceImagePushed"
          countUnit   = "days"
          countNumber = 1
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

# === CloudFront CDN ===
resource "aws_cloudfront_origin_access_identity" "s3_oai" {
  comment = "OAI for ${var.resource_prefix} S3 bucket"
}

resource "aws_s3_bucket_policy" "cdn_access" {
  bucket = aws_s3_bucket.storage.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          AWS = aws_cloudfront_origin_access_identity.s3_oai.iam_arn
        }
        Action   = "s3:GetObject"
        Resource = "${aws_s3_bucket.storage.arn}/*"
      }
    ]
  })
}

resource "aws_cloudfront_distribution" "cdn" {
  origin {
    domain_name = aws_s3_bucket.storage.bucket_regional_domain_name
    origin_id   = "S3-${aws_s3_bucket.storage.id}"

    s3_origin_config {
      origin_access_identity = aws_cloudfront_origin_access_identity.s3_oai.cloudfront_access_identity_path
    }
  }

  enabled             = true
  default_root_object = "index.html"
  comment             = "CDN for ${var.app_name} ${var.environment}"
  price_class         = var.environment == "prod" ? "PriceClass_All" : "PriceClass_100"

  default_cache_behavior {
    allowed_methods        = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = "S3-${aws_s3_bucket.storage.id}"
    compress               = true
    viewer_protocol_policy = "redirect-to-https"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    min_ttl     = 0
    default_ttl = 3600
    max_ttl     = 86400
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
  }

  tags = var.tags
}

# === EventBridge (AWS equivalent of Event Hub) ===
resource "aws_cloudwatch_event_bus" "app" {
  name = "${var.resource_prefix}-events"
  tags = var.tags
}

# === Secrets Manager ===
resource "aws_secretsmanager_secret" "app_secrets" {
  name        = "${var.resource_prefix}-app-secrets"
  description = "Application secrets for ${var.app_name}"

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "app_secrets" {
  secret_id = aws_secretsmanager_secret.app_secrets.id
  secret_string = jsonencode({
    database_url = "postgresql://${var.db_admin_username}:${local.db_password}@${aws_db_instance.postgres.endpoint}/${aws_db_instance.postgres.db_name}"
    redis_url    = "redis://:${random_password.redis_auth.result}@${aws_elasticache_replication_group.redis.configuration_endpoint_address}:6379"
    jwt_secret   = local.jwt_secret
    grafana_password = local.grafana_password
  })
}

resource "random_password" "jwt_secret" {
  length  = 64
  special = true
}

# === CloudWatch Log Groups ===
resource "aws_cloudwatch_log_group" "app" {
  name              = "/aws/eks/${var.resource_prefix}/application"
  retention_in_days = var.log_retention_days

  tags = var.tags
}
