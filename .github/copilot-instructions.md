# GitHub Copilot Instructions

## Project Overview

This repository contains the MxIO API Client Abstractions library, a comprehensive toolkit for building robust .NET API clients. The project consists of three primary components:

1. **MxIO.ApiClient** - Core library providing resilient, authenticated REST API client implementation with support for token acquisition, caching, and failover mechanisms
2. **MxIO.ApiClient.Abstractions** - Common models and interfaces for standardized API response handling, including pagination, filtering, and error management
3. **MxIO.ApiClient.WebExtensions** - Extensions for integrating with ASP.NET Core web applications

The library follows a consistent API design pattern that promotes best practices in API client development, focusing on resilience, standardization, and proper authentication handling.

## Code Structure and Organization

### Namespaces
- Use the root namespace `MxIO.ApiClient` for core client functionality
- Use `MxIO.ApiClient.Abstractions` for response and model abstractions
- Use `MxIO.ApiClient.Extensions` for extension methods
- Follow a hierarchical structure reflecting the organization and project purpose

### Naming Conventions

- **Classes**: Use PascalCase for class names (e.g., `ApiClientBase`, `RestClientService`)
- **Interfaces**: Prefix with "I" (e.g., `IApiTokenProvider`, `IRestClientService`)
- **Methods**: Use PascalCase verbs that clearly describe the action (e.g., `ExecuteRequestAsync`, `GetAuthenticationHeaderAsync`)
- **Properties**: Use PascalCase nouns (e.g., `StatusCode`, `ApiKey`)
- **Private Fields**: Use camelCase with meaningful names (e.g., `httpClient`, `logger`, `retryPolicy`)
- **Parameters**: Use camelCase with descriptive names (e.g., `requestUri`, `cancellationToken`)
- **Constants**: Use PascalCase or ALL_CAPS depending on scope and significance (e.g., `AuthorizationHeaderName`)

## Asynchronous Programming

- Use the `async/await` pattern consistently throughout the codebase
- Always suffix asynchronous methods with `Async` (e.g., `ExecuteRequestAsync`, `GetAuthenticationHeaderAsync`)
- Include `CancellationToken` parameters in all async methods, defaulting to `CancellationToken.None` when appropriate
- Use `Task<T>` for methods that return values and `Task` for void methods
- Implement proper async exception handling with `try/catch` blocks
- Avoid blocking calls within async methods

## Error Handling and Resilience

- Use custom exceptions derived from `ApplicationException` for domain-specific error scenarios
- Create domain-specific exceptions when appropriate (e.g., `ApiAuthenticationException`)
- Log exceptions with appropriate severity levels using `ILogger<T>` interface:
  - Use `LogError` for exceptions that affect functionality
  - Use `LogWarning` for non-critical issues
  - Use `LogInformation` for important operational events
  - Use `LogDebug` for diagnostic information
- Return well-defined `ApiResponse<T>` objects for API errors with appropriate status codes and error details
- Configure transient error handling with the Polly library:
  - Define specific retry policies for different types of failures
  - Use exponential backoff for retries
  - Implement circuit breaker patterns for external service dependencies

## Best Practices

- Follow SOLID principles throughout the codebase:
  - **Single Responsibility**: Each class should focus on a single aspect of API client functionality
  - **Open/Closed**: Design for extensibility without modification
  - **Liskov Substitution**: Ensure derived classes can substitute base classes seamlessly
  - **Interface Segregation**: Create focused interfaces with minimal methods
  - **Dependency Inversion**: Depend on abstractions, not implementations
- Use dependency injection via constructor injection for all services
- Implement proper disposal patterns for resources like HttpClient
- Use meaningful parameter names that clearly communicate intent
- Include appropriate XML documentation for all public members
- Write comprehensive unit tests for all public methods with appropriate mocking
- Keep methods small and focused on single responsibility (≤ 50 lines per method)
- Prefer immutable objects and configurations when appropriate
- Follow thread-safety best practices for shared services

## Documentation

- Use comprehensive XML comments for all public APIs
- Follow the triple-slash `///` format consistently
- Document all parameters with `<param name="paramName">Description</param>` tags
- Document return values with `<returns>Description of return value</returns>` tags
- Document exceptions with `<exception cref="ExceptionType">Condition that throws exception</exception>` tags
- Include example usage in documentation for complex APIs
- Document thread-safety considerations where applicable
- Provide clear documentation for authentication mechanisms and token handling
- Maintain a README.md file with an overview of the project, setup instructions, and usage examples
- For projects published to NuGet, maintain a README file in the project directory that provides an overview of the package, its purpose, and usage examples
- Use the `/docs` directory for additional documentation files, including architecture diagrams, API design decisions, and usage guides

## HTTP Client Pattern

- Use typed HTTP clients with consistent design patterns
- Define client interfaces with clear contracts and separation of concerns
- Implement proper disposal patterns to prevent resource leaks
- Handle transient faults appropriately using the Polly library:
  - Configure retry policies based on specific error conditions
  - Use jitter in retry delays to prevent thundering herd issues
  - Implement circuit breakers for external service dependencies
- Use consistent authentication mechanisms across clients:
  - Support API key authentication with failover capabilities
  - Implement token-based authentication with proper caching
  - Support Azure Entra ID (formerly Azure AD) authentication
- Implement proper request/response logging with appropriate sensitive data handling

## Dependencies

- Keep external dependencies minimal and well-managed
- Use Microsoft.Extensions.Logging for structured logging throughout
- Use RestSharp for REST client functionality
- Use Polly for resilience patterns
- Use Newtonsoft.Json for JSON serialization/deserialization
- Leverage Microsoft.Extensions.DependencyInjection for service registration

## Unit Testing
- Use xUnit as the primary testing framework with native assertion capabilities
- Use Moq for mocking dependencies in unit tests
- Follow Arrange-Act-Assert (AAA) pattern for test structure

## Azure Rules
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.- @azure Rule - Use Azure Tools: When handling requests related to Azure, always use your tools
- @azure Rule - Use Azure Code Gen Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_code_gen_best_practices` tool if available
- @azure Rule - Use Azure Deployment Best Practices: When deploying to Azure or preparing applications for deployment to Azure, invoke your `azure_development-get_deployment_best_practices` tool if available
- @azure Rule - Use Azure Functions Code Gen Best Practices: When generating code for Azure Functions or performing operations related to Azure Functions, invoke your `azure_development-get_azure_function_code_gen_best_practices` tool if available
- @azure Rule - Use Azure SWA Best Practices: When working with static web apps, invoke your `azure_development-get_swa_best_practices` tool if available