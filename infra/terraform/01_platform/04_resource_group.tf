### Resource Group ###

# Resource Group - Platform
resource "azurerm_resource_group" "platform" {
  name     = var.resource_group_name_platform
  location = var.location_long
}
