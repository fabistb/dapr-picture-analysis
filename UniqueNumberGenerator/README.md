# Unique Number Generator

The *Unique Number Generator* hands out unique, consecutive numbers. The number is kept by a *Dapr Actor* which persists its current value in the *Dapr Actor State Store* and increments it by one on every request.

## Endpoint
| Method | Route                     | Description                         |
|--------|---------------------------|-------------------------------------|
| GET    | `/api/v1.0/uniquenumber`  | Returns the next unique number      |

Response:
```json
{ "number": 1 }
```

## Debugging
The service can be easily debugged locally.

**Requirements:**
- Dapr installed locally
- Redis running on `localhost:6379` (e.g. `docker run -d -p 6379:6379 redis`)

The state store is configured in the _statestore.yaml_ file in the components folder (`actorStateStore: true`).

Since the service utilizes _Man.Dapr.Sidekick.AspNetCore_ the application can be debugged locally by starting the application in debug mode.

Example requests can be found in the _requests_ folder.

## Tests
```bash
 $ dotnet test --solution UniqueNumberGenerator.sln
```

## Deployment
To create a docker image for the service navigate to the _UniqueNumberGenerator_ folder and run the following command:

```bash
 $ docker build --platform amd64 -t uniquenumbergenerator  .
```

The infrastructure setup (Dapr components for Azure) is not part of this service yet.
