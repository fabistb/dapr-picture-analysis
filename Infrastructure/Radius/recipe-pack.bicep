extension radius

// Must not overlap with the types of the default pack (Radius.Compute/containers etc.).
resource daprPictureRecipePack 'Radius.Core/recipePacks@2025-08-01-preview' = {
  name: 'dapr-picture'
  properties: {
    recipes: {
      'Radius.Dapr/pubSubBrokers': {
        kind: 'bicep'
        source: 'daprpictureacr001.azurecr.io/recipes/pubsubbrokers:0.62.0'
      }
      'Radius.Dapr/secretStores': {
        kind: 'bicep'
        source: 'daprpictureacr001.azurecr.io/recipes/secretstores:0.62.0'
      }
      'DaprPicture.Dapr/blobBindings': {
        kind: 'bicep'
        source: 'daprpictureacr001.azurecr.io/recipes/blobstoragecontainerdapr:0.62.0'
      }
    }
  }
}
