# Terraform Infrastructure

This directory contains Terraform configurations for deploying the production web app infrastructure across multiple cloud providers.

## Phase 2: Azure Infrastructure (Complete) ✅

The Azure infrastructure module provides all necessary resources for a production web application.

### Prerequisites

1. **Azure CLI** installed and authenticated:
   ```bash
   az login
   az account set --subscription "your-subscription-id"
   ```

2. **Terraform** installed (version >= 1.0):
   ```bash
   # macOS
   brew install terraform
   
   # Or download from https://terraform.io/downloads
   ```

### Quick Start - Deploy to Azure

1. **Navigate to terraform directory**:
   ```bash
   cd webapp-production/infrastructure/terraform
   ```

2. **Initialize Terraform**:
   ```bash
   terraform init
   ```

3. **Plan deployment with development environment**:
   ```bash
   terraform plan -var-file="environments/dev.tfvars"
   ```

4. **Deploy infrastructure**:
   ```bash
   terraform apply -var-file="environments/dev.tfvars"
   ```

5. **Get cluster credentials**:
   ```bash
   # Use the command from terraform output
   az aks get-credentials --resource-group webapp-prod-dev-rg --name webapp-prod-dev-aks
   ```

### Azure Resources Created

The Azure module creates the following resources:

#### Core Infrastructure
- **Resource Group**: `webapp-prod-dev-rg`
- **Virtual Network**: `webapp-prod-dev-vnet` (10.0.0.0/16)
- **Subnets**: AKS, Database, Redis
- **Network Security Groups**: Basic security rules

#### Compute & Container Orchestration
- **AKS Cluster**: `webapp-prod-dev-aks`
  - Kubernetes version: 1.28
  - Node pool: 2 × Standard_B2s VMs
  - Azure CNI networking
- **Azure Container Registry**: `webappproddevacr`

#### Database & Cache
- **PostgreSQL Flexible Server**: `webapp-prod-dev-postgres`
  - Version 15, B_Standard_B1ms
  - Private networking with DNS zone
  - Database: `appdb`
- **Azure Cache for Redis**: `webapp-prod-dev-redis`
  - Standard C0 (250MB)
  - SSL-only connections

#### Storage & CDN
- **Storage Account**: `webappprodddevstorage`
  - Containers: `static`, `media`
  - Public blob access for CDN
- **CDN Profile & Endpoint**: `webapp-prod-dev-cdn`
  - HTTPS redirect rules
  - Connected to storage account

#### Messaging & Events
- **Event Hub Namespace**: `webapp-prod-dev-eventhub`
  - Standard tier, 1 throughput unit
  - Event Hub: `app-events`

#### Security & Secrets
- **Key Vault**: `webapp-prod-dev-kv`
  - RBAC authorization
  - 7-day soft delete

#### Monitoring
- **Log Analytics Workspace**: `webapp-prod-dev-logs`
- **Application Insights**: `webapp-prod-dev-insights`
  - Connected to AKS for monitoring

### Configuration Files

- **`main.tf`**: Root configuration with provider modules
- **`variables.tf`**: All variable definitions with validation
- **`outputs.tf`**: Dynamic outputs based on cloud provider
- **`modules/azure/`**: Azure-specific resources
- **`environments/dev.tfvars`**: Development environment settings

### Environment Customization

Create custom `.tfvars` files for different environments:

```hcl
# environments/staging.tfvars
environment = "staging"
environment_size = "medium"
node_count = 3
redis_capacity = 1

# environments/prod.tfvars  
environment = "prod"
environment_size = "large"
node_count = 5
redis_capacity = 2
enable_backup = true
log_retention_days = 90
```

### Terraform Commands

```bash
# Initialize (first time only)
terraform init

# Validate configuration
terraform validate

# Plan changes
terraform plan -var-file="environments/dev.tfvars"

# Apply changes
terraform apply -var-file="environments/dev.tfvars"

# View current state
terraform show

# List resources
terraform state list

# Destroy infrastructure (be careful!)
terraform destroy -var-file="environments/dev.tfvars"
```

### Output Information

After deployment, Terraform provides connection information:

```bash
# View all outputs
terraform output

# View specific output
terraform output azure_aks_cluster_name
terraform output kubernetes_cluster_info
terraform output next_steps
```

### Security Considerations

1. **Database passwords** should be stored in Key Vault for production
2. **Network access** is currently open (0.0.0.0/0) - restrict for production
3. **Service principals** should be used instead of admin credentials
4. **Resource locks** should be applied to prevent accidental deletion

### Cost Optimization

The configuration includes cost optimization features:
- **Environment sizing**: Automatically adjusts resource sizes
- **Burstable database tiers**: B-series for development
- **Standard storage**: LRS replication for cost savings
- **Basic container registry**: Sufficient for development

### Troubleshooting

#### Common Issues

1. **Resource naming conflicts**:
   ```bash
   # Change app_name or environment in tfvars
   app_name = "myapp-unique"
   ```

2. **Subscription permissions**:
   ```bash
   # Verify permissions
   az account show
   az role assignment list --assignee your-email@domain.com
   ```

3. **Provider registration**:
   ```bash
   # Register required providers
   az provider register --namespace Microsoft.ContainerService
   az provider register --namespace Microsoft.DBforPostgreSQL
   ```

### Next Steps

1. **Phase 3**: Multi-cloud compatibility (AWS & GCP modules)
2. **Phase 4**: Kubernetes resource deployment
3. **Phase 5**: Application development
4. **Phase 6**: CI/CD pipeline setup

### Cost Estimation

Development environment approximate monthly costs:
- AKS cluster: $70-90
- PostgreSQL: $15-20
- Redis: $15
- Storage: $1-5
- Other services: $10-15
- **Total**: ~$110-145/month

Production costs will be higher based on:
- Instance sizes
- High availability
- Backup retention
- Data transfer

Use Azure Pricing Calculator for accurate estimates: https://azure.microsoft.com/pricing/calculator/
