# GitHub Copilot Instructions

## Big Picture
- `src/MX.Api.Abstractions.sln` stitches together three NuGet packages: `MX.Api.Abstractions` (DTOs, envelopes, pagination), `MX.Api.Client` (RestSharp-based consumers with Polly and token orchestration), and `MX.Api.Web.Extensions` (ASP.NET Core glue that turns `ApiResult<T>` into `IActionResult`). Keep changes scoped to the package that owns the behavior so upgrades remain independent.
- Data flows follow a strict pattern: controllers/services build `ApiResponse<T>` models (see `src/MX.Api.Abstractions`), clients wrap HTTP output in `ApiResult<T>` (see `src/MX.Api.Client/RestResponseExtensions.cs`), and web apps call `ToHttpResult()` from `src/MX.Api.Web.Extensions` to surface consistent status codes.
- Documentation in `docs/api-design-v2.md`, `docs/implementing-api-consumer.md`, and `docs/implementing-versioned-api-client.md` explains why these abstractions exist; mirror those patterns rather than inventing new response shapes.

## Client Implementation Patterns
- New clients must inherit from `src/MX.Api.Client/BaseApi*.cs`. Use `CreateRequestAsync` + `ExecuteAsync` and let `IApiTokenProvider` (from `MX.Api.Client.Auth`) add headers—never hand-build `RestClient` instances.
- Register clients via `ServiceCollectionExtensions`: `AddApiClient<TInterface, TImpl>` when default `ApiClientOptions` is enough; `AddTypedApiClient<...Options, ...Builder>` when you need custom option properties/validation (examples in `src/MX.Api.Client/README.md`).
- Centralize configuration in option builders (`Configuration/ApiClientOptionsBuilder.cs`) and call `Validate()` for required fields. Consumers expect fluent methods such as `WithBaseUrl`, `WithSubscriptionKey`, and `WithEntraIdAuthentication` to be available.
- Place reusable request helpers under `MX.Api.Client.Extensions` (e.g., `RestResponseExtensions.ToApiResponse<T>()`) so pagination, metadata, and error parsing stay uniform.

## Abstractions & Web Integration
- `MX.Api.Abstractions` owns `ApiResponse`, `ApiResponse<T>`, `ApiResult<T>`, `CollectionModel<T>`, `ApiPagination`, and `FilterOptions` (see `src/MX.Api.Abstractions/README.md`). Keep these classes POCO-only—no behavior beyond serialization helpers.
- When exposing lists, populate both `CollectionModel<T>.Items` and `ApiResponse.Pagination` so `MX.Api.Web.Extensions` can emit paging headers (see `src/MX.Api.Web.Extensions/README.md`).
- Controllers or BFFs should never inspect raw `RestResponse`; convert to `ApiResult` first and call `ToHttpResult()` / `ToApiResult(HttpStatusCode)` from `MX.Api.Web.Extensions` to map to MVC results consistently.

## Authentication & Configuration
- `ApiClientOptions` (and typed derivatives) support layered auth: `WithApiKeyAuthentication`, `WithSubscriptionKey` for APIM, `WithBearerToken`, and `WithEntraIdAuthentication` that chains to `Azure.Identity` (examples in `src/MX.Api.Client/README.md`). You can enable multiple schemes simultaneously—the request builder will emit every configured header.
- For advanced scenarios, define custom options (see `MX.Api.IntegrationTests/UserApiOptions`) and builders so clients can add domain-specific knobs (`WithApiVersion`, caching hints, etc.) while still being validated via `Validate()`.
- Named clients use `IOptionsSnapshot<ApiClientOptions>` plus the `BaseApi` constructor overload that accepts the name; follow that approach when registering multiple backends in the same app.

## Testing & Tooling
- Standard workflow: `dotnet build src/MX.Api.Abstractions.sln` then `dotnet test src/MX.Api.Abstractions.sln --filter FullyQualifiedName!~IntegrationTests`; integration runs (`--filter FullyQualifiedName~IntegrationTests`) target the sandbox APIs in `src/MX.Api.IntegrationTests`.
- Integration strategy is dual: DI/service-registration tests ensure `AddApiClient`/`AddTypedApiClient` wiring works, while HTTP tests hit the in-memory APIs directly because RestSharp cannot talk to `TestServer` (documented in `src/MX.Api.IntegrationTests/README.md`). When adding features, touch both layers where applicable.
- Use `scripts/update-nuget-packages.ps1` when bumping dependencies; it runs `dotnet-outdated`, a Release build, and tests across all target frameworks so lockfiles stay in sync with CI expectations.
- Projects multi-target `net9.0` + `net10.0`. New code must compile for both runtimes and avoid APIs that only exist in a single target unless wrapped with `#if` guards in `Directory.Build.props`.

## Versioning & CI
- Nerdbank.GitVersioning (`version.json`) governs package, assembly, and git tag versions. CI workflows reuse the shared composites from `frasermolyneux/actions` so preview builds get `-preview.g<sha>` automatically and tagged builds publish to NuGet without rebuilding.
- Keep `Directory.Build.props` authoritative for analyzers and nullable/context settings; do not override them in individual projects unless coordinated, or CI style checks will fail.

## Reference Material
- Key guides live in `docs/`: API consumer/provider walkthroughs, versioned client playbook, package maintenance, and EF regeneration instructions. Link to them from PRs when introducing patterns so future contributors can find precedent quickly.

## Azure Rules
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.
- @azure Rule - Use Azure Tools: When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Code Gen Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_code_gen_best_practices` tool if available.
- @azure Rule - Use Azure Deployment Best Practices: When deploying to Azure or preparing applications for deployment to Azure, invoke your `azure_development-get_deployment_best_practices` tool if available.
- @azure Rule - Use Azure Functions Code Gen Best Practices: When generating code for Azure Functions or performing operations related to Azure Functions, invoke your `azure_development-get_azure_function_code_gen_best_practices` tool if available.
- @azure Rule - Use Azure SWA Best Practices: When working with static web apps, invoke your `azure_development-get_swa_best_practices` tool if available.