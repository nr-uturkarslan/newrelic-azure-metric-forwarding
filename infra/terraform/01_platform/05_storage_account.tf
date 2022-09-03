### Storage Account ###

# Storage Account
resource "azurerm_storage_account" "platform" {
  name                = var.storage_account_name_platform
  resource_group_name = azurerm_resource_group.platform.name
  location            = azurerm_resource_group.platform.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Blob Container - Platform
resource "azurerm_storage_container" "platform" {
  name                  = var.blob_container_name_platform
  storage_account_name  = azurerm_storage_account.platform.name
  container_access_type = "private"
}
