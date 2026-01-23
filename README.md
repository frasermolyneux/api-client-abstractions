# MX API Abstractions
> Unified .NET abstractions, clients, and ASP.NET extensions for standardized, resilient API integrations.

## ⚙️ Workflows
[![Code Quality (Sonar + CodeQL)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/code-quality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/code-quality.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml)
[![Dependabot Auto-Merge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml)
[![Feature Branch Preview Publish](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-preview-ci.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-preview-ci.yml)
[![Main Branch Build and Tag](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/main-branch-build-and-tag.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/main-branch-build-and-tag.yml)
[![PR Validation (CI Only)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-validation.yml)
[![Publish Tagged Build to NuGet](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/publish-tagged-build.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/publish-tagged-build.yml)

## 📌 Overview
MX API Abstractions packages response envelopes, a RestSharp-based client stack, and ASP.NET Core helpers so every team can build and consume APIs with identical conventions. Docs under `docs/` capture the cross-cutting decisions for providers, consumers, versioned clients, and package maintenance so updates stay coordinated.

## 🧱 Technology & Frameworks
- .NET 9.0 & .NET 10.0 – Multi-targeted across every package
- Azure.Identity 1.17.x – Entra ID authentication and credential orchestration
- RestSharp 113 + Polly 8.6 – Resilient HTTP client pipeline with retries and caching hooks
- ASP.NET Core 9/10 + MX.Api.Web.Extensions – Consistent controller and HTTP result mapping

## 📚 Documentation Index
- [docs/dotnet-support-strategy.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/dotnet-support-strategy.md) – .NET 9/10 target framework policy, dependency management, and CI/CD configuration.
- [docs/api-design-v2.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/api-design-v2.md) – Routing, filters, pagination, and response envelope reference.
- [docs/implementing-api-consumer.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-api-consumer.md) – End-to-end guidance for resilient API consumers.
- [docs/implementing-api-provider.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-api-provider.md) – Controller, response, and error-handling patterns for providers.
- [docs/implementing-versioned-api-client.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-versioned-api-client.md) – Structuring multi-version clients with shared options/builders.
- [docs/package-maintenance.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/package-maintenance.md) – Dependabot flow and manual NuGet update process.

## 🚀 Getting Started
**Highlights**
- `ApiResponse<T>`, `ApiResult<T>`, and `CollectionModel<T>` keep contracts uniform across APIs and clients.
- RestSharp clients layer Polly retries plus multiple authentication schemes stitched through `IApiTokenProvider`.
- `MX.Api.Web.Extensions` turns provider responses or consumer results into ASP.NET Core `IActionResult` instances with matching headers.

**Sample Usage (optional)**
```csharp
// Program.cs
builder.Services.AddApiClient<IMyApiClient, MyApiClient>(options =>
{
    options.WithBaseUrl("https://api.example.com")
           .WithSubscriptionKey("apim-key")
           .WithEntraIdAuthentication("api://backend-api");
});

public interface IMyApiClient
{
    Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default);
}

public class MyApiClient : BaseApi, IMyApiClient
{
    public MyApiClient(
        ILogger<BaseApi<ApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        ApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<User>();
    }
}
```

## 🛠️ Developer Quick Start
```shell
git clone https://github.com/frasermolyneux/api-client-abstractions.git
cd api-client-abstractions
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter FullyQualifiedName!~IntegrationTests
dotnet test src/MX.Api.Abstractions.sln --filter FullyQualifiedName~IntegrationTests
```

## 🤝 Contributing
Please read the [contributing](https://github.com/frasermolyneux/api-client-abstractions/blob/main/CONTRIBUTING.md) guidance; this is a learning and development project.

## 🔐 Security
Please read the [security](https://github.com/frasermolyneux/api-client-abstractions/blob/main/SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.

## 📄 License
Distributed under the [GNU General Public License v3.0](https://github.com/frasermolyneux/api-client-abstractions/blob/main/LICENSE).
