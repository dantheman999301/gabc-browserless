provider "azurerm" {
  version = "1.25"
}

resource "azurerm_resource_group" "group" {
  name     = "browserless-web-demo"
  location = "ukwest"
}

resource "azurerm_app_service_plan" "appserviceplan" {
  name                = "${azurerm_resource_group.group.name}-plan"
  location            = "${azurerm_resource_group.group.location}"
  resource_group_name = "${azurerm_resource_group.group.name}"
  reserved = true

  kind = "Linux"

  sku {
    tier = "Standard"
    size = "S2"
    capacity = 1
  }
}

resource "azurerm_app_service" "dockerapp" {
  name                = "${azurerm_resource_group.group.name}-dockerapp"
  location            = "${azurerm_resource_group.group.location}"
  resource_group_name = "${azurerm_resource_group.group.name}"
  app_service_plan_id = "${azurerm_app_service_plan.appserviceplan.id}"

  app_settings {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "0"
    CHROME_REFRESH_TIME = "3600000"
    DEBUG = "browserless*"
    ENABLE_DEBUGGER = "false"
    KEEP_ALIVE = "true"
    MAX_CONCURRENT_SESSIONS = "10"
    MAX_QUEUE_LENGTH = "1000"
    PREBOOT_CHROME = "true"
    TOKEN = "MTQ3ZTRmYjdjYjU5NDY2OGI0ZjNlMDk4YThlMmQ2ZDc="
  }

  site_config {
    linux_fx_version = "DOCKER|browserless/chrome:latest"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }
}