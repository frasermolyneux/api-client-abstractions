using System.Net;
using MX.Api.Client.Configuration;
using MX.Api.Client.Testing;
using MX.Api.Client.Tests.TestClients;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests.Testing;

/// <summary>
/// Integration tests demonstrating how consumers can use testing extensions
/// </summary>
public class TestingExtensionsIntegrationTests
{
    [Fact]
    public void AddTestApiClient_RegistersClientWithTestDoubles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var testService = services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options.WithBaseUrl("https://test.example.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verify the client is registered
        var client = serviceProvider.GetService<IStandardTestApiClient>();
        Assert.NotNull(client);

        // Verify the test service was returned
        Assert.NotNull(testService);
    }

    [Fact]
    public async Task AddTestApiClient_ClientUsesInMemoryService()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                testService.AddResponse("test-resource", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\", \"value\": \"test\"}}"
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IStandardTestApiClient>();

        // Act
        var result = await client.GetTestResourceAsync("123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(testService.WasCalled("test-resource"));
    }

    [Fact]
    public async Task AddTestApiClient_WithEntraIdAuth_UsesFakeTokenProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options
                .WithBaseUrl("https://test.example.com")
                .WithEntraIdAuthentication("api://test-api"),
            testService =>
            {
                testService.AddResponse("test-resource", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\"}}"
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IStandardTestApiClient>();

        // Act
        var result = await client.GetTestResourceAsync("123");

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify token provider was set up
        var tokenProvider = serviceProvider.GetService<Auth.IApiTokenProvider>();
        Assert.NotNull(tokenProvider);
        Assert.IsType<FakeApiTokenProvider>(tokenProvider);
    }

    [Fact]
    public void UseInMemoryRestClientService_ReplacesExistingService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRestClientService, RestClientService>(); // Real service

        // Act
        var testService = services.UseInMemoryRestClientService(service =>
        {
            service.AddResponse("test", new RestResponse { StatusCode = HttpStatusCode.OK });
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var restClientService = serviceProvider.GetRequiredService<IRestClientService>();
        
        Assert.IsType<InMemoryRestClientService>(restClientService);
        Assert.Same(testService, restClientService);
    }

    [Fact]
    public void UseFakeApiTokenProvider_ReplacesExistingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Auth.IApiTokenProvider, Auth.ApiTokenProvider>(); // Real provider

        // Act
        var fakeProvider = services.UseFakeApiTokenProvider(provider =>
        {
            provider.SetToken("api://test", "test-token");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var tokenProvider = serviceProvider.GetRequiredService<Auth.IApiTokenProvider>();
        
        Assert.IsType<FakeApiTokenProvider>(tokenProvider);
        Assert.Same(fakeProvider, tokenProvider);
    }

    [Fact]
    public async Task AddTestTypedApiClient_WorksWithCustomOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = services.AddTestTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://test.example.com")
                .WithTestFeature(),
            testService =>
            {
                testService.AddResponse("custom-endpoint", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"custom\": true}}"
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ITestApiClient>();

        // Act
        var result = await client.GetCustomDataAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(testService.WasCalled("custom-endpoint"));
    }

    [Fact]
    public async Task MultipleTestClients_ShareSameTestService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // When registering multiple test clients, they will share the same InMemoryRestClientService
        // Configure all responses on one service
        var testService = services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options.WithBaseUrl("https://api1.example.com"),
            service =>
            {
                service.AddResponse("test-resource", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"1\"}}"
                });
                
                // Also configure responses for the second client
                service.AddResponse("custom-endpoint", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"custom\": true}}"
                });
            });

        // Register second client - it will use the same test service
        services.AddTestTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options.WithBaseUrl("https://api2.example.com"));

        var serviceProvider = services.BuildServiceProvider();
        var client1 = serviceProvider.GetRequiredService<IStandardTestApiClient>();
        var client2 = serviceProvider.GetRequiredService<ITestApiClient>();

        // Act
        var result1 = await client1.GetTestResourceAsync("1");
        var result2 = await client2.GetCustomDataAsync();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        // Both clients share the same test service
        Assert.True(testService.WasCalled("test-resource"));
        Assert.True(testService.WasCalled("custom-endpoint"));
    }

    [Fact]
    public async Task TestClient_SupportsErrorScenarios()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                // Return 404 which is treated as success by BaseApi but indicates resource not found
                testService.AddResponse("test-resource", new RestResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = "{\"errors\": [{\"code\": \"NOT_FOUND\", \"message\": \"Resource not found\"}]}"
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IStandardTestApiClient>();

        // Act
        var result = await client.GetTestResourceAsync("123");

        // Assert - NotFound is a success status code for BaseApi but indicates no data
        Assert.True(result.IsNotFound);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.True(testService.WasCalled("test-resource"));
    }

    [Fact]
    public async Task TestClient_SupportsDynamicResponses()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTestApiClient<IStandardTestApiClient, StandardTestApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                testService.AddResponseFunction("test-resource", request =>
                {
                    var idParam = request.Parameters.FirstOrDefault(p => p.Name == "id");
                    var id = idParam?.Value?.ToString() ?? "unknown";

                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = $"{{\"data\": {{\"id\": \"{id}\"}}}}"
                    };
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IStandardTestApiClient>();

        // Act
        var result1 = await client.GetTestResourceAsync("123");
        var result2 = await client.GetTestResourceAsync("456");

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal("123", result1.Result?.Data?.Id);
        Assert.Equal("456", result2.Result?.Data?.Id);
    }
}
