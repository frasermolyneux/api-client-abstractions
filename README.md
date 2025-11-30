# MX API Abstractions
> Unified .NET abstractions, clients, and ASP.NET extensions for standardized, resilient API integrations.

[![Code Quality (Sonar + CodeQL)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/code-quality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/code-quality.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml)
[![Dependabot Auto-Merge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml)
[![Feature Branch Preview Publish](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-preview-ci.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-preview-ci.yml)
[![Main Branch Build and Tag](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/main-branch-build-and-tag.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/main-branch-build-and-tag.yml)
[![PR Validation (CI Only)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-validation.yml)
[![Publish Tagged Build to NuGet](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/publish-tagged-build.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/publish-tagged-build.yml)

## 📌 Overview
MX API Abstractions packages common response models, a RestSharp-based client stack, and ASP.NET Core helpers so every team can build and consume APIs with identical conventions. The docs under `docs/` (for example, [implementing providers](docs/implementing-api-provider.md) and the [design pattern overview](docs/api-design-v2.md)) explain the architectural decisions that keep clients, APIs, and infrastructure aligned.

## ⚙️ Workflow Status
| Workflow                         | Status                                                                                                                          | Purpose                                                           |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `Code Quality (Sonar + CodeQL)`  | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/code-quality.yml/badge.svg)`              | `SonarCloud and CodeQL scanning for every push.`                  |
| `Copilot Setup Steps`            | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml/badge.svg)`       | `Validates the Copilot bootstrap steps when they change.`         |
| `Dependabot Auto-Merge`          | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml/badge.svg)`      | `Auto-merges passing Dependabot PRs.`                             |
| `Feature Branch Preview Publish` | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-preview-ci.yml/badge.svg)`        | `Builds feature/* branches and publishes preview NuGet packages.` |
| `Main Branch Build and Tag`      | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/main-branch-build-and-tag.yml/badge.svg)` | `Builds main, calculates NBGV versions, and tags releases.`       |
| `PR Validation (CI Only)`        | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-validation.yml/badge.svg)`             | `Runs dependency review plus CI for pull requests.`               |
| `Publish Tagged Build to NuGet`  | `![badge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/publish-tagged-build.yml/badge.svg)`      | `Publishes tagged artifacts to NuGet and creates releases.`       |

## 🧱 Technology & Frameworks
- `.NET 9.0 and .NET 10.0 (multi-targeted across every package)`
- `Azure.Identity 1.17.x for Entra ID and credential orchestration`
- `RestSharp 113 + Polly 8.6 for resilient HTTP pipelines`
- `ASP.NET Core 9/10 with MX.Api.Web.Extensions for HTTP result mapping`

## 📚 Documentation Index
- [docs/api-design-v2.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/api-design-v2.md) – Reference for standardized routing, filters, pagination, and response envelopes.
- [docs/implementing-api-consumer.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-api-consumer.md) – End-to-end guide for building resilient API consumers.
- [docs/implementing-api-provider.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-api-provider.md) – Patterns for controllers, responses, and error handling when exposing APIs.
- [docs/implementing-versioned-api-client.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/implementing-versioned-api-client.md) – How to structure multi-version clients with shared options.
- [docs/package-maintenance.md](https://github.com/frasermolyneux/api-client-abstractions/blob/main/docs/package-maintenance.md) – Dependabot flow plus the manual NuGet update script.

## 🚀 Getting Started
**Highlights**
- `ApiResponse<T>`, `ApiResult<T>`, and `CollectionModel<T>` keep contracts consistent across services.
- RestSharp clients layer Polly retries, caching hooks, and multiple authentication schemes.
- MX.Api.Web.Extensions transforms client/server responses into ASP.NET Core HTTP results with matching headers.

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
# Optional integration coverage
dotnet test src/MX.Api.Abstractions.sln --filter FullyQualifiedName~IntegrationTests
```

## 🤝 Contributing
Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## 🔐 Security
Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.

## 📄 License
Distributed under the [GNU General Public License v3.0](https://github.com/frasermolyneux/.github-prompts/blob/main/LICENSE).
