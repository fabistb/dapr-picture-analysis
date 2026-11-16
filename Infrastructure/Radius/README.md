# Radius
Ensure that the folling applications are are present.

- [Docker](https://docs.docker.com/desktop/)
- [K3d](https://k3d.io/)
- [Dapr](https://docs.dapr.io/getting-started/install-dapr-cli/)
- [Radius](https://docs.radapp.io/installation/)

## Setup K3d cluster
To setup a new _K3d_ cluster.

```bash
k3d cluster create dapr-picture-analysis
```
Ensure that the newly created cluster is setup as context.

```bash
kubectl config current-context
```

Install _Dapr_ on the cluster.
```bash
dapr init -k
```

Install the _Radius_ CLI in the pinned version.
```bash
curl -fsSL https://raw.githubusercontent.com/radius-project/radius/main/deploy/install.sh | /bin/bash -s -- --version 0.62.0
```

Install _Radius_ on the cluster.
```bash
rad install kubernetes
```

To upgrade an existing installation run `rad upgrade kubernetes`. Only upgrades to the next version are supported, so an installation older than 0.62 (e.g. 0.40) has to be uninstalled (`rad uninstall kubernetes`) and installed again.

## Setup Radius environment
Initialize the _Radius environment_ run:

```bash
rad init
```

Ensure that you don't setup a new app in the folder.

```bash
rad env update default --azure-subscription-id <azure subscription> --azure-resource-group  <azure resource group>
```

```bash
az ad sp create-for-rbac -n DaprPictureAnalysis
```

The same service principal is used by the Dapr secret store to read the key vault, because a K3d cluster has no Azure workload identity. Note its `appId` (client ID), `password` (client secret) and look up its object ID:

```bash
az ad sp show --id <appId> --query id -o tsv
```

```bash
rad credential register azure sp --client-id myClientId  --client-secret myClientSecret  --tenant-id myTenantId
```

```bash
az acr update --name myregistry --anonymous-pull-enabled
```

## Resource types
The Dapr resource types `Radius.Dapr/pubSubBrokers` and `Radius.Dapr/secretStores` come from [resource-types-contrib](https://github.com/radius-project/resource-types-contrib) (commit `9073ec4adf71b7a27b5899f609a5e2eaa9fd4c48`). The custom type `DaprPicture.Dapr/blobBindings` is defined in `types/blobBindings.yaml`.

Register the types in _Radius_.

```bash
C=https://raw.githubusercontent.com/radius-project/resource-types-contrib/9073ec4adf71b7a27b5899f609a5e2eaa9fd4c48
mkdir -p /tmp/contrib
curl -fsSL $C/Dapr/pubSubBrokers/pubSubBrokers.yaml -o /tmp/contrib/pubSubBrokers.yaml
curl -fsSL $C/Dapr/secretStores/secretStores.yaml -o /tmp/contrib/secretStores.yaml
rad resource-type create --from-file /tmp/contrib/pubSubBrokers.yaml
rad resource-type create --from-file /tmp/contrib/secretStores.yaml
rad resource-type create --from-file types/blobBindings.yaml
```

The Bicep files import the types through the extensions in `extensions/` (see `bicepconfig.json`). Regenerate them when a type changes. Both Dapr types share one namespace, so they are merged into one file first.

```bash
(cat /tmp/contrib/pubSubBrokers.yaml; echo; tail -n +3 /tmp/contrib/secretStores.yaml) > /tmp/contrib/radius-dapr.yaml
rad bicep publish-extension --from-file /tmp/contrib/radius-dapr.yaml --target extensions/radius-dapr.tgz --force
rad bicep publish-extension --from-file types/blobBindings.yaml --target extensions/dapr-picture.tgz --force
```

## Recipes
Recipes provision the infrastructure behind a resource type. Push them to the Azure Container Registry. `blobstoragecontainerdapr` is in `recipes/`, the Dapr recipes are taken from _resource-types-contrib_.

```bash
az acr login --name daprpictureacr001
curl -fsSL $C/Dapr/pubSubBrokers/recipes/kubernetes/bicep/kubernetes-dapr-pubsubbrokers.bicep -o /tmp/contrib/pubsubbrokers.bicep
curl -fsSL $C/Dapr/secretStores/recipes/kubernetes/bicep/kubernetes-dapr-secretstores.bicep -o /tmp/contrib/secretstores.bicep
rad bicep publish --file /tmp/contrib/pubsubbrokers.bicep --target br:daprpictureacr001.azurecr.io/recipes/pubsubbrokers:0.62.0
rad bicep publish --file /tmp/contrib/secretstores.bicep --target br:daprpictureacr001.azurecr.io/recipes/secretstores:0.62.0
rad bicep publish --file recipes/blobstoragecontainerdapr.bicep --target br:daprpictureacr001.azurecr.io/recipes/blobstoragecontainerdapr:0.62.0
```

Deploy the recipe pack and assign it to the environment. The `default` pack stays assigned, because it provides the recipe for `Radius.Compute/containers`.

```bash
rad deploy recipe-pack.bicep
rad env update default --recipe-packs default,dapr-picture
```

## Run Radius application

The service principal values are passed on the command line, so the client secret never lands in `parameters.json`. Radius stores the Service Bus connection string and the client secret in the Kubernetes secret `dapr-credentials`, and the Dapr components reference it with `secretKeyRef`.

```bash
rad deploy app.bicep --parameters @parameters.json \
  --parameters azureClientId=<appId> \
  --parameters azureClientObjectId=<objectId> \
  --parameters azureClientSecret=<password>
```
