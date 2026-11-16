@description('Radius-provided object containing information about the resource calling the Recipe')
param context object

var storageAccountName = context.resource.properties.storageAccountName
var containerName = context.resource.properties.containerName
var namespace = context.runtime.kubernetes.namespace

resource storageAccount 'Microsoft.Storage/storageAccounts@2026-09-01' existing = {
  name: storageAccountName
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-09-01' = {
  name: '${storageAccount.name}/default/${containerName}'
  properties: {
    publicAccess: 'None'
  }
} 

extension kubernetes with {
  kubeConfig: ''
  namespace: namespace
} as kubernetes

var daprType = 'bindings.azure.blobstorage'
var daprVersion = 'v1'

#disable-next-line BCP081
resource daprComponent 'dapr.io/Component@v1alpha1' = {
  metadata: {
    name: context.resource.name
    namespace: namespace
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
    '/planes/kubernetes/local/namespaces/${namespace}/providers/dapr.io/Component/${daprComponent.metadata.name}'
  ]
  values: {
    componentName: daprComponent.metadata.name
  }
}
