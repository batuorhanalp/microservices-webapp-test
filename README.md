# Production Web App Project

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Architecture](https://img.shields.io/badge/architecture-microservices-blue)
![Services](https://img.shields.io/badge/microservices-4%20operational-success)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Kubernetes](https://img.shields.io/badge/kubernetes-ready-326ce5)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13+-336791)
![Multi-Cloud](https://img.shields.io/badge/multi--cloud-AWS%20%7C%20Azure%20%7C%20GCP-orange)
![License](https://img.shields.io/badge/license-MIT-green)
![Progress](https://img.shields.io/badge/progress-72%25-orange)

This project demonstrates building a **production-ready microservices web application** with modern cloud-native technologies and enterprise-grade architecture.

## Learning Path

### Phase 1: System Design ✅
- [x] Architecture documentation
- [x] Technology stack selection
- [x] Cloud provider resource mapping

### Phase 2: Azure Infrastructure ✅
- [x] Terraform infrastructure as code
- [x] Azure resource configurations (AKS, PostgreSQL, Redis, Storage, CDN, Event Hub)
- [x] Environment setup scripts and deployment automation
- [x] Multi-environment support (dev/staging/prod)
- [x] Security and monitoring setup

### Phase 3: Multi-Cloud Compatibility ✅
- [x] AWS EKS infrastructure module
- [x] GCP GKE infrastructure module  
- [x] Terraform for unified provisioning

### Phase 4: Kubernetes Configuration ✅
- [x] Complete Kubernetes manifests with Kustomize
- [x] .NET 8 backend deployment (migrated from Node.js)
- [x] External Secrets Operator integration
- [x] Multi-environment overlays (dev/staging/production)
- [x] Production-ready monitoring (Prometheus/Grafana)
- [x] Ingress with SSL termination and load balancing
- [x] Horizontal Pod Autoscaling and security policies

### Phase 5: Application Development 🚧
- [x] Microservices architecture setup
- [x] PostService (posts CRUD, validation)
- [x] LikeService (post interactions, user tracking)
- [x] CommentService (threaded comments, moderation)
- [x] UserService (authentication, user profiles)
- [ ] NotificationService (real-time notifications)
- [ ] Frontend React app
- [ ] WebSocket implementation
- [ ] Database models and migrations

### Phase 6: Local Development
- [ ] Docker Compose setup
- [ ] Local development environment
- [ ] Testing and debugging

### Phase 7: Cloud Deployments
- [ ] Azure deployment
- [ ] AWS deployment
- [ ] GCP deployment

## Project Structure

```
webapp-production/
├── docs/                    # Documentation
├── infrastructure/          # Infrastructure as Code
│   ├── azure/              # Azure ARM/Bicep templates
│   ├── aws/                # AWS CloudFormation
│   ├── gcp/                # GCP Deployment Manager
│   └── terraform/          # Cross-cloud Terraform
├── k8s/                    # Kubernetes manifests
├── src/                    # Application source code
│   ├── frontend/           # React application
│   ├── backend/            # .NET 8 API services
│   └── shared/             # Shared utilities
├── docker/                 # Docker configurations
├── scripts/               # Deployment and utility scripts
└── local/                 # Local development setup
```

## Quick Start

Each phase includes detailed instructions in its respective directory. Start with the system design document in `docs/` and follow the learning path above.

## Microservices Architecture Status

### ✅ **Operational Services**

| Service | Status | Health Check | API Endpoints | Database | Features |
|---------|--------|--------------|---------------|----------|---------|
| **PostService** | 🟢 Running | `/health` | `/api/posts/*` | PostgreSQL | CRUD operations, validation |
| **LikeService** | 🟢 Running | `/health` | `/api/likes/*` | PostgreSQL | Post interactions, user tracking |
| **CommentService** | 🟢 Running | `/health` | `/api/comments/*` | PostgreSQL | Threaded comments, moderation |
| **UserService** | 🟢 Running | `/health` | `/api/users/*` | PostgreSQL | Authentication, profiles |

### 📦 **Shared Components**
- **WebApp.Common**: Shared library with common models, DTOs, and utilities
- **Database Context**: Entity Framework Core with PostgreSQL
- **Health Checks**: Standardized health monitoring across all services
- **Dependency Injection**: Consistent service registration patterns

### 🔧 **Service Architecture**
Each microservice follows a clean architecture pattern:
```
services/{service-name}/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Models/              # Domain models
├── Program.cs           # Service configuration
└── Properties/          # Launch settings
```

## Key Features

### 🏗️ **Multi-Cloud Infrastructure**
- **Azure**: AKS, PostgreSQL, Redis, Blob Storage, CDN, Event Hub
- **AWS**: EKS, RDS, ElastiCache, S3, CloudFront, EventBridge
- **GCP**: GKE, Cloud SQL, Memorystore, Cloud Storage, Cloud CDN, Pub/Sub
- Unified Terraform modules for consistent provisioning

### 🔐 **Enterprise Security**
- External Secrets Operator integration
- Cloud-native secret managers (AWS Secrets Manager, GCP Secret Manager, Azure Key Vault)
- Zero hardcoded passwords in repository
- RBAC and network policies
- Secure container contexts

### ⚙️ **Production-Ready Kubernetes**
- Kustomize-based multi-environment deployments
- Horizontal Pod Autoscaling
- Health checks and readiness probes
- Resource limits and requests
- Monitoring with Prometheus and Grafana

### 🚀 **Modern Tech Stack**
- **Frontend**: React with NGINX
- **Backend**: .NET 8 API services
- **WebSocket**: Real-time communication
- **Authentication**: OAuth2/OIDC integration
- **Databases**: PostgreSQL with Redis caching

## Prerequisites

- **.NET 8 SDK**
- **Node.js 18+** (for frontend development)
- **Docker and Docker Compose**
- **kubectl** (Kubernetes CLI)
- **Cloud CLI tools**: `az`, `aws`, `gcloud`
- **Terraform** (for infrastructure provisioning)
- **Kustomize** (included with kubectl)

## Quick Deployment

### 1. Infrastructure Setup
```bash
# Deploy to AWS
cd infrastructure/terraform/environments/aws/dev
terraform init && terraform apply

# Deploy to GCP
cd infrastructure/terraform/environments/gcp/dev  
terraform init && terraform apply

# Deploy to Azure
cd infrastructure/terraform/environments/azure/dev
terraform init && terraform apply
```

### 2. Kubernetes Deployment
```bash
# Deploy to development environment
./scripts/k8s-deploy.sh apply dev

# Deploy to staging environment
./scripts/k8s-deploy.sh apply staging

# Deploy to production environment
./scripts/k8s-deploy.sh apply production
```

### 3. Verify Deployment
```bash
# Check application status
./scripts/k8s-deploy.sh status dev

# View logs
./scripts/k8s-deploy.sh logs dev backend
```

## Getting Started

1. **Review the architecture**: `docs/system-design.md`
2. **Understand security**: `docs/security-multi-cloud.md`  
3. **Deploy infrastructure**: Follow Terraform guides in `infrastructure/`
4. **Deploy applications**: Use Kubernetes deployment scripts
5. **Monitor and scale**: Access Grafana dashboards

## Documentation

- 📋 **[System Design](docs/system-design.md)** - Architecture and technology decisions
- 🏗️ **[Phase 3: Multi-Cloud](docs/phase-3-multi-cloud.md)** - Infrastructure setup
- ⚙️ **[Phase 4: Kubernetes](docs/phase4-kubernetes-dotnet.md)** - Container orchestration
- 🔐 **[Security Guide](docs/security-multi-cloud.md)** - Security best practices
- 📊 **[Monitoring](docs/monitoring-setup.md)** - Observability and alerting

## Project Progress: 72% Complete 🚀

| Phase | Status | Completion |
|-------|--------|------------|
| System Design | ✅ Complete | 100% |
| Azure Infrastructure | ✅ Complete | 100% |
| Multi-Cloud Setup | ✅ Complete | 100% |
| Kubernetes Config | ✅ Complete | 100% |
| Application Dev | 🚧 In Progress | 55% |
| └─ Microservices | ✅ 4/4 Services | 100% |
| └─ Frontend | ⏳ Planned | 0% |
| Local Development | ⏳ Planned | 0% |
| Cloud Deployments | ⏳ Planned | 0% |
