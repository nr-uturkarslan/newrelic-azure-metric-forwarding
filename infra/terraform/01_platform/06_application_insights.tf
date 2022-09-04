### Application Insights ###

# Application Insights
resource "azurerm_application_insights" "platform" {
  name                = var.application_insights_name_platform
  resource_group_name = azurerm_resource_group.platform.name
  location            = azurerm_resource_group.platform.location
  application_type    = "web"
}
