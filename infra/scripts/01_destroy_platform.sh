#!/bin/bash

###################
### Infra Setup ###
###################

### Set parameters
project="nr1"
locationLong="westeurope"
locationShort="euw"
stageLong="dev"
stageShort="d"
instance="001"

shared="shared"
platform="platform"

### Set variables

# Shared
resourceGroupNameShared="rg${project}${locationShort}${shared}x000"
storageAccountNameShared="st${project}${locationShort}${shared}x000"

# Platform
resourceGroupNamePlatform="rg${project}${locationShort}${platform}${stageShort}${instance}"

storageAccountNamePlatform="st${project}${locationShort}${platform}${stageShort}${instance}"
blobContainerNamePlatform="platform"

applicationInsightsNamePlatform="appins${project}${locationShort}${platform}${stageShort}${instance}"
servicePlanNamePlatform="plan${project}${locationShort}${platform}${stageShort}${instance}"
functionAppNamePlatform="func${project}${locationShort}${platform}${stageShort}${instance}"

### Terraform destroy

terraform -chdir=../terraform/01_platform destroy \
  -var project=$project \
  -var location_long=$locationLong \
  -var location_short=$locationShort \
  -var stage_short=$stageShort \
  -var stage_long=$stageLong \
  -var instance=$instance \
  -var new_relic_license_key=$NEWRELIC_LICENSE_KEY \
  -var resource_group_name_platform=$resourceGroupNamePlatform \
  -var storage_account_name_platform=$storageAccountNamePlatform \
  -var blob_container_name_platform=$blobContainerNamePlatform \
  -var application_insights_name_platform=$applicationInsightsNamePlatform \
  -var service_plan_name_platform=$servicePlanNamePlatform \
  -var function_app_name_platform=$functionAppNamePlatform
