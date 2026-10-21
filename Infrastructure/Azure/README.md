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

Storage key, Service Bus connection string, Cosmos DB key and Key Vault identity come from the resources in the same template. Secrets are stored as component secrets (`secretRef`).

The Cosmos DB account, database (`daprstate`) and container (`statestore`, partition key `/partitionKey`) are created in `infrastructure.bicep`. The state store module gets endpoint and key from the account.

No `az containerapp env dapr-component set` needed anymore.
