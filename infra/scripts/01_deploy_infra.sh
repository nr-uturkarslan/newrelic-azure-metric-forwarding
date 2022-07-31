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
appInsightsName="appins$program$locationShort$project$stageShort$instance"
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

# Function app
echo "Checking function app..."

functionApp=$(az functionapp show \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  2> /dev/null)

if [[ $functionApp == "" ]]; then
  echo "Function app does not exists. Creating..."

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
