# api-client-abstractions

Shared .NET libraries for API response envelopes, typed client execution, and ASP.NET response mapping. The repository publishes `MX.Api.Abstractions`, `MX.Api.Client`, and `MX.Api.Web.Extensions`.

## Locations

- Solution and projects: `src/MX.Api.Abstractions.sln`
- Unit tests: matching `*.Tests` projects under `src/`
- Integration tests: `src/MX.Api.IntegrationTests`
- Package and architecture guidance: `docs/`

## Commands

```pwsh
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
dotnet format src/MX.Api.Abstractions.sln --verify-no-changes
```

## Constraints

- Preserve compatibility across `ApiResponse<T>`, `ApiResult<T>`, `IApiResult<T>`, client execution, and ASP.NET mapping helpers.
- Treat all three package surfaces and their package READMEs as published contracts.
- Keep package identities, target frameworks, and `version.json` behavior unchanged unless explicitly requested.
- Build generates packages; do not publish packages during validation.

## Documentation

- [API design](docs/api-design-v2.md)
- [Consumer implementation](docs/implementing-api-consumer.md)
- [Provider implementation](docs/implementing-api-provider.md)
- [.NET support strategy](docs/dotnet-support-strategy.md)
- [Package maintenance](docs/package-maintenance.md)
