# Copilot Instructions

This repository publishes the shared `MX.Api.*` libraries used for response envelopes, typed API clients, and ASP.NET response translation.

## Runtime and layout

- SDK: `10.0.301` from `global.json`; package and test projects target `net9.0` and `net10.0`.
- Solution: `src/MX.Api.Abstractions.sln`.
- Packages: `src/MX.Api.Abstractions`, `src/MX.Api.Client`, and `src/MX.Api.Web.Extensions`.
- Unit tests use matching `*.Tests` projects; integration coverage is isolated in `src/MX.Api.IntegrationTests`.

## Repository rules

- Keep envelope behavior consistent across `ApiResponse<T>`, `ApiResult<T>`, and `IApiResult<T>`.
- Centralize client execution changes in `BaseApi.cs`; keep authentication, DI registration, request creation, retries, and response mapping aligned.
- Keep ASP.NET translation helpers consistent with the shared envelope contract.
- Public types, interfaces, serialization behavior, and testing helpers are consumer-facing contracts.
- Package IDs, target frameworks, generated package metadata, and NBGV configuration in `version.json` are release boundaries.
- Never add credentials or publish packages as part of routine validation.

## Validation

```pwsh
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
dotnet format src/MX.Api.Abstractions.sln --verify-no-changes
```

Run integration tests only when the changed behavior requires them. Detailed design and maintenance guidance is in `docs/`.
