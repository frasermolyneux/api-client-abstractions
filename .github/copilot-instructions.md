# GitHub Copilot Instructions

This document provides guidance on coding standards, patterns, and practices for this project. Following these instructions will ensure consistency when working with GitHub Copilot.

## Copilot Directives

- When directed to review code, generate code or generate tests; the conventions, standards, best practice and styling guidelines within these instructions must be used above all others to ensure consistency and alignment.

- When directed to review code, generate code or generate tests; Upon completion unit tests for the solution should be executed to validate the changes have been implemented without breaking tested functionality.

- When completing any build or test execution the output should be checked for any warnings or errors; if any exist they should be resolved.

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