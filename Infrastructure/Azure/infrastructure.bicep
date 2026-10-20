@description('The container registry name')
param acrName string

@description('The azure storage account name')
param storageAccountName string

@description('The azure service bus namespace name')
param serviceBusNamespaceName string

@description('The container app environment')
param containerAppEnvironmentName string

@description('The log analytics workspace name')
param logAnalyticsWorkspaceName string

@description('The cognitiv service account name')
param cognitiveServiceAccountName string

@description('The key vault name')
param keyVaultName string

@description('The user managed identity name')
param userManagedIdentityName string

@description('The cosmos db account name')
param cosmosDbAccountName string

@description('The cosmos db database name used by the dapr state store')
param cosmosDbDatabaseName string = 'daprstate'

@description('The cosmos db container name used by the dapr state store')
param cosmosDbContainerName string = 'statestore'

@description('The resource location')
param location string = resourceGroup().location

resource userManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userManagedIdentityName
  location: location
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties:{}
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
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

resource fileEntry 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: '${storageAccountName}/default/file-entry'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccount
  ]
}

resource notificationStorage 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: '${storageAccountName}/default/notification-storage'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccount
  ]
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    retentionInDays: 30
  }
} 

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: containerAppEnvironmentName
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2022-12-01' = {
  name: cognitiveServiceAccountName
  location: location
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

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
    accessPolicies: [ 
      {
        tenantId: subscription().tenantId
        objectId: userManagedIdentity.properties.principalId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
            'purge'
          ]
        }
      }
    ]
    enableSoftDelete: false
    publicNetworkAccess: 'Enabled'
  }
}

resource blobStorageKeyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: 'storage-connectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

resource cognitivServiceKey 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
  parent: keyVault
  name: 'cognitive-service-key'
  properties: {
    value: cognitiveService.listKeys().key1
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: cosmosDbAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Strong'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosDbAccount
  name: cosmosDbDatabaseName
  properties: {
    resource: {
      id: cosmosDbDatabaseName
    }
  }
}

// Dapr actor state store requires the partition key /partitionKey
resource cosmosDbContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDbDatabase
  name: cosmosDbContainerName
  properties: {
    resource: {
      id: cosmosDbContainerName
      partitionKey: {
        paths: [
          '/partitionKey'
        ]
        kind: 'Hash'
      }
    }
  }
}

module fileEntryBinding 'DaprComponents/storage.bicep' = {
  name: 'daprFileEntryBinding'
  params: {
    containerAppEnvironmentName: containerAppEnvironment.name
    storageAccountName: storageAccount.name
  }
}

module notificationBinding 'DaprComponents/notificationstorage.bicep' = {
  name: 'daprNotificationBinding'
  params: {
    containerAppEnvironmentName: containerAppEnvironment.name
    storageAccountName: storageAccount.name
  }
}

module messageBus 'DaprComponents/messagebus.bicep' = {
  name: 'daprMessageBus'
  params: {
    containerAppEnvironmentName: containerAppEnvironment.name
    serviceBusNamespaceName: serviceBusNamespace.name
  }
}

module secretStore 'DaprComponents/secretstore.bicep' = {
  name: 'daprSecretStore'
  params: {
    containerAppEnvironmentName: containerAppEnvironment.name
    keyVaultName: keyVault.name
    managedIdentityClientId: userManagedIdentity.properties.clientId
  }
}

module stateStore 'DaprComponents/statestore.bicep' = {
  name: 'daprStateStore'
  params: {
    containerAppEnvironmentName: containerAppEnvironment.name
    cosmosDbAccountName: cosmosDbAccount.name
    cosmosDbDatabase: cosmosDbDatabase.name
    cosmosDbCollection: cosmosDbContainer.name
  }
}
