# Kubernetes Resources Module
# Deploys Prometheus, Grafana, and other K8s resources after cluster creation

terraform {
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~>2.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~>2.0"
    }
  }
}

# Configure Kubernetes provider
provider "kubernetes" {
  host                   = var.cluster_endpoint
  cluster_ca_certificate = var.cluster_ca_certificate
  token                  = var.cluster_token
  
  # For Azure AKS, AWS EKS, and GCP GKE authentication
  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = var.kubectl_command
    args        = var.kubectl_args
  }
}

# Configure Helm provider
provider "helm" {
  kubernetes {
    host                   = var.cluster_endpoint
    cluster_ca_certificate = var.cluster_ca_certificate
    token                  = var.cluster_token
    
    exec {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = var.kubectl_command
      args        = var.kubectl_args
    }
  }
}

# === NAMESPACES ===

# Application namespace
resource "kubernetes_namespace" "app" {
  metadata {
    name = var.namespace
    labels = {
      "app.kubernetes.io/name"     = var.app_name
      "app.kubernetes.io/instance" = var.environment
      "app.kubernetes.io/part-of"  = "webapp-production"
    }
  }
}

# Monitoring namespace
resource "kubernetes_namespace" "monitoring" {
  metadata {
    name = "monitoring"
    labels = {
      "name" = "monitoring"
      "app.kubernetes.io/name" = "monitoring-stack"
      "app.kubernetes.io/part-of" = "webapp-production"
    }
  }
}

# Ingress namespace
resource "kubernetes_namespace" "ingress" {
  metadata {
    name = "ingress-nginx"
    labels = {
      "name" = "ingress-nginx"
      "app.kubernetes.io/name" = "ingress-nginx"
    }
  }
}

# === MONITORING STACK ===

# Add Prometheus Helm repository
resource "helm_release" "prometheus_operator" {
  name       = "prometheus-operator"
  repository = "https://prometheus-community.github.io/helm-charts"
  chart      = "kube-prometheus-stack"
  namespace  = kubernetes_namespace.monitoring.metadata[0].name
  version    = "~> 55.0"

  # Wait for CRDs to be established
  wait             = true
  timeout          = 600
  create_namespace = false

  values = [
    yamlencode({
      # Global configuration
      global = {
        imageRegistry = ""
      }

      # Prometheus configuration
      prometheus = {
        prometheusSpec = {
          storageSpec = {
            volumeClaimTemplate = {
              spec = {
                storageClassName = var.storage_class
                accessModes      = ["ReadWriteOnce"]
                resources = {
                  requests = {
                    storage = "20Gi"
                  }
                }
              }
            }
          }
          retention = "30d"
          scrapeInterval = "30s"
          evaluationInterval = "30s"
          
          # Resource requests and limits
          resources = {
            requests = {
              memory = "1Gi"
              cpu    = "500m"
            }
            limits = {
              memory = "2Gi"
              cpu    = "1"
            }
          }
          
          # Service Monitor selectors
          serviceMonitorSelectorNilUsesHelmValues = false
          podMonitorSelectorNilUsesHelmValues     = false
          ruleSelectorNilUsesHelmValues           = false
        }
        
        # Prometheus service configuration
        service = {
          type = "ClusterIP"
          port = 9090
        }
      }

      # Grafana configuration
      grafana = {
        enabled = true
        
        # Admin credentials
        adminPassword = var.grafana_admin_password
        
        # Persistence
        persistence = {
          enabled          = true
          storageClassName = var.storage_class
          size             = "10Gi"
          accessModes      = ["ReadWriteOnce"]
        }
        
        # Resource configuration
        resources = {
          requests = {
            memory = "256Mi"
            cpu    = "100m"
          }
          limits = {
            memory = "512Mi"
            cpu    = "500m"
          }
        }
        
        # Service configuration
        service = {
          type = "LoadBalancer"
          port = 80
          annotations = var.cloud_provider == "azure" ? {
            "service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path" = "/api/health"
          } : var.cloud_provider == "aws" ? {
            "service.beta.kubernetes.io/aws-load-balancer-type" = "nlb"
          } : {}
        }
        
        # Grafana configuration
        "grafana.ini" = {
          server = {
            root_url = "http://localhost:3000"
          }
          security = {
            allow_embedding = true
          }
          "auth.anonymous" = {
            enabled = false
          }
        }
        
        # Default dashboards
        defaultDashboardsEnabled = true
        
        # Additional dashboards
        dashboardProviders = {
          "dashboardproviders.yaml" = {
            apiVersion = 1
            providers = [
              {
                name = "webapp-dashboards"
                orgId = 1
                folder = "WebApp"
                type = "file"
                disableDeletion = false
                editable = true
                options = {
                  path = "/var/lib/grafana/dashboards/webapp"
                }
              }
            ]
          }
        }
        
        # Custom dashboards
        dashboards = {
          webapp-dashboards = {
            "webapp-overview" = {
              gnetId = 6417  # Kubernetes cluster monitoring dashboard
              revision = 1
              datasource = "Prometheus"
            }
            "webapp-performance" = {
              gnetId = 315   # Kubernetes capacity planning dashboard
              revision = 3
              datasource = "Prometheus"
            }
          }
        }
      }

      # AlertManager configuration
      alertmanager = {
        alertmanagerSpec = {
          storage = {
            volumeClaimTemplate = {
              spec = {
                storageClassName = var.storage_class
                accessModes      = ["ReadWriteOnce"]
                resources = {
                  requests = {
                    storage = "5Gi"
                  }
                }
              }
            }
          }
          
          resources = {
            requests = {
              memory = "128Mi"
              cpu    = "100m"
            }
            limits = {
              memory = "256Mi"
              cpu    = "200m"
            }
          }
        }
        
        service = {
          type = "ClusterIP"
          port = 9093
        }
      }

      # Node Exporter configuration
      nodeExporter = {
        enabled = true
      }

      # Kube State Metrics configuration
      kubeStateMetrics = {
        enabled = true
      }

      # Prometheus Node Exporter
      "prometheus-node-exporter" = {
        enabled = true
        hostRootFsMount = {
          enabled = true
          mountPropagation = "HostToContainer"
        }
      }
    })
  ]

  depends_on = [kubernetes_namespace.monitoring]
}

