# Azure
Run the following command to create the necessary azure ressources.

```bash
az deployment group create --name <deployment Name> --resource-group <resource group> --template-file infrastructure.bicep --parameters @parameter.json
```

## Dapr Components
Dapr components deploy with `infrastructure.bicep`. Each component is a module in `DaprComponents/`:

| Module | Component | Type |
|---|---|---|
| `storage.bicep` | `file-entry-storage-binding` | `bindings.azure.blobstorage` |
| `notificationstorage.bicep` | `notification-storage` | `bindings.azure.blobstorage` |
| `messagebus.bicep` | `messagebus` | `pubsub.azure.servicebus.topics` |
| `secretstore.bicep` | `secretstore` | `secretstores.azure.keyvault` |
| `statestore.bicep` | `statestore` | `state.azure.cosmosdb` |

Components authenticate with the user-assigned managed identity (`azureClientId`); its roles are assigned by the modules in `RoleAssignments/` (`storage.bicep`, `servicebus.bicep`, `cosmosdb.bicep`). The Dapr components store no keys or component secrets.

The Cognitive Services key is still stored in Key Vault as `cognitive-service-key`. The Computervision services read it through the `secretstore` component.

The Cosmos DB account, database (`daprstate`) and container (`statestore`, partition key `/partitionKey`) are created in `infrastructure.bicep`. The state store module gets the endpoint from the account.

No `az containerapp env dapr-component set` needed anymore.
