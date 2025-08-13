# Kubernetes Deployment Guide

This document describes the Kubernetes configuration for the production-ready webapp infrastructure.

## Architecture Overview

The webapp is deployed using a microservices architecture with the following components:

### Core Services
- **Frontend**: React application served by NGINX
- **Backend API**: Node.js/Express REST API service
- **WebSocket Service**: Real-time Socket.io service
- **Authentication**: JWT-based authentication service

### Infrastructure Services
- **Ingress Controller**: NGINX ingress with SSL termination
- **Monitoring**: Prometheus + Grafana stack
- **Secrets Management**: Kubernetes secrets with cloud integration

## Directory Structure

```
k8s/
├── base/                     # Base Kubernetes manifests
│   ├── kustomization.yaml   # Base kustomization
│   ├── namespace.yaml       # Namespace definitions
│   ├── configmap.yaml      # Application configuration
│   └── secrets.yaml        # Secret templates
├── components/              # Individual service manifests
│   ├── frontend/           # Frontend service
│   ├── backend/           # Backend API service
│   ├── websocket/         # WebSocket service
│   ├── auth/              # Authentication service
│   ├── ingress/           # Ingress controller
│   └── monitoring/        # Monitoring stack
└── overlays/               # Environment-specific configurations
    ├── dev/               # Development environment
    ├── staging/           # Staging environment
    └── production/        # Production environment
```

## Prerequisites

### Required Tools
- `kubectl` CLI tool
- `kustomize` (built into kubectl 1.14+)
- Access to Kubernetes cluster
- Container images built and pushed to registry

### Infrastructure Requirements
- Kubernetes cluster (1.24+)
- Ingress controller (NGINX recommended)
- Persistent storage (for monitoring data)
- DNS configuration for domains
- SSL certificates or cert-manager
- Container registry access

### Cloud Provider Setup

#### AWS (EKS)
```bash
# Install AWS Load Balancer Controller
kubectl apply --validate=false -f https://github.com/jetstack/cert-manager/releases/download/v1.8.0/cert-manager.yaml
helm repo add eks https://aws.github.io/eks-charts
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=webapp-production \
  --set serviceAccount.create=false \
  --set serviceAccount.name=aws-load-balancer-controller
```

#### GCP (GKE)
```bash
# Enable required APIs
gcloud services enable container.googleapis.com
gcloud services enable compute.googleapis.com
gcloud services enable dns.googleapis.com

# Configure GKE cluster
gcloud container clusters get-credentials webapp-production --zone=us-central1-a
```

#### Azure (AKS)
```bash
# Install Application Gateway Ingress Controller
az aks enable-addons --resource-group webapp-prod-rg --name webapp-production --addons ingress-appgw
```

## Configuration

### Environment Variables

Each environment uses different configurations through Kustomize overlays:

#### Development
```yaml
- ENV=development
- LOG_LEVEL=debug
- DEBUG=true
- CORS_ORIGIN=http://localhost:3000
- RATE_LIMIT_MAX=100
```

#### Staging
```yaml
- ENV=staging
- LOG_LEVEL=info
- DEBUG=false
- CORS_ORIGIN=https://staging.webapp.example.com
- RATE_LIMIT_MAX=500
```

#### Production
```yaml
- ENV=production
- LOG_LEVEL=warn
- DEBUG=false
- CORS_ORIGIN=https://webapp.example.com
- RATE_LIMIT_MAX=1000
```

### Secrets Management

Secrets should be managed using one of these methods:

1. **External Secrets Operator** (Recommended)
2. **Cloud Provider Secret Managers**
3. **HashiCorp Vault**
4. **Manual kubectl secrets**

#### Using External Secrets Operator

```bash
# Install External Secrets Operator
helm repo add external-secrets https://charts.external-secrets.io
helm install external-secrets external-secrets/external-secrets -n external-secrets-system --create-namespace
```

Example SecretStore for AWS:
```yaml
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: webapp-secretstore
spec:
  provider:
    aws:
      service: SecretsManager
      region: us-west-2
      auth:
        jwt:
          serviceAccountRef:
            name: webapp-external-secrets-sa
```

### SSL/TLS Certificates

#### Using cert-manager (Recommended)

```bash
# Install cert-manager
kubectl apply --validate=false -f https://github.com/jetstack/cert-manager/releases/download/v1.8.0/cert-manager.yaml

# Create ClusterIssuer for Let's Encrypt
kubectl apply -f - <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@webapp.example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

## Deployment Instructions

### 1. Build and Push Images

```bash
# Build frontend image
docker build -t your-registry/webapp-frontend:v1.2.3 ./frontend
docker push your-registry/webapp-frontend:v1.2.3

# Build backend image
docker build -t your-registry/webapp-api:v1.2.3 ./backend
docker push your-registry/webapp-api:v1.2.3

# Build websocket image
docker build -t your-registry/webapp-websocket:v1.2.3 ./websocket
docker push your-registry/webapp-websocket:v1.2.3

# Build auth image
docker build -t your-registry/webapp-auth:v1.2.3 ./auth
docker push your-registry/webapp-auth:v1.2.3
```

### 2. Configure Secrets

Create production secrets (never commit real secrets to git):

```bash
# Create namespace
kubectl create namespace webapp-production

# Create main secrets
kubectl create secret generic webapp-secrets \
  --from-literal=db_user="webapp_user" \
  --from-literal=db_password="$(openssl rand -base64 32)" \
  --from-literal=redis_password="$(openssl rand -base64 32)" \
  --from-literal=jwt_secret="$(openssl rand -base64 32)" \
  -n webapp-production

