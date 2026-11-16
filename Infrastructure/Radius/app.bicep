// Import the set of Radius resources (Radius.*) into Bicep
extension radius
extension radiusDapr
extension daprPicture

@description('Specifies the environment for resources')
param environment string

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

@description('The client ID of the service principal the Dapr secret store uses to read the key vault')
param azureClientId string

@description('The object ID of the same service principal')
param azureClientObjectId string

@secure()
@description('The client secret of the same service principal')
param azureClientSecret string

resource application 'Radius.Core/applications@2025-08-01-preview' = {
  name: 'dapr-picture-analysis'
  properties: {
    environment: environment
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2026-09-01' = {
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

resource fileEntryContainer 'DaprPicture.Dapr/blobBindings@2025-08-01-preview' = {
  name: 'file-entry-storage-binding'
  properties: {
    environment: environment
    application: application.id
    storageAccountName: storageAccount.name
    containerName: 'file-entry'
    scopes: [
      'file-service'
    ]
  }
} 

resource notificationContainer 'DaprPicture.Dapr/blobBindings@2025-08-01-preview' = {
  name: 'notification-storage'
  properties: {
    environment: environment
    application: application.id
    storageAccountName: storageAccount.name
    containerName: 'notification-storage'
    scopes: [
      'notification'
    ]
  }
}

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2026-09-01' = {
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

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2026-01-01' = {
  name: servicebusNamespaceName
  location: azLocation
  sku: {
    name: 'Standard'
  }
}

// Azure creates this rule with the namespace; the parent reference orders listKeys after the namespace.
resource serviceBusRootRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2026-01-01' existing = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
}

// Becomes a Kubernetes secret of the same name; Dapr resolves secretKeyRef from it through its default Kubernetes secret store.
resource daprCredentials 'Radius.Security/secrets@2025-08-01-preview' = {
  name: 'dapr-credentials'
  properties: {
    environment: environment
    application: application.id
    data: {
      servicebusConnectionString: {
        value: serviceBusRootRule.listKeys().primaryConnectionString
      }
      azureClientSecret: {
        value: azureClientSecret
      }
    }
  }
}

resource daprPubSub 'Radius.Dapr/pubSubBrokers@2025-08-01-preview' = {
  name: 'messagebus'
  properties: {
    environment: environment
    application: application.id
    componentType: 'pubsub.azure.servicebus.queues'
    version: 'v1'
    metadata: {
      connectionString: {
        secretKeyRef: {
          name: daprCredentials.name
          key: 'servicebusConnectionString'
        }
      }
    }
    // Dapr's built-in Kubernetes secret store, which holds dapr-credentials.
    auth: {
      secretStore: 'kubernetes'
    }
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2026-05-15' = {
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
      {
        tenantId: subscription().tenantId
        objectId: azureClientObjectId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

resource secretStore 'Radius.Dapr/secretStores@2025-08-01-preview' = {
  name: 'secretstore'
  properties: {
    environment: environment
    application: application.id
    componentType: 'secretstores.azure.keyvault'
    version: 'v1'
    metadata: {
      vaultName: {
        value: keyVault.name
      }
      azureTenantId: {
        value: subscription().tenantId
      }
      azureClientId: {
        value: azureClientId
      }
      azureClientSecret: {
        secretKeyRef: {
          name: daprCredentials.name
          key: 'azureClientSecret'
        }
      }
    }
  }
}

resource cognitiveServiceKey 'Microsoft.KeyVault/vaults/secrets@2026-05-15' = {
  parent: keyVault
  name: 'cognitive-service-key'
  properties: {
    value: cognitiveService.listKeys().key1
  }
}

resource gateway 'Radius.Compute/containers@2025-08-01-preview' = {
  name: 'gateway'
  properties: {
    environment: environment
    application: application.id
    containers: {
      gateway: {
        image: 'daprpictureacr001.azurecr.io/gateway:latest'
      }
    }
    extensions: {
      daprSidecar: {
        appId: 'gateway'
        appPort: 80
      }
    }
  }
}

resource computervision 'Radius.Compute/containers@2025-08-01-preview' = {
  name: 'computervision'
  properties: {
    environment: environment
    application: application.id
    containers: {
      computervision: {
        image: 'daprpictureacr001.azurecr.io/computervision:latest'
        env: {
          COGNITIVE_SERVICE_URL: {
            value: cognitiveService.properties.endpoint
          }
        }
      }
    }
    extensions: {
      daprSidecar: {
        appId: 'computervision'
        appPort: 80
      }
    }
  }
}

resource fileService 'Radius.Compute/containers@2025-08-01-preview' = {
  name: 'file-service'
  properties: {
    environment: environment
    application: application.id
    containers: {
      fileService: {
        image: 'daprpictureacr001.azurecr.io/fileservice:latest'
      }
    }
    extensions: {
      daprSidecar: {
        appId: 'file-service'
        appPort: 80
      }
    }
  }
}

resource notification 'Radius.Compute/containers@2025-08-01-preview' = {
  name: 'notification'
  properties: {
    environment: environment
    application: application.id
    containers: {
      notification: {
        image: 'daprpictureacr001.azurecr.io/notificationdotnet:latest'
      }
    }
    extensions: {
      daprSidecar: {
        appId: 'notification'
        appPort: 80
      }
    }
  }
}
