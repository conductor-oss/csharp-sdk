# Conductor OSS C# SDK

[![CI](https://github.com/conductor-oss/csharp-sdk/actions/workflows/pull_request.yml/badge.svg)](https://github.com/conductor-oss/csharp-sdk/actions)

The conductor-csharp repository provides the client SDKs to build task workers in C#.

Building the task workers in C# mainly consists of the following steps:

1. Setup `conductor-csharp` package
1. Create and run task workers
1. Create workflows using code
1. API Documentation

## ⭐ Conductor OSS
Show support for the Conductor OSS.  Please help spread the awareness by starring Conductor repo.

[![GitHub stars](https://img.shields.io/github/stars/conductor-oss/conductor.svg?style=social&label=Star&maxAge=)](https://GitHub.com/conductor-oss/conductor/)

   
### Setup Conductor C# Package​

```shell
dotnet add package conductor-csharp
```

## Configurations

### Authentication Settings (Optional)
Configure the authentication settings if your Conductor server requires authentication.
* keyId: Key for authentication.
* keySecret: Secret for the key.

```csharp
authenticationSettings: new OrkesAuthenticationSettings(
    KeyId: "key",
    KeySecret: "secret"
)
```

### Access Control Setup
See [Access Control](https://orkes.io/content/docs/getting-started/concepts/access-control) for more details on role-based access control with Conductor and generating API keys for your environment.

### Configure API Client
```csharp
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Authentication;

var configuration = new Configuration() {
    BasePath = basePath,
    AuthenticationSettings = new OrkesAuthenticationSettings("keyId", "keySecret")
};

var workflowClient = configuration.GetClient<WorkflowResourceApi>();

workflowClient.StartWorkflow(
    name: "test-sdk-csharp-workflow",
    body: new Dictionary<string, object>(),
    version: 1
)
```

## Running Integration Tests Locally

Set the environment variables for your Conductor server:

```shell
export CONDUCTOR_SERVER_URL=https://your-cluster.example.com/api
export CONDUCTOR_AUTH_KEY=your-key
export CONDUCTOR_AUTH_SECRET=your-secret
```

Run the integration test suite:

```shell
dotnet test Tests/conductor-csharp.test.csproj \
  -p:DefineConstants=EXCLUDE_EXAMPLE_WORKERS \
  --filter "Category=Integration" \
  -l "console;verbosity=normal"
```

All three flags matter:
- `-p:DefineConstants=EXCLUDE_EXAMPLE_WORKERS` prevents example worker code from compiling into the test assembly.
- `--filter "Category=Integration"` runs only the integration tests (skips legacy API tests that require specific server state).
- `-l "console;verbosity=normal"` streams per-test results to the console.

Alternatively, you can run the tests via Docker (see [Harness README](Harness/README.md#integration-test-runner)).

### Next: [Create and run task workers](https://github.com/conductor-sdk/conductor-csharp/blob/main/docs/readme/workers.md)