# === INGRESS CONTROLLER ===

# NGINX Ingress Controller
resource "helm_release" "nginx_ingress" {
  name       = "ingress-nginx"
  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"
  namespace  = kubernetes_namespace.ingress.metadata[0].name
  version    = "~> 4.8"

  wait    = true
  timeout = 300

  values = [
    yamlencode({
      controller = {
        replicaCount = 2
        
        resources = {
          requests = {
            cpu    = "100m"
            memory = "128Mi"
          }
          limits = {
            cpu    = "500m"
            memory = "512Mi"
          }
        }
        
        service = {
          type = "LoadBalancer"
          annotations = var.cloud_provider == "azure" ? {
            "service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path" = "/healthz"
          } : var.cloud_provider == "aws" ? {
            "service.beta.kubernetes.io/aws-load-balancer-type" = "nlb"
          } : {}
        }
        
        metrics = {
          enabled = true
          serviceMonitor = {
            enabled = true
            additionalLabels = {
              release = "prometheus-operator"
            }
          }
        }
        
        # Enable Prometheus metrics
        podAnnotations = {
          "prometheus.io/scrape" = "true"
          "prometheus.io/port"   = "10254"
        }
      }
    })
  ]

  depends_on = [
    kubernetes_namespace.ingress,
    helm_release.prometheus_operator
  ]
}

# === APPLICATION RESOURCES ===

