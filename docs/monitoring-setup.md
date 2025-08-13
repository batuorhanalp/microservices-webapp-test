# Production Monitoring with Prometheus and Grafana

This guide covers the comprehensive monitoring stack deployed with your production web application.

## 🎯 What's Deployed

### **Prometheus Stack**
- **Prometheus Server**: Metrics collection and storage
- **Grafana**: Visualization and dashboards
- **AlertManager**: Alert routing and management
- **Node Exporter**: System-level metrics
- **Kube State Metrics**: Kubernetes cluster metrics
- **Service Monitors**: Custom application monitoring

### **Ingress Controller**
- **NGINX Ingress**: Load balancing and traffic routing
- **Metrics Integration**: Built-in Prometheus metrics

## 📊 Monitoring Architecture

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Your      │    │   NGINX     │    │ Prometheus  │
│   App       │───►│  Ingress    │───►│  Scraping   │
└─────────────┘    └─────────────┘    └─────────────┘
       │                                      │
       │           ┌─────────────┐           │
       └──────────►│   Service   │◄──────────┘
                   │   Monitor   │
                   └─────────────┘
                          │
                          ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Grafana    │    │ Prometheus  │    │ AlertManager│
│ Dashboards  │◄───│   Server    │───►│  Alerts     │
└─────────────┘    └─────────────┘    └─────────────┘
```

## 🚀 Quick Start

### 1. Deploy Infrastructure with Monitoring

```bash
cd webapp-production/infrastructure/terraform
terraform apply -var-file="environments/dev.tfvars"
```

### 2. Access Grafana

```bash
# Port forward to Grafana
kubectl port-forward -n monitoring svc/prometheus-operator-grafana 3000:80

# Open in browser: http://localhost:3000
# Username: admin
# Password: GrafanaAdmin123! (from tfvars)
```

### 3. Access Prometheus

```bash
# Port forward to Prometheus
kubectl port-forward -n monitoring svc/prometheus-operator-prometheus 9090:9090

# Open in browser: http://localhost:9090
```

### 4. Check Deployment Status

```bash
# Check all monitoring pods
kubectl get pods -n monitoring

# Check services
kubectl get svc -n monitoring

# Check ingress controller
kubectl get svc -n ingress-nginx
```

## 📈 Default Dashboards

Grafana comes with pre-configured dashboards:

### **Kubernetes Cluster Overview**
- Node resource usage (CPU, Memory, Disk)
- Pod status and resource consumption
- Network traffic and I/O metrics
- Storage utilization

### **Kubernetes Capacity Planning**
- Resource requests vs limits
- Node capacity planning
- Pod scheduling efficiency
- Resource recommendations

### **NGINX Ingress Controller**
- Request rate and response times
- Error rates and status codes
- Traffic distribution
- SSL certificate status

## 🔧 Custom Application Metrics

### Adding Metrics to Your App

Your application should expose metrics on `/metrics` endpoint:

```javascript
// Node.js example with prom-client
const prometheus = require('prom-client');

// Create custom metrics
const httpRequestsTotal = new prometheus.Counter({
  name: 'http_requests_total',
  help: 'Total number of HTTP requests',
  labelNames: ['method', 'status_code', 'route']
});

const httpRequestDuration = new prometheus.Histogram({
  name: 'http_request_duration_seconds',
  help: 'Duration of HTTP requests in seconds',
  labelNames: ['method', 'route'],
  buckets: [0.1, 0.5, 1, 2, 5]
});

// Metrics endpoint
app.get('/metrics', (req, res) => {
  res.set('Content-Type', prometheus.register.contentType);
  res.end(prometheus.register.metrics());
});
```

### Service Monitor Configuration

The ServiceMonitor is automatically created to scrape your application:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: webapp-prod-metrics
  namespace: monitoring
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: webapp-prod
      app.kubernetes.io/component: backend
  endpoints:
  - port: metrics
    path: /metrics
    interval: 30s
```

## 🚨 Alerting Rules

### Built-in Alerts

The stack includes default alerts for:
- **High CPU usage** (>80% for 5 minutes)
- **High memory usage** (>80% for 5 minutes)
- **Pod crash loops** (restarting frequently)
- **Node disk space** (<15% free)
- **Kubernetes API server** errors

### Custom Alerts Example

```yaml
# Create custom-alerts.yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: webapp-custom-alerts
  namespace: monitoring
spec:
  groups:
  - name: webapp.rules
    rules:
    - alert: HighErrorRate
      expr: rate(http_requests_total{status_code=~"5.."}[5m]) > 0.1
      for: 2m
      labels:
        severity: warning
      annotations:
        summary: "High error rate detected"
        description: "Error rate is above 10% for 2 minutes"
```

Apply with: `kubectl apply -f custom-alerts.yaml`

## 📊 Key Metrics to Monitor

### **Application Metrics**
- Request rate and latency
- Error rates by endpoint
- Database connection pool
- Cache hit/miss ratios
- Business-specific metrics

### **Infrastructure Metrics**
- CPU and memory usage
- Disk I/O and network traffic
- Kubernetes resource usage
- Container restarts and failures

