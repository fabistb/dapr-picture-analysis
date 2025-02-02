# Shared
Run the following command to create the Azure Container Registry.
The registry is required for the **Azure** and **Radius** setup.

```bash
az deployment group create --name <deployment name> --resource-group <resource group> --template-file containerregistry.bicep --parameters @parameter.json
```