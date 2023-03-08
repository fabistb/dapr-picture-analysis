# Infrastructure
Run the following command to create the necessary azure ressources.

```bash
az deployment group create --name <deployment name> --resource-group <your ressource group> --template-file resources.bicep --parameters @parameters.json
```