# Create OAuth secrets
kubectl create secret generic webapp-oauth-secrets \
  --from-literal=google_client_id="your-google-client-id" \
  --from-literal=google_client_secret="your-google-client-secret" \
  -n webapp-production

# Create Grafana secrets
kubectl create secret generic grafana-secrets \
  --from-literal=admin_password="$(openssl rand -base64 32)" \
  --from-literal=secret_key="$(openssl rand -base64 32)" \
  -n webapp-production
```

### 3. Deploy to Environment

#### Development
```bash
kubectl apply -k k8s/overlays/dev/
```

#### Staging
```bash
kubectl apply -k k8s/overlays/staging/
```

#### Production
```bash
# Verify configuration first
kubectl kustomize k8s/overlays/production/ | kubectl apply --dry-run=client -f -

# Deploy to production
kubectl apply -k k8s/overlays/production/
```

### 4. Verify Deployment

```bash
# Check pod status
kubectl get pods -n webapp-production

# Check services
kubectl get services -n webapp-production

# Check ingress
kubectl get ingress -n webapp-production

# View logs
kubectl logs -f deployment/webapp-frontend-deployment -n webapp-production
kubectl logs -f deployment/webapp-api-deployment -n webapp-production
```

### 5. Configure DNS

Update your DNS settings to point domains to the ingress controller:

```bash
# Get ingress IP/hostname
kubectl get ingress webapp-ingress -n webapp-production

# Configure DNS records
webapp.example.com         -> <INGRESS_IP>
api.webapp.example.com     -> <INGRESS_IP>
ws.webapp.example.com      -> <INGRESS_IP>
grafana.webapp.example.com -> <INGRESS_IP>
```

## Monitoring and Observability

### Accessing Monitoring

- **Prometheus**: https://prometheus.webapp.example.com
- **Grafana**: https://grafana.webapp.example.com
  - Username: admin
  - Password: (from grafana-secrets)

### Available Dashboards

1. **WebApp Overview**: Application metrics and health
2. **Infrastructure**: Node and cluster metrics
3. **Ingress**: HTTP request metrics and errors
4. **Database**: Database connection and query metrics

### Alerts Configuration

Key alerts configured:
- High CPU/Memory usage
- Pod crash looping
- Service availability
- High HTTP error rates
- High response times

## Scaling

### Manual Scaling

```bash
# Scale frontend deployment
kubectl scale deployment webapp-frontend-deployment --replicas=10 -n webapp-production

# Scale all deployments
kubectl scale deployment --all --replicas=5 -n webapp-production
```

### Auto Scaling

HPA (Horizontal Pod Autoscaler) is configured for:
- CPU utilization (70%)
- Memory utilization (80%)
- HTTP requests per second (100 req/s)

VPA (Vertical Pod Autoscaler) can be configured for resource optimization.

## Security Considerations

### Network Policies
- All services have restrictive network policies
- Only required communication is allowed
- Monitoring traffic is isolated

### Security Contexts
- All containers run as non-root users
- Read-only root filesystems where possible
- Security capabilities are dropped

### RBAC
- Least-privilege service accounts
- Role-based access control for all components
- Monitoring has limited cluster access

## Troubleshooting

### Common Issues

#### Pod Not Starting
```bash
kubectl describe pod <pod-name> -n webapp-production
kubectl logs <pod-name> -n webapp-production --previous
```

#### Service Not Accessible
```bash
kubectl get endpoints -n webapp-production
kubectl describe service <service-name> -n webapp-production
```

#### Ingress Issues
```bash
kubectl describe ingress webapp-ingress -n webapp-production
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller
```

#### Database Connection Issues
```bash
# Check if secrets exist
kubectl get secrets -n webapp-production

# Test database connectivity
kubectl run -it --rm debug --image=postgres:13 --restart=Never -- psql -h <db-host> -U webapp_user webapp_production
```

### Health Checks

All services provide health check endpoints:
- `/health` - Liveness probe
- `/ready` - Readiness probe  
- `/startup` - Startup probe
- `/metrics` - Prometheus metrics

### Log Aggregation

For production environments, consider deploying:
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Fluentd** or **Fluent Bit** for log forwarding
- **Grafana Loki** for lightweight log aggregation

## Maintenance

### Updates and Rollbacks

```bash
# Update image version
kubectl set image deployment/webapp-frontend-deployment webapp-frontend=your-registry/webapp-frontend:v1.2.4 -n webapp-production

# Check rollout status
kubectl rollout status deployment/webapp-frontend-deployment -n webapp-production

# Rollback if needed
kubectl rollout undo deployment/webapp-frontend-deployment -n webapp-production
```

### Backup and Disaster Recovery

1. **Database Backups**: Configure automated backups
2. **Persistent Volume Snapshots**: For monitoring data
3. **Configuration Backup**: Keep infrastructure as code in git
4. **Secret Backup**: Backup secrets to secure storage

### Resource Management

Monitor and optimize:
- Resource requests and limits
- Persistent volume usage
- Network bandwidth
- Ingress rate limiting

## Cost Optimization

- Use appropriate resource requests/limits
- Configure Pod Disruption Budgets for availability
- Use node selectors for workload placement
- Monitor unused resources
- Configure appropriate storage classes
- Use spot instances where possible

## Support and Documentation

- **Application Logs**: Available in Grafana Loki
- **Metrics**: Available in Prometheus/Grafana
- **Alerts**: Configured for Slack/PagerDuty
- **Documentation**: This guide and inline comments
- **Source Code**: Version controlled with deployment configs
