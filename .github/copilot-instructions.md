# GitHub Copilot Instructions

## Project Overview

This repository packages the MX API Abstractions approach, a toolkit for building resilient .NET API consumers and web integrations. It contains:

1. **MX.Api.Abstractions** – shared DTOs, envelopes, pagination helpers, and validation contracts.
2. **MX.Api.Client** – REST clients with configurable authentication, retry, and logging behaviors.
3. **MX.Api.Web.Extensions** – ASP.NET Core helpers for wiring responses into MVC/Razor apps.

## Repository Layout & Namespaces
- `src/MX.Api.Abstractions` exposes canonical types such as `ApiError`, `ApiPagination`, and `ApiResponse<T>`; keep new models under this namespace so they can ship independently.
- `src/MX.Api.Client` hosts client infrastructure. Use `MX.Api.Client` for core types, `MX.Api.Client.Auth` for token providers, and `MX.Api.Client.Extensions` for helper methods.
- `src/MX.Api.Web.Extensions` carries MVC extensions (`ResultExtensions`, `ApiProblemDetailsMapper`) that depend on abstractions but not on concrete client implementations.
- Tests live alongside their production counterparts (`MX.Api.Abstractions.Tests`, `MX.Api.Client.Tests`, `MX.Api.Web.Extensions.Tests`, plus `MX.Api.IntegrationTests` for end-to-end coverage). Mirror this layout when adding new projects.

## Client Implementation Patterns
- Derive new clients from `src/MX.Api.Client/BaseApi.cs`; it already wires `ILogger`, `IApiTokenProvider`, `IRestClientService`, retry policies, and authentication header injection. Override only the resource-specific logic.
- `RestClientService.cs` centralizes `RestSharp` configuration. Reuse it instead of instantiating `RestClient` directly so telemetry and retry hooks remain consistent.
- Authentication helpers live in `MX.Api.Client.Auth` (`ApiTokenProvider`, `ClientCredentialProvider`, `DefaultTokenCredentialProvider`). Extend these rather than embedding custom token code per client.
- Options classes inherit from `Configuration/ApiClientOptionsBase`. Override `Validate()` to enforce per-client settings; `BaseApi` will call it at construction time.
- Store reusable request/response helpers under `MX.Api.Client.Extensions` to avoid duplicating serialization or header logic.

## Abstractions & Web Extensions
- DTOs under `MX.Api.Abstractions` should remain serialization-friendly (no behavior, only state). Use nested namespaces (e.g., `.Responses`, `.Requests`) to keep contracts discoverable.
- `src/MX.Api.Web.Extensions` exposes helpers like `ApiResponseExtensions` and `HttpResponseExtensions` so MVC apps can translate `ApiResponse<T>` objects into consistent `IActionResult`s. Add new rendering patterns there instead of duplicating them inside sites.
- Keep documentation artifacts in `docs/` (e.g., `api-design-v2.md`, `implementing-versioned-api-client.md`). Update them whenever client surface areas change.

## Testing & Validation
- Unit tests live in the `*.Tests` projects named above; reuse existing fixtures and builders when adding coverage for new clients or abstractions.
- Integration tests in `MX.Api.IntegrationTests` target deployed endpoints. When introducing breaking API changes, add or update these tests to catch regressions before publishing packages.

## Azure Rules
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.
- @azure Rule - Use Azure Tools: When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Code Gen Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_code_gen_best_practices` tool if available.
- @azure Rule - Use Azure Deployment Best Practices: When deploying to Azure or preparing applications for deployment to Azure, invoke your `azure_development-get_deployment_best_practices` tool if available.
- @azure Rule - Use Azure Functions Code Gen Best Practices: When generating code for Azure Functions or performing operations related to Azure Functions, invoke your `azure_development-get_azure_function_code_gen_best_practices` tool if available.
- @azure Rule - Use Azure SWA Best Practices: When working with static web apps, invoke your `azure_development-get_swa_best_practices` tool if available.