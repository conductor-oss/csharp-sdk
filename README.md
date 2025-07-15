# Conductor OSS C# SDK

[![CI](https://github.com/conductor-oss/csharp-sdk/actions/workflows/pull_request.yml/badge.svg)](https://github.com/conductor-oss/csharp-sdk/actions)
[![Coverage](https://codecov.io/gh/conductor-oss/csharp-sdk/branch/main/graph/badge.svg?token=AXNN6NU99Y)](https://codecov.io/gh/conductor-oss/csharp-sdk)

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

### Next: [Create and run task workers](https://github.com/conductor-sdk/conductor-csharp/blob/main/docs/readme/workers.md)
