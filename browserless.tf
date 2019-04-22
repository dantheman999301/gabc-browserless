provider "azurerm" {
  version = "1.25"
}

resource "azurerm_resource_group" "webapp" {
  name     = "browserless-webapp-demo"
  location = "ukwest"
}

resource "azurerm_resource_group" "function" {
  name     = "browserless-function-demo"
  location = "ukwest"
}

resource "azurerm_app_service_plan" "appserviceplan" {
  name                = "${azurerm_resource_group.webapp.name}-plan"
  location            = "${azurerm_resource_group.webapp.location}"
  resource_group_name = "${azurerm_resource_group.webapp.name}"
  reserved            = true

  kind = "Linux"

  sku {
    tier     = "Standard"
    size     = "S2"
    capacity = 1
  }
}

resource "azurerm_app_service_plan" "appserviceplanfunction" {
  name                = "${azurerm_resource_group.function.name}-function-plan"
  location            = "${azurerm_resource_group.function.location}"
  resource_group_name = "${azurerm_resource_group.function.name}"
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_app_service" "dockerapp" {
  name                = "${azurerm_resource_group.webapp.name}-dockerapp"
  location            = "${azurerm_resource_group.webapp.location}"
  resource_group_name = "${azurerm_resource_group.webapp.name}"
  app_service_plan_id = "${azurerm_app_service_plan.appserviceplan.id}"

  app_settings {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "0"
    CHROME_REFRESH_TIME                 = "3600000"
    DEBUG                               = "browserless*"
    ENABLE_DEBUGGER                     = "false"
    KEEP_ALIVE                          = "true"
    MAX_CONCURRENT_SESSIONS             = "10"
    MAX_QUEUE_LENGTH                    = "1000"
    PREBOOT_CHROME                      = "true"
    TOKEN                               = "MTQ3ZTRmYjdjYjU5NDY2OGI0ZjNlMDk4YThlMmQ2ZDc="
  }

  site_config {
    linux_fx_version = "DOCKER|browserless/chrome:latest"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_storage_account" "browserlessapps" {
  name                     = "strbrowserless"
  resource_group_name      = "${azurerm_resource_group.function.name}"
  location                 = "${azurerm_resource_group.function.location}"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "pdf" {
  name                  = "pdf"
  resource_group_name   = "${azurerm_resource_group.function.name}"
  storage_account_name  = "${azurerm_storage_account.browserlessapps.name}"
  container_access_type = "private"
}

resource "azurerm_function_app" "browserless-apps" {
  name                      = "browserless-functions"
  location                  = "${azurerm_resource_group.function.location}"
  resource_group_name       = "${azurerm_resource_group.function.name}"
  app_service_plan_id       = "${azurerm_app_service_plan.appserviceplanfunction.id}"
  storage_connection_string = "${azurerm_storage_account.browserlessapps.primary_connection_string}"

  app_settings = {
    browserless             = "https://${azurerm_app_service.dockerapp.default_site_hostname}/pdf?token=MTQ3ZTRmYjdjYjU5NDY2OGI0ZjNlMDk4YThlMmQ2ZDc="
    servicebusconnection    = "${azurerm_servicebus_namespace.browserless-apps.default_primary_connection_string}"
    storageconnectionstring = "${azurerm_storage_account.browserlessapps.primary_connection_string}"
  }
}

resource "azurerm_servicebus_namespace" "browserless-apps" {
  name                = "browserless-apps"
  location            = "${azurerm_resource_group.function.location}"
  resource_group_name = "${azurerm_resource_group.function.name}"
  sku                 = "Standard"

  tags = {
    source = "terraform"
  }
}

resource "azurerm_servicebus_queue" "generate" {
  name                = "browserless-apps"
  resource_group_name = "${azurerm_resource_group.function.name}"
  namespace_name      = "${azurerm_servicebus_namespace.browserless-apps.name}"

  enable_partitioning = true
}
