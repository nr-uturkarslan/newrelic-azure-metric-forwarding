#!/bin/bash

#############
### Setup ###
#############

### Set parameters
program="ugur"
locationLong="westeurope"
locationShort="euw"
project="metrics"
stageLong="dev"
stageShort="d"
instance="001"

### Set variables
resourceGroupName="rg$program$locationShort$project$stageShort$instance"
storageAccountName="st$program$locationShort$project$stageShort$instance"
configBlobContainerName="config"
appInsightsName="appins$program$locationShort$project$stageShort$instance"
appServicePlanName="plan$program$locationShort$project$stageShort$instance"
functionAppName="func$program$locationShort$project$stageShort$instance"

#############
### Infra ###
#############

# Resource group
echo "Checking resource group..."

resourceGroup=$(az group show \
  --name $resourceGroupName \
  2> /dev/null)

if [[ $resourceGroup == "" ]]; then
  echo "Resource group does not exists. Creating..."

  resourceGroup=$(az group create \
    --name $resourceGroupName \
    --location $locationLong)

  echo -e "Resource group is created.\n"
else
  echo -e "Resource group already exists.\n"
fi

# Storage account
echo "Checking storage account..."

storageAccount=$(az storage account show \
  --resource-group $resourceGroupName \
  --name $storageAccountName \
  2> /dev/null)

if [[ $storageAccount == "" ]]; then
  echo "Storage account does not exists. Creating..."

  storageAccount=$(az storage account create \
    --resource-group $resourceGroupName \
    --name $storageAccountName \
    --location $locationLong \
    --sku "Standard_LRS")

  echo -e "Storage account is created.\n"
else
  echo -e "Storage account already exists.\n"
fi

# Blob container (config)
echo -e "Retrieving storage account connection string..."
storageAccountConnectionString=$(az storage account show-connection-string \
  --resource-group $resourceGroupName \
  --name $storageAccountName \
  | jq -r .connectionString \
  2> /dev/null)

if [[ $storageAccountConnectionString == "" ]]; then
  echo -e "Storage account connection string could not be retrieved.\n"
  exit 1
fi

echo "Checking config blob container..."

blobContainer=$(az storage container show \
  --account-name $storageAccountName \
  --name $configBlobContainerName \
  --connection-string $storageAccountConnectionString \
  2> /dev/null)

if [[ $blobContainer == "" ]]; then
  echo "Config blob container does not exists. Creating..."

  blobContainer=$(az storage container create \
    --account-name $storageAccountName \
    --name $configBlobContainerName \
    --connection-string $storageAccountConnectionString)

  echo -e "Config blob container is created.\n"
else
  echo -e "Config blob container already exists.\n"
fi

# Application insights
echo "Checking app insights..."

appInsights=$(az monitor app-insights component show \
    --resource-group $resourceGroupName \
    --app $appInsightsName \
    2> /dev/null)

if [[ $appInsights == "" ]]; then
  echo "App insights does not exists. Creating..."

  appInsights=$(az monitor app-insights component create \
    --resource-group $resourceGroupName \
    --app $appInsightsName \
    --location $locationLong \
    --kind "web" \
    --application-type "web")

    echo -e "App insights is created.\n"
else
  echo -e "App insights already exists.\n"
fi

# # App Service plan
# echo "Checking app service plan..."

# appServicePlan=$(az appservice plan show \
#   --resource-group $resourceGroupName \
#   --name $appServicePlanName \
#   2> /dev/null)

# if [[ $appServicePlan == "" ]]; then
#   echo "App service plan does not exists. Creating..."

#   appServicePlan=$(az appservice plan create \
#     --resource-group $resourceGroupName \
#     --name $appServicePlanName \
#     --sku "F1")

#   echo -e "App service plan is created.\n"
# else
#   echo -e "App service plan already exists.\n"
# fi

# Function app
echo "Checking function app..."

functionApp=$(az functionapp show \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  2> /dev/null)

if [[ $functionApp == "" ]]; then
  echo "Function app does not exists. Creating..."

  # # if you want to assign a plan to function app
  # --plan $appServicePlanName \

  functionApp=$(az functionapp create \
    --resource-group $resourceGroupName \
    --name $functionAppName \
    --storage-account $storageAccountName \
    --consumption-plan-location $locationLong \
    --app-insights $appInsightsName \
    --functions-version 4 \
    --runtime dotnet)

  echo -e "Function app is created.\n"
else
  echo -e "Function app already exists.\n"
fi

# Function app system assigned identity
echo "Checking function app system assigned identity..."

functionAppPrincipalId=$(az functionapp identity show \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  | jq -r .principalId \
  2> /dev/null)

if [[ $functionAppPrincipalId == "" ]]; then
  echo "Function app does not have system assigned identity. Creating..."

  functionAppPrincipalId=$(az functionapp identity assign \
    --resource-group $resourceGroupName \
    --name $functionAppName \
    | jq -r .principalId)

  echo -e "Function app system assigned identity is created.\n"
else
  echo -e "Function app system assigned identity already exists.\n"
fi

# Set function app environment variables
az functionapp config appsettings set \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  --settings "NEW_RELIC_LICENSE_KEY=$NEWRELIC_LICENSE_KEY" \
  > /dev/null

az functionapp config appsettings set \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  --settings "CONFIG_BLOB_URI=https://$storageAccountName.blob.core.windows.net/$configBlobContainerName" \
  > /dev/null

# The identity is not assigned directly, wait for a while
sleep 5

# Account info
subscriptionId=$(az account show | jq -r .id)
subscriptionScope="/subscriptions/$subscriptionId"
blobContainerScope="$subscriptionScope/resourceGroups/$resourceGroupName/providers/Microsoft.Storage/storageAccounts/$storageAccountName/blobServices/default/containers/$configBlobContainerName"

# Monitoring Reader role assignment
monitoringRoleAssignmentCount=$(az role assignment list \
  --assignee "$functionAppPrincipalId" \
  --role "Monitoring Reader" \
  --scope "$subscriptionScope" \
  | jq '. | length' \
  2> /dev/null)

if [[ $monitoringRoleAssignmentCount -eq 0 ]]; then
  echo "Function app does not have Monitoring Reader role. Assigning..."

  monitoringRoleAssignment=$(az role assignment create \
    --assignee "$functionAppPrincipalId" \
    --role "Monitoring Reader" \
    --scope "$subscriptionScope" \
    2> /dev/null)

  echo -e "Monitoring Reader role is assinged.\n"
else
  echo -e "Function app already has Monitoring Reader role.\n"
fi

# Storage Blob Data Contributor role assignment
blobContributorRoleAssignmentCount=$(az role assignment list \
  --assignee "$functionAppPrincipalId" \
  --role "Storage Blob Data Contributor" \
  --scope "$blobContainerScope" \
  | jq '. | length' \
  2> /dev/null)

if [[ $blobContributorRoleAssignmentCount -eq 0 ]]; then
  echo "Function app does not have Storage Blob Data Contributor role. Assigning..."

  blobContributorRoleAssignment=$(az role assignment create \
      --assignee "$functionAppPrincipalId" \
      --role "Storage Blob Data Contributor" \
      --scope $blobContainerScope \
    2> /dev/null)

  echo -e "Storage Blob Data Contributor role is assinged.\n"
else
  echo -e "Function app already has Storage Blob Data Contributor role.\n"
fi
