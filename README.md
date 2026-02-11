# MX API Abstractions
[![Build and Test](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/build-and-test.yml)
[![PR Verify](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pr-verify.yml)
[![Code Quality](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml)
[![Release - Version and Tag](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-version-and-tag.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-version-and-tag.yml)
[![Release - Publish NuGet](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-publish-nuget.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-publish-nuget.yml)
[![Dependabot Auto-Merge](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/dependabot-automerge.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/copilot-setup-steps.yml)

## Documentation
* [Development Workflows](docs/development-workflows.md) - Branch strategy, CI/CD triggers, and development flows
* [API Design V2](docs/api-design-v2.md) - Routing, filters, pagination, and response envelope reference
* [Implementing API Consumer](docs/implementing-api-consumer.md) - Guidance for resilient API consumers
* [Implementing API Provider](docs/implementing-api-provider.md) - Controller, response, and error-handling patterns
* [Implementing Versioned API Client](docs/implementing-versioned-api-client.md) - Structuring multi-version clients with shared options and builders
* [Package Maintenance](docs/package-maintenance.md) - Dependabot flow and manual NuGet update process
* [Dotnet Support Strategy](docs/dotnet-support-strategy.md) - Target framework policy and dependency management

## Overview
MX API Abstractions delivers shared response envelopes, RestSharp-based API clients, and ASP.NET Core extensions so providers and consumers follow the same conventions. Packages multi-target net9.0 and net10.0, layering Polly retries and Entra ID or API key authentication through a configurable options builder. Documentation in docs/ captures API design patterns, client/provider guidance, and release practices to keep integrations consistent.

## Contributing
Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security
Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
