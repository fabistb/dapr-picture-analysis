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