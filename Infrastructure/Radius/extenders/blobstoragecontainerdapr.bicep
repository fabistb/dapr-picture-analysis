@description('Radius-provided object containing information about the resouce calling the Recipe')
param context object

@description('The storage account which already exists.')
param storageAccountName string

@description('The container name which is used in the dapr binding.')
param containerName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' existing = {
  name: storageAccountName
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-05-01' = {
  name: '${storageAccount.name}/default/${containerName}'
  properties: {
    publicAccess: 'None'
  }
} 

extension kubernetes with {
  kubeConfig: ''
  namespace: context.runtime.kubernetes.namespace
} as kubernetes

var daprType = 'bindings.azure.blobstorage'
var daprVersion = 'v1'

resource daprComponent 'dapr.io/Component@v1alpha1' = {
  metadata: {
    name: context.resource.name
  }
  spec: {
    type: daprType 
    version: daprVersion
    metadata: [
      {
        name: 'accountName'
        value: storageAccount.name
      }
      {
        name: 'accountKey'
        value: storageAccount.listKeys().keys[0].value
      }
      {
        name: 'containerName'
        value: containerName
      }
    ]
  }
}

output result object = {
  // This workaround is needed because the deployment engine omits Kubernetes resources from its output.
  // This allows Kubernetes resources to be cleaned up when the resource is deleted.
  // Once this gap is addressed, users won't need to do this.
  resources: [
    '/planes/kubernetes/local/namespaces/${daprComponent.metadata.namespace}/providers/dapr.io/Component/${daprComponent.metadata.name}'
  ]
  values: {
    type: daprType
    version: daprVersion
    metadata: daprComponent.spec.metadata
  }
}
