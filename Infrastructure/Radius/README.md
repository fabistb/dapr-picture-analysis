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

Install _Radius_ on the cluster.
```bash
rad install kubernetes
```

## Run Radius application
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

```bash
rad credential register azure --client-id myClientId  --client-secret myClientSecret  --tenant-id myTenantId
```

```bash
az acr update --name myregistry --anonymous-pull-enabled
```


```bash
rad run app.bicep --parameters @parameters.json
```
