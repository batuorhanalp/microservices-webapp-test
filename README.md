# 🚀 Production-Ready Microservices Web Application

<div align="center">

![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge&logo=github)
![Architecture](https://img.shields.io/badge/architecture-microservices-blue?style=for-the-badge&logo=kubernetes)
![Services](https://img.shields.io/badge/microservices-8%20operational-success?style=for-the-badge&logo=docker)
![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=.net)
![Kubernetes](https://img.shields.io/badge/kubernetes-ready-326ce5?style=for-the-badge&logo=kubernetes)

![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13+-336791?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-cache-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![Multi-Cloud](https://img.shields.io/badge/multi--cloud-AWS%20%7C%20Azure%20%7C%20GCP-orange?style=for-the-badge&logo=cloud)
![Docker](https://img.shields.io/badge/Docker-containerized-2496ED?style=for-the-badge&logo=docker&logoColor=white)

![Terraform](https://img.shields.io/badge/Terraform-IaC-623CE4?style=for-the-badge&logo=terraform&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=for-the-badge&logo=github-actions&logoColor=white)
![Security](https://img.shields.io/badge/Security-Enterprise%20Grade-green?style=for-the-badge&logo=shield&logoColor=white)
![Monitoring](https://img.shields.io/badge/Monitoring-Prometheus%20%7C%20Grafana-E6522C?style=for-the-badge&logo=prometheus&logoColor=white)

![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)
![Progress](https://img.shields.io/badge/progress-85%25-brightgreen?style=for-the-badge)
![Stars](https://img.shields.io/github/stars/batuorhanalp/microservices-webapp-test?style=for-the-badge&logo=github)
![Forks](https://img.shields.io/github/forks/batuorhanalp/microservices-webapp-test?style=for-the-badge&logo=github)

</div>

## 📖 Project Overview

This is a **comprehensive, production-ready microservices web application** that demonstrates modern software engineering practices, cloud-native technologies, and enterprise-grade architecture patterns. The project serves as both a learning resource and a practical implementation of scalable, secure, and maintainable microservices architecture.

### 🎯 **What This Project Demonstrates**
- **Microservices Architecture**: Domain-driven design with clear service boundaries
- **Multi-Cloud Deployment**: Infrastructure-agnostic design supporting AWS, Azure, and GCP
- **DevOps Excellence**: CI/CD pipelines, Infrastructure as Code, and automated deployments
- **Production Readiness**: Security, monitoring, scaling, and operational excellence
- **Modern Tech Stack**: .NET 8, Kubernetes, PostgreSQL, Redis, React
- **Enterprise Patterns**: Clean architecture, CQRS, event sourcing, and distributed systems

### 🏢 **Target Audience**
- **Software Engineers** learning microservices and cloud-native development
- **DevOps Engineers** implementing infrastructure automation and deployment pipelines
- **Architects** designing scalable, distributed systems
- **Engineering Teams** adopting modern development practices
- **Students and Educators** exploring real-world software architecture

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

### Phase 5: Application Development ✅
- [x] Microservices architecture setup
- [x] AuthService (JWT authentication, user management)
- [x] UserService (user profiles, management)
- [x] PostService (posts CRUD, validation)
- [x] LikeService (post interactions, user tracking)
- [x] CommentService (threaded comments, moderation)
- [x] NotificationService (real-time notifications)
- [x] MediaUploadService (file upload, processing)
- [x] MediaProcessingService (media transformation)
- [x] API Gateway (request routing, authentication proxy)
- [x] Frontend React app (Next.js, TypeScript)
- [x] Database models and migrations
- [x] Comprehensive test coverage

### Phase 6: Local Development ✅
- [x] Docker Compose infrastructure setup
- [x] Local development environment
- [x] Development scripts and automation
- [x] Testing and debugging tools

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

| Service | Port | Status | Health Check | API Endpoints | Database | Features |
|---------|------|--------|--------------|---------------|----------|---------|
| **AuthService** | 7001 | 🟢 Running | `/health` | `/api/auth/*` | PostgreSQL | JWT authentication, user management |
| **UserService** | 7002 | 🟢 Running | `/health` | `/api/users/*` | PostgreSQL | User profiles, management |
| **PostService** | 7003 | 🟢 Running | `/health` | `/api/posts/*` | PostgreSQL | CRUD operations, validation |
| **LikeService** | 7004 | 🟢 Running | `/health` | `/api/likes/*` | PostgreSQL | Post interactions, user tracking |
| **CommentService** | 7005 | 🟢 Running | `/health` | `/api/comments/*` | PostgreSQL | Threaded comments, moderation |
| **NotificationService** | 7006 | 🟢 Running | `/health` | `/api/notifications/*` | PostgreSQL | Real-time notifications, alerts |
| **MediaUploadService** | 7007 | 🟢 Running | `/health` | `/api/media/upload/*` | MinIO/S3 | File uploads, validation |
| **MediaProcessingService** | 7008 | 🟢 Running | `/health` | `/api/media/process/*` | MinIO/S3 | Media transformation, optimization |
| **API Gateway** | 7009 | 🟢 Running | `/health` | Proxy to all services | - | Request routing, authentication proxy |
| **Frontend** | 3000 | 🟢 Running | `/` | Next.js app | - | React/TypeScript UI |

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

## 🛠️ Technology Stack

### **Backend Services**
| Technology | Version | Purpose | Status |
|------------|---------|---------|--------|
| **.NET** | 8.0 LTS | Microservices runtime | ✅ Production |
| **ASP.NET Core** | 8.0 | Web API framework | ✅ Production |
| **Entity Framework Core** | 8.0 | ORM and data access | ✅ Production |
| **PostgreSQL** | 13+ | Primary database | ✅ Production |
| **Redis** | 7.0+ | Caching and sessions | ✅ Production |

### **Frontend \u0026 UI**
| Technology | Version | Purpose | Status |
|------------|---------|---------|--------|
| **React** | 18+ | Frontend framework | ✅ Production |
| **Next.js** | 14+ | Full-stack React framework | ✅ Production |
| **TypeScript** | 5.0+ | Type-safe development | ✅ Production |
| **Tailwind CSS** | 3.0+ | Utility-first CSS framework | ✅ Production |
| **Zustand** | 4.0+ | State management | ✅ Production |
| **React Hook Form** | 7.0+ | Form handling | ✅ Production |
| **NGINX** | 1.24+ | Web server \u0026 reverse proxy | ✅ Ready |

### **Container & Orchestration**
| Technology | Version | Purpose | Status |
|------------|---------|---------|--------|
| **Docker** | 24.0+ | Containerization | ✅ Production |
| **Kubernetes** | 1.28+ | Container orchestration | ✅ Production |
| **Kustomize** | 5.0+ | Configuration management | ✅ Production |
| **Helm** | 3.12+ | Package management | 🚧 Planned |

### **Cloud Providers**
| Provider | Services | Status |
|----------|----------|--------|
| **AWS** | EKS, RDS, ElastiCache, S3, CloudFront, EventBridge | ✅ Ready |
| **Azure** | AKS, PostgreSQL, Redis, Blob Storage, CDN, Event Hub | ✅ Production |
| **GCP** | GKE, Cloud SQL, Memorystore, Cloud Storage, Cloud CDN, Pub/Sub | ✅ Ready |

### **Infrastructure & DevOps**
| Technology | Version | Purpose | Status |
|------------|---------|---------|--------|
| **Terraform** | 1.6+ | Infrastructure as Code | ✅ Production |
| **GitHub Actions** | - | CI/CD pipelines | ✅ Active |
| **Prometheus** | 2.45+ | Metrics collection | ✅ Production |
| **Grafana** | 10.0+ | Monitoring dashboards | ✅ Production |
| **External Secrets Operator** | 0.9+ | Secrets management | ✅ Production |

### **Security & Authentication**
| Technology | Purpose | Status |
|------------|---------|--------|
| **OAuth2/OIDC** | Authentication & authorization | 🚧 Planned |
| **JWT** | Token-based authentication | 🚧 Planned |
| **RBAC** | Role-based access control | ✅ Production |
| **Network Policies** | Network segmentation | ✅ Production |
| **TLS/SSL** | Encryption in transit | ✅ Production |

## Key Features

### 🏗️ **Multi-Cloud Infrastructure**
- **Azure**: AKS, PostgreSQL, Redis, Blob Storage, CDN, Event Hub
- **AWS**: EKS, RDS, ElastiCache, S3, CloudFront, EventBridge
- **GCP**: GKE, Cloud SQL, Memorystore, Cloud Storage, Cloud CDN, Pub/Sub
- Unified Terraform modules for consistent provisioning across all clouds
- Environment-specific configurations (dev, staging, production)

### 🔐 **Enterprise Security**
- **Zero-Trust Architecture**: Network policies and service mesh security
- **Secret Management**: External Secrets Operator with cloud-native secret stores
- **Authentication**: OAuth2/OIDC integration with enterprise identity providers
- **Authorization**: RBAC with fine-grained permissions
- **Encryption**: TLS everywhere, encrypted storage, and secure communication
- **Compliance**: Security best practices and automated security scanning

### ⚙️ **Production-Ready Kubernetes**
- **Multi-Environment**: Kustomize overlays for dev, staging, and production
- **Scaling**: Horizontal Pod Autoscaling and Cluster Autoscaling
- **Reliability**: Health checks, readiness probes, and circuit breakers
- **Resource Management**: CPU/memory limits, quality of service classes
- **Observability**: Comprehensive logging, metrics, and distributed tracing

### 🚀 **Developer Experience**
- **Local Development**: Docker Compose for easy local setup
- **Hot Reload**: Fast development cycles with live reloading
- **Testing**: Comprehensive test suites with automated testing
- **Documentation**: Extensive documentation and architectural decision records
- **CI/CD**: Automated build, test, and deployment pipelines

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

## 📁 Complete Documentation Index

### 🏠 **Architecture & Design**
- 📋 **[System Design](docs/system-design.md)** - Complete architecture overview and technology decisions
- 🛠️ **[Phase 2 Summary](docs/phase-2-summary.md)** - Azure infrastructure implementation summary
- 🌍 **[Phase 3: Multi-Cloud](docs/phase-3-multi-cloud.md)** - AWS, Azure, GCP infrastructure setup
- ⚙️ **[Phase 4: Kubernetes](docs/phase4-kubernetes-dotnet.md)** - Container orchestration and .NET 8 deployment

### 🔐 **Security & Compliance**
- 🔒 **[Security Multi-Cloud Guide](docs/security-multi-cloud.md)** - Comprehensive security best practices
- 🔑 **[Secrets Management](docs/security-secrets-management.md)** - External Secrets Operator and cloud secret stores
- ✅ **[Security Checklist](docs/SECURITY-CHECKLIST.md)** - Security implementation checklist
- 🚫 **[GitIgnore Security Summary](docs/GITIGNORE-SECURITY-SUMMARY.md)** - Repository security practices

### 📊 **Operations & Monitoring**
- 📊 **[Monitoring Setup](docs/monitoring-setup.md)** - Prometheus, Grafana, and observability
- 🚀 **[Kubernetes Deployment](docs/kubernetes-deployment.md)** - Deployment strategies and operations
- 🌿 **[Branch Protection](docs/BRANCH_PROTECTION.md)** - Git workflow and branch policies

### 📝 **Component Documentation**
- 🛜 **[Infrastructure Terraform](infrastructure/terraform/README.md)** - Terraform modules and deployment guide
- 💻 **[Backend Services](src/backend/README.md)** - .NET 8 microservices development guide
- 📦 **[GitHub Workflows](.github/README.md)** - CI/CD pipeline documentation

### 🎯 **Quick Access Links**
| Category | Document | Purpose |
|----------|----------|----------|
| **Start Here** | [System Design](docs/system-design.md) | Project overview and architecture |
| **Security** | [Security Guide](docs/security-multi-cloud.md) | Production security implementation |
| **Infrastructure** | [Multi-Cloud Setup](docs/phase-3-multi-cloud.md) | Deploy to AWS, Azure, GCP |
| **Development** | [Backend Guide](src/backend/README.md) | Microservices development |
| **Operations** | [Monitoring](docs/monitoring-setup.md) | Observability and alerting |
| **Deployment** | [Kubernetes Guide](docs/phase4-kubernetes-dotnet.md) | Container orchestration |

### 📚 **Learning Resources**

#### **🎨 For Developers**
1. Start with [System Design](docs/system-design.md) to understand the architecture
2. Review [Backend Development Guide](src/backend/README.md) for microservices patterns
3. Follow [Kubernetes Deployment](docs/kubernetes-deployment.md) for container orchestration
4. Implement [Security Best Practices](docs/security-multi-cloud.md)

#### **⚙️ For DevOps Engineers**
1. Begin with [Multi-Cloud Infrastructure](docs/phase-3-multi-cloud.md)
2. Configure [Terraform Modules](infrastructure/terraform/README.md)
3. Set up [Monitoring and Observability](docs/monitoring-setup.md)
4. Review [Security and Secrets Management](docs/security-secrets-management.md)

#### **📈 For Architects**
1. Study [System Design](docs/system-design.md) for architecture patterns
2. Review [Phase 2 Summary](docs/phase-2-summary.md) for implementation decisions
3. Analyze [Security Multi-Cloud Strategy](docs/security-multi-cloud.md)
4. Understand [Kubernetes Production Setup](docs/phase4-kubernetes-dotnet.md)

## Project Progress: 85% Complete 🚀

| Phase | Status | Completion |
|-------|--------|------------|
| System Design | ✅ Complete | 100% |
| Azure Infrastructure | ✅ Complete | 100% |
| Multi-Cloud Setup | ✅ Complete | 100% |
| Kubernetes Config | ✅ Complete | 100% |
| Application Dev | ✅ Complete | 100% |
| └─ Microservices | ✅ 8/8 Services | 100% |
| └─ Frontend | ✅ Complete | 100% |
| └─ API Gateway | ✅ Complete | 100% |
| Local Development | ✅ Complete | 100% |
| Cloud Deployments | ⏳ Planned | 0% |

---

## 🤝 Contributing

We welcome contributions to this project! This is a learning-focused repository that demonstrates production-ready practices.

### 🚀 **How to Contribute**

1. **Fork the Repository**
   ```bash
   git clone https://github.com/batuorhanalp/microservices-webapp-test.git
   cd microservices-webapp-test
   ```

2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-amazing-feature
   ```

3. **Follow Our Standards**
   - Write clean, well-documented code
   - Follow existing architectural patterns
   - Add tests for new functionality
   - Update documentation as needed

4. **Submit a Pull Request**
   - Ensure all tests pass
   - Include detailed description of changes
   - Reference any related issues

### 📝 **Contribution Areas**

| Area | Examples | Skill Level |
|------|----------|-------------|
| **Documentation** | Improve guides, fix typos | Beginner |
| **Frontend** | React components, UI/UX | Intermediate |
| **Microservices** | New services, API improvements | Intermediate |
| **Infrastructure** | Terraform modules, K8s configs | Advanced |
| **Security** | Security hardening, compliance | Advanced |
| **DevOps** | CI/CD improvements, monitoring | Advanced |

### 🔍 **Code Style**
- **C#**: Follow Microsoft coding standards
- **TypeScript/React**: Use ESLint and Prettier
- **Terraform**: Follow HashiCorp best practices
- **Documentation**: Use clear, concise language

## 🔗 Useful Links

### **🚀 Project Resources**
- [**Live Demo**](https://webapp-production.example.com) *(Coming Soon)*
- [**API Documentation**](https://api-docs.webapp-production.example.com) *(Coming Soon)*
- [**Monitoring Dashboard**](https://monitoring.webapp-production.example.com) *(Coming Soon)*

### **📚 Learning Resources**
- [Microservices Patterns by Chris Richardson](https://microservices.io/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Terraform Documentation](https://www.terraform.io/docs/)
- [Azure Well-Architected Framework](https://docs.microsoft.com/en-us/azure/architecture/)

### **🔧 Tools & Technologies**
- [Docker Hub - Official Images](https://hub.docker.com/)
- [Kubernetes Dashboard](https://kubernetes.io/docs/tasks/access-application-cluster/web-ui-dashboard/)
- [Prometheus Monitoring](https://prometheus.io/)
- [Grafana Dashboards](https://grafana.com/)

## ⚖️ License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 Production Web App Project

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

## 📧 Support & Contact

### **💬 Get Help**
- [**GitHub Issues**](https://github.com/batuorhanalp/microservices-webapp-test/issues) - Bug reports and feature requests
- [**GitHub Discussions**](https://github.com/batuorhanalp/microservices-webapp-test/discussions) - Questions and community support
- [**Wiki**](https://github.com/batuorhanalp/microservices-webapp-test/wiki) - Additional documentation and guides

### **🌐 Connect**
- **GitHub**: [@batuorhanalp](https://github.com/batuorhanalp)
- **LinkedIn**: [Connect for professional discussions](https://linkedin.com/in/batuorhanalp)
- **Email**: [Contact for collaboration opportunities](mailto:contact@webapp-production.example.com)

### **⭐ Show Your Support**
If this project helped you learn or build something amazing:

- Give it a ⭐ star on GitHub
- Share it with your network
- Contribute back to the community
- Write about your experience

---

<div align="center">

### Built with ❤️ by developers, for developers

**Happy coding! 🚀**

</div>