# ConfigMap for application configuration
resource "kubernetes_config_map" "app_config" {
  metadata {
    name      = "${var.app_name}-config"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  data = {
    NODE_ENV               = var.environment
    LOG_LEVEL             = var.log_level
    METRICS_ENABLED       = "true"
    PROMETHEUS_PORT       = "9090"
    REDIS_URL             = var.redis_connection_string
    DATABASE_URL          = var.database_connection_string
    STORAGE_ACCOUNT_URL   = var.storage_account_url
    CDN_URL               = var.cdn_url
    EVENT_HUB_CONNECTION  = var.event_hub_connection_string
  }
}

# Secret for sensitive configuration
resource "kubernetes_secret" "app_secrets" {
  metadata {
    name      = "${var.app_name}-secrets"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  type = "Opaque"

  data = {
    database-password           = base64encode(var.database_password)
    redis-password             = base64encode(var.redis_password)
    storage-account-key        = base64encode(var.storage_account_key)
    container-registry-password = base64encode(var.container_registry_password)
    jwt-secret                 = base64encode(var.jwt_secret)
    event-hub-key              = base64encode(var.event_hub_key)
  }
}

# === SERVICE MONITORS ===

# Custom ServiceMonitor for application metrics
resource "kubernetes_manifest" "app_service_monitor" {
  manifest = {
    apiVersion = "monitoring.coreos.com/v1"
    kind       = "ServiceMonitor"
    metadata = {
      name      = "${var.app_name}-metrics"
      namespace = kubernetes_namespace.monitoring.metadata[0].name
      labels = {
        app     = var.app_name
        release = "prometheus-operator"
      }
    }
    spec = {
      selector = {
        matchLabels = {
          "app.kubernetes.io/name" = var.app_name
          "app.kubernetes.io/component" = "backend"
        }
      }
      namespaceSelector = {
        matchNames = [kubernetes_namespace.app.metadata[0].name]
      }
      endpoints = [
        {
          port     = "metrics"
          path     = "/metrics"
          interval = "30s"
        }
      ]
    }
  }

  depends_on = [helm_release.prometheus_operator]
}

# === PERSISTENT VOLUMES ===

# Storage class for cloud provider
resource "kubernetes_storage_class" "app_storage" {
  count = var.create_storage_class ? 1 : 0
  
  metadata {
    name = "${var.app_name}-storage"
    annotations = {
      "storageclass.kubernetes.io/is-default-class" = "false"
    }
  }

  storage_provisioner = var.cloud_provider == "azure" ? "disk.csi.azure.com" : (
    var.cloud_provider == "aws" ? "ebs.csi.aws.com" : 
    "pd.csi.storage.gke.io"
  )
  
  reclaim_policy         = "Retain"
  allow_volume_expansion = true
  volume_binding_mode    = "WaitForFirstConsumer"

  parameters = var.cloud_provider == "azure" ? {
    skuName = "Premium_LRS"
    kind    = "Managed"
  } : var.cloud_provider == "aws" ? {
    type      = "gp3"
    encrypted = "true"
  } : {
    type             = "pd-ssd"
    replication-type = "none"
  }
}

# === NETWORK POLICIES ===

# Network policy for monitoring namespace
resource "kubernetes_network_policy" "monitoring_policy" {
  count = var.enable_network_policies ? 1 : 0

  metadata {
    name      = "monitoring-network-policy"
    namespace = kubernetes_namespace.monitoring.metadata[0].name
  }

  spec {
    pod_selector {
      match_labels = {}
    }

    policy_types = ["Ingress", "Egress"]

    ingress {
      from {
        namespace_selector {
          match_labels = {
            name = kubernetes_namespace.app.metadata[0].name
          }
        }
      }
      
      from {
        namespace_selector {
          match_labels = {
            name = kubernetes_namespace.monitoring.metadata[0].name
          }
        }
      }

      ports {
        protocol = "TCP"
        port     = "9090"
      }
      ports {
        protocol = "TCP"
        port     = "3000"
      }
      ports {
        protocol = "TCP"
        port     = "9093"
      }
    }

    egress {}
  }
}

# === RESOURCE QUOTAS ===

# Resource quota for application namespace
resource "kubernetes_resource_quota" "app_quota" {
  metadata {
    name      = "${var.app_name}-quota"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  spec {
    hard = {
      "requests.cpu"    = "4"
      "requests.memory" = "8Gi"
      "limits.cpu"      = "8"
      "limits.memory"   = "16Gi"
      "persistentvolumeclaims" = "10"
      "services"        = "10"
      "secrets"         = "10"
      "configmaps"      = "10"
    }
  }
}
