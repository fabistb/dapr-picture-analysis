// Import the set of Radius resources (Applications.*) into Bicep
import radius as radius

@description('Specifies the environment for resources')
param environment string

@description('The app ID of your Radius Application. Set automatically by the rad CLI.')
param application string

@description('The azure location.')
param azLocation string = resourceGroup().location

@description('The azure storage account name')
param storageAccountName string

@description('The cognitive service account name')
param cognitiveServiceAccountName string

@description('The azure servicebus namespace name.')
param servicebusNamespaceName string

@description('The key vault name')
param keyVaultName string

var defaultSASKeyName = 'RootManageSharedAccessKey'
var sbVersion = '2022-10-01-preview'
var defaultAuthRulesResourceId = resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', servicebusNamespaceName, defaultSASKeyName)

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
  location: azLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

resource fileEntryContainer 'Applications.Core/extenders@2023-10-01-preview' = {
  name: 'file-entry-storage-binding'
  properties: {
    environment: environment
    application: application
    recipe: {
      name: 'blobstoragecontainerdapr'
      parameters: {
        storageAccountName: storageAccount.name
        containerName: 'file-entry'
      }
    }
  }
} 

resource notificationContainer 'Applications.Core/extenders@2023-10-01-preview' = {
  name: 'notification-storage'
  properties: {
    environment: environment
    application: application
    recipe: {
      name: 'blobstoragecontainerdapr'
      parameters: {
        storageAccountName: storageAccount.name
        containerName: 'notification-storage'
      }
    }
  }
}

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2022-10-01' = {
  name: cognitiveServiceAccountName
  location: azLocation
  sku: {
    name: 'F0'
  }
  kind: 'ComputerVision'
  properties: {
    customSubDomainName: cognitiveServiceAccountName
    networkAcls: {
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
  }
} 

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: servicebusNamespaceName
  location: azLocation
  sku: {
    name: 'Standard'
  }
}

resource daprPubSub 'Applications.Dapr/pubSubBrokers@2023-10-01-preview' = {
  name: 'messagebus'
  properties: {
    environment: environment
    application: application
    resourceProvisioning: 'manual'
    type: 'pubsub.azure.servicebus.queues'
    version: 'v1'
    metadata: {
      connectionString: listkeys(defaultAuthRulesResourceId, sbVersion).primaryConnectionString
    }

  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: azLocation
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
    enableSoftDelete: false
    publicNetworkAccess: 'Enabled'
    accessPolicies: [
      
    ]
  }
}

resource secretStore 'Applications.Dapr/secretStores@2023-10-01-preview' = {
  name: 'secretstore'
  properties: {
    environment: environment
    application: application
    resourceProvisioning: 'manual'
    type: 'secretstores.azure.keyvault'
    version: 'v1'
    metadata: {
      vaultName: keyVault.name
      azureTenantId: ''
      azureClientId: ''
      azureClientSecret: ''
    } 
  }
}

resource cognitiveServiceKey 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: 'cognitive-service-key'
  properties: {
    value: cognitiveService.listKeys().key1
  }
}

resource gateway 'Applications.Core/containers@2023-10-01-preview' = {
  name: 'gateway'
  properties: {
    application: application
    container: {
      image: 'daprpictureacr001.azurecr.io/gateway:latest'
    }
    extensions: [
      {
        kind: 'daprSidecar'
        appId: 'gateway'
        appPort: 80
      }
    ]
  } 
}

resource computervision 'Applications.Core/containers@2023-10-01-preview' = {
  name: 'computervision'
  properties: {
    application: application
    container: {
      image: 'daprpictureacr001.azurecr.io/computervision:latest'
      env: {
        COGNITIVE_SERVICE_URL: cognitiveService.properties.endpoint
      }
    }
    extensions: [
      {
        kind: 'daprSidecar'
        appId: 'computervision'
        appPort: 80
      }
    ]
  
  }
}

resource fileService 'Applications.Core/containers@2023-10-01-preview' = {
  name: 'file-service'
  properties: {
    application: application
    container: {
      image: 'daprpictureacr001.azurecr.io/fileservice:latest'
    }
    extensions: [
      {
        kind: 'daprSidecar'
        appId: 'file-service'
        appPort: 80
      }
    ]
  }
}

resource notification 'Applications.Core/containers@2023-10-01-preview' = {
  name: 'notification'
  properties: {
    application: application
    container: {
      image: 'daprpictureacr001.azurecr.io/notificationdotnet:latest'
    }
    extensions: [
      {
        kind: 'daprSidecar'
        appId: 'notification'
        appPort: 80
      }
    ]
  }
}