### **Database Metrics**
- Query performance
- Connection counts
- Lock waits and deadlocks
- Storage usage

### **Redis Metrics**
- Memory usage
- Cache hit ratios
- Connection counts
- Key expiration rates

## 🔍 Useful Prometheus Queries

### **Basic System Metrics**
```promql
# CPU usage per node
100 - (avg by(instance) (irate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)

# Memory usage per pod
container_memory_usage_bytes / container_spec_memory_limit_bytes * 100

# Network traffic
rate(container_network_receive_bytes_total[5m])
```

### **Application Metrics**
```promql
# Request rate
rate(http_requests_total[5m])

# Error rate
rate(http_requests_total{status_code=~"5.."}[5m]) / rate(http_requests_total[5m])

# Response time percentiles
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

### **Kubernetes Metrics**
```promql
# Pod restarts
rate(kube_pod_container_status_restarts_total[5m]) > 0

# Node disk usage
(node_filesystem_size_bytes - node_filesystem_avail_bytes) / node_filesystem_size_bytes * 100
```

## 🎨 Creating Custom Dashboards

### 1. Access Grafana Dashboard Creation

1. Go to **Dashboards** → **New Dashboard**
2. Click **Add Query**
3. Select **Prometheus** as data source
4. Enter your PromQL query
5. Configure visualization options

### 2. Example Dashboard Panel

**HTTP Request Rate:**
```json
{
  "targets": [
    {
      "expr": "rate(http_requests_total[5m])",
      "legendFormat": "{{method}} {{route}}"
    }
  ],
  "title": "HTTP Request Rate",
  "type": "graph"
}
```

### 3. Dashboard Import

Import community dashboards:
1. Go to **Dashboards** → **Import**
2. Enter dashboard ID (e.g., `6417` for Kubernetes cluster monitoring)
3. Select Prometheus data source
4. Click **Import**

## 🔧 Troubleshooting

### **Prometheus Not Scraping Targets**

```bash
# Check ServiceMonitor
kubectl get servicemonitor -n monitoring

# Check Prometheus targets
kubectl port-forward -n monitoring svc/prometheus-operator-prometheus 9090:9090
# Visit http://localhost:9090/targets
```

### **Grafana Dashboard Issues**

```bash
# Check Grafana logs
kubectl logs -n monitoring -l app.kubernetes.io/name=grafana

# Reset admin password
kubectl get secret -n monitoring prometheus-operator-grafana -o jsonpath='{.data.admin-password}' | base64 --decode
```

### **High Resource Usage**

```bash
# Check resource consumption
kubectl top pods -n monitoring

# Reduce Prometheus retention
# Edit values in terraform/modules/k8s/main.tf
# retention = "15d"  # Reduce from 30d
```

## 📚 Advanced Configuration

### **External Storage for Prometheus**

For production, configure external storage:

```yaml
# In terraform/modules/k8s/main.tf
prometheus = {
  prometheusSpec = {
    storageSpec = {
      volumeClaimTemplate = {
        spec = {
          storageClassName = "premium-ssd"
          resources = {
            requests = {
              storage = "100Gi"  # Increase for production
            }
          }
        }
      }
    }
    retention = "90d"  # Longer retention for production
  }
}
```

### **High Availability Setup**

```yaml
prometheus = {
  prometheusSpec = {
    replicas = 2  # Enable HA
    shards = 2    # Horizontal sharding
  }
}
```

### **Remote Write Configuration**

For long-term storage:

```yaml
prometheus = {
  prometheusSpec = {
    remoteWrite = [
      {
        url = "https://your-remote-storage/write"
        basicAuth = {
          username = { name = "remote-storage-secret", key = "username" }
          password = { name = "remote-storage-secret", key = "password" }
        }
      }
    ]
  }
}
```

## 💡 Best Practices

### **Performance**
- Use recording rules for expensive queries
- Configure appropriate retention periods
- Monitor Prometheus resource usage
- Use federation for multi-cluster setups

### **Security**
- Enable authentication for Grafana
- Use RBAC for Prometheus access
- Secure metrics endpoints
- Regular backup of dashboards

### **Alerting**
- Create meaningful alert descriptions
- Set appropriate thresholds
- Avoid alert fatigue with proper grouping
- Test alert delivery channels

## 📋 Monitoring Checklist

- [ ] Prometheus collecting metrics from all targets
- [ ] Grafana dashboards displaying data correctly
- [ ] Alerts configured and tested
- [ ] Application metrics exposed on `/metrics`
- [ ] Storage configured for data retention
- [ ] Backup strategy for dashboards and alerts
- [ ] Documentation for team access and procedures

## 🔗 Resources

- **Prometheus Documentation**: https://prometheus.io/docs/
- **Grafana Documentation**: https://grafana.com/docs/
- **Kube-Prometheus-Stack**: https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack
- **Dashboard Library**: https://grafana.com/grafana/dashboards/
- **PromQL Tutorial**: https://prometheus.io/docs/prometheus/latest/querying/basics/

---

Your monitoring stack is now ready for production! 🚀 Access Grafana at http://localhost:3000 after port-forwarding to start exploring your infrastructure metrics.
