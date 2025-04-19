# GitHub Copilot Instructions

This document provides guidance on coding standards, patterns, and practices for this project. Following these instructions will ensure consistency when working with GitHub Copilot.

## Copilot Directives

- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.

## Naming Conventions for .NET

- **Namespaces**: Follow a hierarchical structure reflecting the organization and project
- **Classes**: Use PascalCase for class names (e.g., `ApiClientBase`, `ResponseDto`)
- **Interfaces**: Prefix with "I" (e.g., `IClient`, `IAuthenticationProvider`)
- **Methods**: Use PascalCase verbs (e.g., `GetClient`, `CreateClient`)
- **Properties**: Use PascalCase nouns (e.g., `StatusCode`, `RequestUrl`)
- **Private Fields**: Use camelCase (e.g., `httpClient`, `logger`)
- **Parameters**: Use camelCase (e.g., `requestUri`, `cancellationToken`)

## Asynchronous Programming

- Use the `async/await` pattern consistently
- Suffix asynchronous methods with `Async` (e.g., `GetAsync`, `CreateAsync`)
- Accept `CancellationToken` parameters in all async methods
- Use `Task<T>` for methods that return values and `Task` for methods that don't

## Error Handling

- Use custom exceptions derived from `ApplicationException` 
- Create domain-specific exceptions when appropriate
- Log exceptions with appropriate severity levels
- Return well-defined response objects for API errors
- Transient errors should be handled with retries through the Polly library

## Best Practices

- Follow SOLID principles
- Use dependency injection
- Use meaningful parameter names
- Include appropriate XML documentation
- Write unit tests for all public methods
- Keep methods small and focused on single responsibility

## Documentation

- Use XML comments for public APIs
- Follow the triple-slash `///` format
- Document parameters with `<param>` tags
- Document return values with `<returns>` tags
- Document exceptions with `<exception>` tags

## HTTP Client Pattern

- Use typed HTTP clients
- Define client interfaces with clear contracts
- Implement proper disposal patterns
- Handle transient faults appropriately using the Polly library
- Use consistent authentication mechanisms

## Dependencies

- Keep external dependencies minimal
- Use Microsoft.Extensions.Logging for logging