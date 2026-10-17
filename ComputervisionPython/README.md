# Computervision Python
Python (FastAPI) implementation of the _Computervision_ service. Behaves like the .NET version:
it receives `message-received` events, loads the picture from the file service, analyses it with
Azure Computer Vision and publishes a `notification-received` event if it probably contains a cat.

## Debugging
**Requirements:**
- [uv](https://docs.astral.sh/uv/)
- Dapr installed locally, File Service running locally, Azure Cognitive Services

In _secrets.json_ set _cognitive-service-key_ to the key of your Azure Cognitive Services account.
In _components/secretstore.yaml_ replace `[Path]` with the path to the _secrets.json_ file.

```bash
$ uv sync
$ COGNITIVE_SERVICE_URL=<endpoint> dapr run --app-id computervision --app-port 8000 --dapr-http-port 3500 --components-path ./components -- uv run uvicorn app.main:app --reload --port 8000
```

## Tests
```bash
$ uv run pytest
```

## Deployment
To create a docker image (runs the unit tests during the build) navigate to the _ComputervisionPython_ folder and run:

```bash
$ docker build --platform amd64 -t computervisionpython .
```

The container app settings are the same as for _Computervision_ (target port 80, `COGNITIVE_SERVICE_URL`, dapr app id/port).
