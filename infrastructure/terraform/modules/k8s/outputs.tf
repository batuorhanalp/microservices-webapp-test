# Outputs for Kubernetes resources module

# === Namespaces ===
output "app_namespace" {
  description = "Application namespace name"
  value       = kubernetes_namespace.app.metadata[0].name
}

output "monitoring_namespace" {
  description = "Monitoring namespace name"
  value       = kubernetes_namespace.monitoring.metadata[0].name
}

output "ingress_namespace" {
  description = "Ingress namespace name"
  value       = kubernetes_namespace.ingress.metadata[0].name
}

# === Monitoring Stack ===
output "prometheus_service_name" {
  description = "Prometheus service name"
  value       = "${helm_release.prometheus_operator.name}-prometheus"
}

output "grafana_service_name" {
  description = "Grafana service name"  
  value       = "${helm_release.prometheus_operator.name}-grafana"
}

output "grafana_admin_password" {
  description = "Grafana admin password"
  value       = var.grafana_admin_password
  sensitive   = true
}

output "alertmanager_service_name" {
  description = "AlertManager service name"
  value       = "${helm_release.prometheus_operator.name}-alertmanager"
}

# === Monitoring URLs ===
output "monitoring_urls" {
  description = "URLs for accessing monitoring services"
  value = {
    prometheus_port_forward   = "kubectl port-forward -n ${kubernetes_namespace.monitoring.metadata[0].name} svc/${helm_release.prometheus_operator.name}-prometheus 9090:9090"
    grafana_port_forward     = "kubectl port-forward -n ${kubernetes_namespace.monitoring.metadata[0].name} svc/${helm_release.prometheus_operator.name}-grafana 3000:80"
    alertmanager_port_forward = "kubectl port-forward -n ${kubernetes_namespace.monitoring.metadata[0].name} svc/${helm_release.prometheus_operator.name}-alertmanager 9093:9093"
  }
}

# === Ingress Controller ===
output "ingress_controller_name" {
  description = "NGINX Ingress Controller name"
  value       = helm_release.nginx_ingress.name
}

output "ingress_controller_service" {
  description = "NGINX Ingress Controller service name"
  value       = "${helm_release.nginx_ingress.name}-controller"
}

# === Application Resources ===
output "app_config_map_name" {
  description = "Application ConfigMap name"
  value       = kubernetes_config_map.app_config.metadata[0].name
}

output "app_secret_name" {
  description = "Application Secret name"
  value       = kubernetes_secret.app_secrets.metadata[0].name
}

# === Storage ===
output "storage_class_name" {
  description = "Custom storage class name"
  value       = var.create_storage_class ? kubernetes_storage_class.app_storage[0].metadata[0].name : var.storage_class
}

# === Service Monitor ===
output "app_service_monitor_name" {
  description = "Application ServiceMonitor name"
  value       = kubernetes_manifest.app_service_monitor.manifest.metadata.name
}

# === Resource Quota ===
output "app_resource_quota_name" {
  description = "Application resource quota name"
  value       = kubernetes_resource_quota.app_quota.metadata[0].name
}

# === Connection Commands ===
output "kubectl_commands" {
  description = "Useful kubectl commands"
  value = {
    get_pods_all          = "kubectl get pods -A"
    get_services_all      = "kubectl get svc -A"
    get_monitoring_pods   = "kubectl get pods -n ${kubernetes_namespace.monitoring.metadata[0].name}"
    get_app_pods         = "kubectl get pods -n ${kubernetes_namespace.app.metadata[0].name}"
    
    # Monitoring access
    port_forward_grafana     = "kubectl port-forward -n ${kubernetes_namespace.monitoring.metadata[0].name} svc/${helm_release.prometheus_operator.name}-grafana 3000:80"
    port_forward_prometheus  = "kubectl port-forward -n ${kubernetes_namespace.monitoring.metadata[0].name} svc/${helm_release.prometheus_operator.name}-prometheus 9090:9090"
    
    # Logs
    logs_prometheus = "kubectl logs -n ${kubernetes_namespace.monitoring.metadata[0].name} -l app.kubernetes.io/name=prometheus"
    logs_grafana   = "kubectl logs -n ${kubernetes_namespace.monitoring.metadata[0].name} -l app.kubernetes.io/name=grafana"
    
    # Configuration
    get_configmaps = "kubectl get configmaps -n ${kubernetes_namespace.app.metadata[0].name}"
    get_secrets   = "kubectl get secrets -n ${kubernetes_namespace.app.metadata[0].name}"
  }
}

# === Monitoring Stack Status ===
output "monitoring_stack_info" {
  description = "Information about the deployed monitoring stack"
  value = {
    prometheus = {
      namespace     = kubernetes_namespace.monitoring.metadata[0].name
      service_name  = "${helm_release.prometheus_operator.name}-prometheus"
      port         = 9090
      retention    = var.prometheus_retention
      storage_size = var.prometheus_storage_size
    }
    grafana = {
      namespace     = kubernetes_namespace.monitoring.metadata[0].name
      service_name  = "${helm_release.prometheus_operator.name}-grafana"
      port         = 80
      admin_user   = "admin"
      storage_size = var.grafana_storage_size
    }
    alertmanager = {
      namespace    = kubernetes_namespace.monitoring.metadata[0].name
      service_name = "${helm_release.prometheus_operator.name}-alertmanager"
      port        = 9093
    }
    ingress = {
      namespace    = kubernetes_namespace.ingress.metadata[0].name
      service_name = "${helm_release.nginx_ingress.name}-controller"
      class        = var.ingress_class
    }
  }
}

# === Application Configuration ===
output "app_configuration" {
  description = "Application configuration summary"
  value = {
    namespace       = kubernetes_namespace.app.metadata[0].name
    config_map      = kubernetes_config_map.app_config.metadata[0].name
    secrets         = kubernetes_secret.app_secrets.metadata[0].name
    resource_quota  = kubernetes_resource_quota.app_quota.metadata[0].name
    storage_class   = var.create_storage_class ? kubernetes_storage_class.app_storage[0].metadata[0].name : var.storage_class
    log_level       = var.log_level
    metrics_enabled = true
  }
}

# === Quick Start Guide ===
output "quick_start_commands" {
  description = "Commands to get started with the deployed infrastructure"
  value = {
    "1_check_pods" = "kubectl get pods -A"
    "2_access_grafana" = "kubectl port-forward -n monitoring svc/${helm_release.prometheus_operator.name}-grafana 3000:80"
    "3_grafana_login" = "Open http://localhost:3000 - admin/${var.grafana_admin_password}"
    "4_access_prometheus" = "kubectl port-forward -n monitoring svc/${helm_release.prometheus_operator.name}-prometheus 9090:9090"
    "5_prometheus_ui" = "Open http://localhost:9090"
    "6_check_ingress" = "kubectl get svc -n ingress-nginx"
    "7_view_metrics" = "kubectl top nodes && kubectl top pods -A"
  }
}
