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
functionAppName="func$program$locationShort$project$stageShort$instance"

###########
### App ###
###########

### Publish code
dotnet publish \
  ../../apps/ForwardMetrics/ForwardMetrics/ForwardMetrics.csproj \
  -c Release

### Zip binaries
currentDir=$(pwd)
cd ../../apps/ForwardMetrics/ForwardMetrics/bin/Release/net6.0/publish
zip -r publish.zip .
# zip -r "publish.zip" "../../apps/ForwardMetrics/ForwardMetrics/bin/Release/net6.0/publish"

### Deploy binaries
az functionapp deployment source config-zip \
  --resource-group $resourceGroupName \
  --name $functionAppName \
  --src "publish.zip"

### Clean up
rm publish.zip
cd $currentDir
