# GitHub Copilot Instructions

This document provides guidance on coding standards, patterns, and practices for this project. Following these instructions will ensure consistency when working with GitHub Copilot.

## Project Overview

This is a .NET library project focusing on standardization and clean architecture principles.

## Naming Conventions

- **Namespaces**: Follow a hierarchical structure reflecting the organization and project
- **Classes**: Use PascalCase for class names (e.g., `ApiClientBase`, `ResponseDto`)
- **Interfaces**: Prefix with "I" (e.g., `IClient`, `IAuthenticationProvider`)
- **Methods**: Use PascalCase verbs (e.g., `GetAsync`, `CreateAsync`)
- **Properties**: Use PascalCase nouns (e.g., `StatusCode`, `RequestUrl`)
- **Private Fields**: Use camelCase with underscore prefix (e.g., `_httpClient`, `_logger`)
- **Parameters**: Use camelCase (e.g., `requestUri`, `cancellationToken`)

## Project Structure

- **Abstract Base Classes**: Place in appropriate namespaces reflecting their purpose
- **DTOs**: Define in a `.Dto` suffixed namespace
- **Interfaces**: Group by functionality
- **Extensions**: Place in an `.Extensions` namespace

## Testing

- **Test Framework**: Use xUnit for all test projects
- **Test Project Naming**: Suffix with `.Tests`
- **Test Class Naming**: Suffix class name with `Tests` (e.g., `ServiceTests`)
- **Test Method Naming**: Follow the `Should_ExpectedBehavior_When_StateUnderTest` pattern
- **Mocking Framework**: Use Moq for mocking interfaces and dependencies
- **Test Fixtures**: Use xUnit fixtures for shared context setup

## Error Handling

- Use custom exceptions derived from `ApplicationException` 
- Create domain-specific exceptions when appropriate
- Log exceptions with appropriate severity levels
- Return well-defined response objects for API errors

## Documentation

- Use XML comments for public APIs
- Follow the triple-slash `///` format
- Document parameters with `<param>` tags
- Document return values with `<returns>` tags
- Document exceptions with `<exception>` tags

## Dependencies

- Keep external dependencies minimal
- Use Microsoft.Extensions.Logging for logging
- Use System.Net.Http for HTTP communications
- Use System.Text.Json for JSON serialization/deserialization

## Asynchronous Programming

- Use the `async/await` pattern consistently
- Suffix asynchronous methods with `Async`
- Accept `CancellationToken` parameters in all async methods
- Use `Task<T>` for methods that return values and `Task` for methods that don't

## Best Practices

- Follow SOLID principles
- Use dependency injection
- Create immutable DTOs where possible
- Use meaningful parameter names
- Include appropriate XML documentation
- Write unit tests for all public methods
- Keep methods small and focused on single responsibility

## API Response Pattern

- Use consistent response DTOs with common properties:
  - `IsSuccessful` (bool)
  - `StatusCode` (HttpStatusCode)
  - `ErrorMessage` (string, optional)
  - `ValidationErrors` (Dictionary<string, string[]>, optional)
  - Generic type parameter for the response payload

## HTTP Client Pattern

- Use typed HTTP clients
- Define client interfaces with clear contracts
- Implement proper disposal patterns
- Handle transient faults appropriately
- Use consistent authentication mechanisms
