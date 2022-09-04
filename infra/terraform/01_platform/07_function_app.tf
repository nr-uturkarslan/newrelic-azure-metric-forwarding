### Function App ###

# Service Plan
resource "azurerm_service_plan" "platform" {
  name                = var.service_plan_name_platform
  resource_group_name = azurerm_resource_group.platform.name
  location            = azurerm_resource_group.platform.location

  os_type  = "Linux"
  sku_name = "Y1"
}

# Function App
resource "azurerm_linux_function_app" "platform" {
  name                = var.function_app_name_platform
  resource_group_name = azurerm_resource_group.platform.name
  location            = azurerm_resource_group.platform.location

  storage_account_name       = azurerm_storage_account.platform.name
  storage_account_access_key = azurerm_storage_account.platform.primary_access_key
  service_plan_id            = azurerm_service_plan.platform.id

  app_settings = {

    # Application Insights
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.platform.instrumentation_key

    # Open Telemetry
    NEW_RELIC_APP_NAME    = "ForwardMetrics"
    NEW_RELIC_LICENSE_KEY = var.new_relic_license_key
  }

  site_config {}

  identity {
    type = "SystemAssigned"
  }
}

# Monitoring Reader on subscription
resource "azurerm_role_assignment" "monitoring_reader_on_subscription_for_function" {
  scope                = data.azurerm_subscription.primary.id
  role_definition_name = "Monitoring Reader"
  principal_id         = azurerm_linux_function_app.platform.identity[0].principal_id
}

# Storage Blob Data Contributor on Platform Blob
resource "azurerm_role_assignment" "blob_contributor_on_platform_blob_for_function" {
  scope                = azurerm_storage_container.platform.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_function_app.platform.identity[0].principal_id
}
