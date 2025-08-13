# Azure Key Vault CSI Driver for Kubernetes
# This enables secure mounting of Key Vault secrets into Kubernetes pods

# Azure Key Vault Provider for Secrets Store CSI Driver
resource "helm_release" "secrets_store_csi_driver" {
  count = var.cloud_provider == "azure" ? 1 : 0
  
  name       = "secrets-store-csi-driver"
  repository = "https://kubernetes-sigs.github.io/secrets-store-csi-driver/charts"
  chart      = "secrets-store-csi-driver"
  namespace  = "kube-system"
  version    = "~> 1.3"

  wait    = true
  timeout = 300

  values = [
    yamlencode({
      syncSecret = {
        enabled = true
      }
      enableSecretRotation = true
      rotationPollInterval = "2m"
    })
  ]
}

# Azure Key Vault Provider
resource "helm_release" "azure_keyvault_provider" {
  count = var.cloud_provider == "azure" ? 1 : 0
  
  name       = "csi-secrets-store-provider-azure"
  repository = "https://azure.github.io/secrets-store-csi-driver-provider-azure/charts"
  chart      = "csi-secrets-store-provider-azure"
  namespace  = "kube-system"
  version    = "~> 1.4"

  wait       = true
  timeout    = 300
  depends_on = [helm_release.secrets_store_csi_driver]

  values = [
    yamlencode({
      secrets-store-csi-driver = {
        syncSecret = {
          enabled = true
        }
        enableSecretRotation = true
      }
    })
  ]
}

# SecretProviderClass for application secrets
resource "kubernetes_manifest" "app_secret_provider_class" {
  count = var.cloud_provider == "azure" ? 1 : 0
  
  depends_on = [
    helm_release.azure_keyvault_provider,
    kubernetes_namespace.app
  ]

  manifest = {
    apiVersion = "secrets-store.csi.x-k8s.io/v1"
    kind       = "SecretProviderClass"
    metadata = {
      name      = "${var.app_name}-secrets"
      namespace = kubernetes_namespace.app.metadata[0].name
    }
    spec = {
      provider = "azure"
      secretObjects = [
        {
          secretName = "${var.app_name}-kv-secrets"
          type       = "Opaque"
          data = [
            {
              objectName = "database-admin-password"
              key        = "database-password"
            },
            {
              objectName = "database-connection-string"
              key        = "database-connection-string"
            },
            {
              objectName = "redis-connection-string"
              key        = "redis-connection-string"
            },
            {
              objectName = "redis-password"
              key        = "redis-password"
            },
            {
              objectName = "storage-account-key"
              key        = "storage-account-key"
            },
            {
              objectName = "container-registry-password"
              key        = "container-registry-password"
            },
            {
              objectName = "jwt-secret"
              key        = "jwt-secret"
            },
            {
              objectName = "app-encryption-key"
              key        = "encryption-key"
            },
            {
              objectName = "event-hub-connection-string"
              key        = "event-hub-connection-string"
            },
            {
              objectName = "app-insights-connection-string"
              key        = "app-insights-connection-string"
            },
            {
              objectName = "storage-connection-string"
              key        = "storage-connection-string"
            }
          ]
        }
      ]
      parameters = {
        usePodIdentity         = "false"
        useVMManagedIdentity   = "true"
        userAssignedIdentityID = var.aks_kubelet_identity_client_id
        keyvaultName          = var.key_vault_name
        tenantId              = var.azure_tenant_id
        objects = yamlencode([
          {
            objectName = "database-admin-password"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "database-connection-string"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "redis-connection-string"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "redis-password"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "storage-account-key"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "container-registry-password"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "jwt-secret"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "app-encryption-key"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "event-hub-connection-string"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "app-insights-connection-string"
            objectType = "secret"
            objectVersion = ""
          },
          {
            objectName = "storage-connection-string"
            objectType = "secret"
            objectVersion = ""
          }
        ])
      }
    }
  }
}

# SecretProviderClass for Grafana secrets
resource "kubernetes_manifest" "grafana_secret_provider_class" {
  count = var.cloud_provider == "azure" ? 1 : 0
  
  depends_on = [
    helm_release.azure_keyvault_provider,
    kubernetes_namespace.monitoring
  ]

  manifest = {
    apiVersion = "secrets-store.csi.x-k8s.io/v1"
    kind       = "SecretProviderClass"
    metadata = {
      name      = "grafana-secrets"
      namespace = kubernetes_namespace.monitoring.metadata[0].name
    }
    spec = {
      provider = "azure"
      secretObjects = [
        {
          secretName = "grafana-kv-secrets"
          type       = "Opaque"
          data = [
            {
              objectName = "grafana-admin-password"
              key        = "admin-password"
            }
          ]
        }
      ]
      parameters = {
        usePodIdentity         = "false"
        useVMManagedIdentity   = "true"
        userAssignedIdentityID = var.aks_kubelet_identity_client_id
        keyvaultName          = var.key_vault_name
        tenantId              = var.azure_tenant_id
        objects = yamlencode([
          {
            objectName = "grafana-admin-password"
            objectType = "secret"
            objectVersion = ""
          }
        ])
      }
    }
  }
}

# Example deployment showing how to use Key Vault secrets
resource "kubernetes_manifest" "example_secret_consumer" {
  count = var.create_example_secret_consumer ? 1 : 0
  
  depends_on = [kubernetes_manifest.app_secret_provider_class]

  manifest = {
    apiVersion = "apps/v1"
    kind       = "Deployment"
    metadata = {
      name      = "secret-consumer-example"
      namespace = kubernetes_namespace.app.metadata[0].name
      labels = {
        app = "secret-consumer-example"
      }
    }
    spec = {
      replicas = 1
      selector = {
        matchLabels = {
          app = "secret-consumer-example"
        }
      }
      template = {
        metadata = {
          labels = {
            app = "secret-consumer-example"
          }
        }
        spec = {
          serviceAccountName = "default"
          containers = [
            {
              name  = "app"
              image = "nginx:alpine"
              volumeMounts = [
                {
                  name      = "secrets-store"
                  mountPath = "/mnt/secrets"
                  readOnly  = true
                }
              ]
              env = [
                {
                  name = "DATABASE_PASSWORD"
                  valueFrom = {
                    secretKeyRef = {
                      name = "${var.app_name}-kv-secrets"
                      key  = "database-password"
                    }
                  }
                },
                {
                  name = "JWT_SECRET"
                  valueFrom = {
                    secretKeyRef = {
                      name = "${var.app_name}-kv-secrets"
                      key  = "jwt-secret"
                    }
                  }
                }
              ]
            }
          ]
          volumes = [
            {
              name = "secrets-store"
              csi = {
                driver   = "secrets-store.csi.k8s.io"
                readOnly = true
                volumeAttributes = {
                  secretProviderClass = "${var.app_name}-secrets"
                }
              }
            }
          ]
        }
      }
    }
  }
}
