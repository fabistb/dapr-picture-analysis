# Number Generator Service

A unique number generator service built with .NET 10.0, utilizing Dapr Actors for distributed state management.

## Overview

This service provides a simple REST API endpoint that generates unique, incrementing numbers using Dapr Actors and Redis as the actor state store.

## Architecture

- **Framework**: .NET 10.0
- **Dapr SDK**: 1.16.1
- **State Store**: Redis (via Dapr Actor State Store)
- **Actor Pattern**: Dapr Actors for reliable state management

## API Endpoints

### GET /api/v1.0/Number

Returns the next unique number in the sequence.

**Response**: `200 OK`
```
1
```

Each subsequent call increments the number:
```
2
3
4
...
```

## Project Structure

```
NumberGenerator/
├── NumberGenerator/              # Main API project
│   ├── Actors/                  # Dapr Actor implementations
│   │   ├── IUniqueNumberActor.cs
│   │   └── UniqueNumberActor.cs
│   ├── Controllers/             # API Controllers
│   │   └── NumberController.cs
│   ├── components/              # Dapr component definitions
│   │   └── actorstatestore.yaml
│   └── Program.cs
├── NumberGenerator.UnitTests/    # Unit tests
└── NumberGenerator.IntegrationTests/  # Integration tests with Testcontainers
```

## Running the Service

### Prerequisites

- .NET 10.0 SDK
- Dapr CLI (for local development)
- Redis (or use Dapr Sidekick for automatic setup)

### Local Development

The service uses Dapr Sidekick for automatic Dapr sidecar management in development mode:

```bash
cd NumberGenerator/NumberGenerator
dotnet run
```

Dapr Sidekick will automatically:
- Start the Dapr sidecar
- Configure the Redis actor state store
- Set up the necessary ports

### Manual Dapr Execution

Alternatively, you can run with Dapr CLI manually:

```bash
cd NumberGenerator/NumberGenerator
dapr run --app-id numbergenerator --app-port 5000 --dapr-http-port 3500 --components-path ./components -- dotnet run
```

## Testing

### Unit Tests

Run unit tests using:

```bash
cd NumberGenerator
dotnet test NumberGenerator.UnitTests/NumberGenerator.UnitTests.csproj
```

Unit tests use:
- MSTest
- Moq (for mocking)
- AutoFixture (for test data generation)
- AwesomeAssertions (for enhanced assertions)

### Integration Tests

Run integration tests using:

```bash
cd NumberGenerator
dotnet test NumberGenerator.IntegrationTests/NumberGenerator.IntegrationTests.csproj
```

Integration tests use:
- MSTest
- Testcontainers (for Redis container)
- Microsoft.AspNetCore.Mvc.Testing (for in-memory API testing)

**Note**: Integration tests require Docker to be running for Testcontainers.

## Configuration

### appsettings.json

The service can be configured via `appsettings.json`:

```json
{
  "DaprSidekick": {
    "Sidecar": {
      "AppId": "numbergenerator",
      "AppPort": 5000,
      "DaprHttpPort": 3500,
      "DaprGrpcPort": 50001,
      "MetricsPort": 9090,
      "ComponentsDirectory": "./components"
    }
  }
}
```

### Actor State Store

The Redis actor state store is configured in `components/actorstatestore.yaml`:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: actorstatestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: actorStateStore
    value: "true"
```

## Dependencies

### Main Project
- Dapr.AspNetCore: 1.16.1
- Dapr.Actors.AspNetCore: 1.16.1
- Man.Dapr.Sidekick.AspNetCore: 1.2.2

### Unit Tests
- MSTest: 4.0.1
- Moq: 4.20.72
- AutoFixture: 4.18.1
- AwesomeAssertions: 7.0.0

### Integration Tests
- MSTest: 4.0.1
- Microsoft.AspNetCore.Mvc.Testing: 9.0.0
- Testcontainers: 4.0.0
- Testcontainers.Redis: 4.0.0

## How It Works

1. **Request**: Client makes a GET request to `/api/v1.0/Number`
2. **Controller**: `NumberController` receives the request and creates an actor proxy
3. **Actor**: `UniqueNumberActor` retrieves the current number from Redis state store
4. **Increment**: Actor increments the number and saves it back to Redis
5. **Response**: New number is returned to the client

The actor ensures thread-safe, distributed number generation across multiple instances of the service.
