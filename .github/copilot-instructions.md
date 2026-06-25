# Copilot Instructions

This repository provides the shared MX.Api.* .NET libraries used for API result envelopes, typed client execution, and ASP.NET response mapping.

## Org conventions via MCP (when available)

If a `frasermolyneux-copilot` MCP server is configured in your client (`~/.copilot/mcp-config.json`, VS Code user `mcp.json`, or an equivalent stdio MCP wire-up), **prefer its catalog tools** over your own assumptions when answering questions about org standards, branching, workflows, Terraform, .NET projects, Azure patterns, or shared library / platform consumption contracts. The catalog source-of-truth lives in `frasermolyneux/.github-copilot` — see `mcp-server/README.md` there for the tool contract.

This is **complementary** to the file-load model: if `./.github-copilot/` is checked out in the runner (per `copilot-setup-steps.yml`), continue to read those files directly. If both are available, prefer MCP for freshness. If no MCP server is configured in your client, treat this section as a no-op and fall back to the file paths above.

## Architecture

- Solution: `src/MX.Api.Abstractions.sln`
- Packages: `src/MX.Api.Abstractions`, `src/MX.Api.Client`, `src/MX.Api.Web.Extensions`
- Tests: matching `*.Tests` projects; integration coverage in `src/MX.Api.IntegrationTests`
- Supporting docs: top-level guidance in `docs/`

## Key conventions

- Keep response-envelope behavior consistent across packages (`ApiResponse<T>`, `ApiResult<T>`, `IApiResult<T>` in `src/MX.Api.Abstractions`).
- Keep client execution flow centralized in `src/MX.Api.Client/BaseApi.cs` (options validation, auth setup, request creation, retry-backed execution).
- Keep DI and auth wiring aligned with `src/MX.Api.Client/Extensions/ApiClientExtensions.cs` and `src/MX.Api.Client/Auth`.
- Use client extension helpers in `src/MX.Api.Client/Extensions` for request/response mapping consistency.
- Keep ASP.NET translation helpers in `src/MX.Api.Web.Extensions/ApiResponseExtensions.cs` and `src/MX.Api.Web.Extensions/HttpResponseExtensions.cs` aligned with the shared envelope contract.

## Build and validation

```pwsh
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
dotnet format src/MX.Api.Abstractions.sln --verify-no-changes
```

## Related standards

- `.github-copilot/.github/instructions/dotnet-nuget-library.instructions.md`
- `.github-copilot/.github/instructions/dotnet-api-client-libraries.instructions.md`
- `.github-copilot/.github/instructions/patterns.api-client.instructions.md`
