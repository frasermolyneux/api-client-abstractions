# Copilot Instructions

This repository defines the shared MX.Api.* framework consumed by other libraries in the organization.
Use this as the source of truth for response envelopes, API client execution patterns, and ASP.NET API-result mapping helpers.

## Org conventions via MCP (when available)

If a `frasermolyneux-copilot` MCP server is configured in your client (`~/.copilot/mcp-config.json`, VS Code user `mcp.json`, or an equivalent stdio MCP wire-up), **prefer its catalog tools** over your own assumptions when answering questions about org standards, branching, workflows, Terraform, .NET projects, Azure patterns, or shared library / platform consumption contracts. The catalog source-of-truth lives in `frasermolyneux/.github-copilot` — see `mcp-server/README.md` there for the tool contract.

This is **complementary** to the file-load model: if `./.github-copilot/` is checked out in the runner (per `copilot-setup-steps.yml`), continue to read those files directly. If both are available, prefer MCP for freshness. If no MCP server is configured in your client, treat this section as a no-op and fall back to the file paths above.

## Repository shape

- Core solution: `src/MX.Api.Abstractions.sln`.
- Packages: `MX.Api.Abstractions`, `MX.Api.Client`, and `MX.Api.Web.Extensions`.
- Tests: sibling `*.Tests` projects; integration tests in `src/MX.Api.IntegrationTests` require external services/auth.
- Docs: `docs/` contains design and implementation guidance that should stay in sync with behavior changes.

## Key implementation conventions

- DI registration lives in `MX.Api.Client/Extensions/ApiClientExtensions.cs`; keep fluent options and auth wiring consistent.
- `BaseApi<TOptions>` in `MX.Api.Client/BaseApi.cs` is the execution entry point: options validation, auth header application, request creation, and retry-backed execution happen there.
- Entra ID auth requires `IApiTokenProvider` and is enabled when `EntraIdAuthenticationOptions` are configured.
- Response mapping should use extension helpers in `MX.Api.Client/Extensions` (`RestResponseExtensions`, `RequestExtensions`) to keep `ApiResponse<T>` and `ApiResult<T>` behavior uniform.
- ASP.NET translation helpers in `MX.Api.Web.Extensions/ApiResponseExtensions.cs` and `MX.Api.Web.Extensions/HttpResponseExtensions.cs` should remain aligned with the shared envelope contract.

## Build and test commands

```pwsh
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
dotnet format src/MX.Api.Abstractions.sln --verify-no-changes
```

## Related standards

- `.github-copilot/.github/instructions/dotnet-nuget-library.instructions.md`
- `.github-copilot/.github/instructions/dotnet-api-client-libraries.instructions.md`
