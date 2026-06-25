using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.Client.Tests.TestClients;
using Xunit;

namespace MX.Api.Client.Tests.Extensions;

public class ApiClientExtensionsTests
{
    [Fact]
    public void AddTypedApiClient_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required logging services for DI resolution
        _ = services.AddLogging();

        // Act
        var result = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api.example.com"));

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Verify IMemoryCache is registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());

        // Verify IRestClientService is registered
        Assert.NotNull(serviceProvider.GetService<IRestClientService>());

        // Verify the options are registered
        Assert.NotNull(serviceProvider.GetService<TestApiOptions>());

        // Verify the client is registered
        Assert.NotNull(serviceProvider.GetService<ITestApiClient>());
        _ = Assert.IsType<TestApiClient>(serviceProvider.GetService<ITestApiClient>());
    }

    [Fact]
    public void AddTypedApiClient_ConfiguresOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        var baseUrl = "https://api.example.com";
        var apiKey = "test-api-key";

        // Act
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiKey)
            .WithTestFeature());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<TestApiOptions>();

        Assert.Equal(baseUrl, configuredOptions.BaseUrl);
        Assert.True(configuredOptions.EnableTestFeature);
        _ = Assert.Single(configuredOptions.AuthenticationOptions);
        _ = Assert.IsType<ApiKeyAuthenticationOptions>(configuredOptions.AuthenticationOptions.First());
    }

    [Fact]
    public void AddTypedApiClient_WithEntraIdAuthentication_RegistersTokenProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        _ = loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        _ = services.AddSingleton(loggerFactoryMock.Object);
        _ = services.AddLogging();

        // Act
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithEntraIdAuthentication("api://resource"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify token providers are registered
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        _ = Assert.IsType<DefaultTokenCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        _ = Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());
    }

    [Fact]
    public void AddTypedApiClient_WithClientCredentialAuthentication_RegistersClientCredentialProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        _ = loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        _ = services.AddSingleton(loggerFactoryMock.Object);
        _ = services.AddLogging();

        var clientCredOptions = new ClientCredentialAuthenticationOptions
        {
            ApiAudience = "api://resource",
            TenantId = "tenant-id",
            ClientId = "client-id"
        };
        clientCredOptions.SetClientSecret("client-secret");

        // Act
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithAuthentication(clientCredOptions));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify token providers are registered
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        _ = Assert.IsType<ClientCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        _ = Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());
    }

    [Fact]
    public void AddTypedApiClient_WithMultipleClients_RegistersEachClientSeparately()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act - Register two different API clients
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api1.example.com")
            .WithTestFeature());

        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api2.example.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should be able to get instances of both clients
        var clients = serviceProvider.GetServices<ITestApiClient>().ToList();
        Assert.Equal(2, clients.Count);
    }

    [Fact]
    public void AddTypedApiClient_ConfiguresHttpClientWithRetryPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithMaxRetryCount(5));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.NotNull(httpClientFactory);

        // Verify we can create an HttpClient for our typed client
        var httpClient = httpClientFactory?.CreateClient(nameof(TestApiClient));
        Assert.NotNull(httpClient);
    }

    [Fact]
    public void AddTypedApiClient_WithStandardApiClientOptions_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act
        _ = services.AddTypedApiClient<IStandardTestApiClient, StandardTestApiClient, ApiClientOptions, ApiClientOptionsBuilder>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithApiKeyAuthentication("test-key"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<IStandardTestApiClient>();
        var options = serviceProvider.GetService<ApiClientOptions>();

        Assert.NotNull(client);
        Assert.NotNull(options);
        Assert.Equal("https://api.example.com", options.BaseUrl);
    }

    [Fact]
    public void AddTypedApiClient_ThrowsArgumentNullException_WhenBaseUrlIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(
                options => options.WithBaseUrl(null!)));

        Assert.Contains("baseUrl", ex.Message);
    }

    [Fact]
    public void AddTypedApiClient_ThrowsArgumentException_WhenBaseUrlIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(
                options => options.WithBaseUrl("")));

        Assert.Contains("baseUrl", ex.Message);
    }

    [Fact]
    public void AddTypedApiClient_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(null!));
    }

    [Fact]
    public void AddTypedApiClient_CanRegisterMultipleDifferentClients()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act
        _ = services.AddTypedApiClient<ITestApiClient, TestApiClient, TestApiOptions, TestApiOptionsBuilder>(options => options
            .WithBaseUrl("https://api1.example.com"));

        _ = services.AddTypedApiClient<IStandardTestApiClient, StandardTestApiClient, ApiClientOptions, ApiClientOptionsBuilder>(options => options
            .WithBaseUrl("https://api2.example.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var testClient = serviceProvider.GetService<ITestApiClient>();
        var standardClient = serviceProvider.GetService<IStandardTestApiClient>();

        Assert.NotNull(testClient);
        Assert.NotNull(standardClient);
        Assert.NotSame(testClient, standardClient);
    }

    #region AddApiClient (Simplified Registration) Tests

    [Fact]
    public void AddApiClient_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act
        var result = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl("https://api.example.com"));

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Verify IMemoryCache is registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());

        // Verify IRestClientService is registered
        Assert.NotNull(serviceProvider.GetService<IRestClientService>());

        // Verify the default options are registered
        Assert.NotNull(serviceProvider.GetService<ApiClientOptions>());

        // Verify the client is registered
        Assert.NotNull(serviceProvider.GetService<IStandardTestApiClient>());
        _ = Assert.IsType<StandardTestApiClient>(serviceProvider.GetService<IStandardTestApiClient>());
    }

    [Fact]
    public void AddApiClient_ConfiguresOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        var baseUrl = "https://api.example.com";
        var apiKey = "test-api-key";

        // Act
        _ = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiKey));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<ApiClientOptions>();

        Assert.Equal(baseUrl, configuredOptions.BaseUrl);
        _ = Assert.Single(configuredOptions.AuthenticationOptions);
        _ = Assert.IsType<ApiKeyAuthenticationOptions>(configuredOptions.AuthenticationOptions.First());
    }

    [Fact]
    public void AddApiClient_WithEntraIdAuthentication_RegistersTokenProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        _ = loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        _ = services.AddSingleton(loggerFactoryMock.Object);
        _ = services.AddLogging();

        // Act
        _ = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithEntraIdAuthentication("api://resource"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify token providers are registered
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        _ = Assert.IsType<DefaultTokenCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        _ = Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());
    }

    [Fact]
    public void AddApiClient_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
                .WithBaseUrl("https://api.example.com")));
    }

    [Fact]
    public void AddApiClient_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(null!));
    }

    [Fact]
    public void AddApiClient_CanRegisterMultipleClients()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act - Register multiple clients with simplified method
        _ = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl("https://api1.example.com"));

        _ = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl("https://api2.example.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should be able to get instances of both clients
        var clients = serviceProvider.GetServices<IStandardTestApiClient>().ToList();
        Assert.Equal(2, clients.Count);
    }

    [Fact]
    public void AddApiClient_ConfiguresHttpClientWithRetryPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddLogging(); // Add required logging services

        // Act
        _ = services.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl("https://api.example.com")
            .WithMaxRetryCount(5));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.NotNull(httpClientFactory);

        // Verify we can create an HttpClient for our typed client
        var httpClient = httpClientFactory?.CreateClient(nameof(StandardTestApiClient));
        Assert.NotNull(httpClient);
    }

    [Fact]
    public void AddApiClient_IsEquivalentToAddTypedApiClientWithDefaultOptions()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();
        _ = services1.AddLogging();
        _ = services2.AddLogging();

        var baseUrl = "https://api.example.com";
        var apiKey = "test-api-key";

        // Act - Register using simplified method
        _ = services1.AddApiClient<IStandardTestApiClient, StandardTestApiClient>(options => options
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiKey));

        // Act - Register using full method with default options
        _ = services2.AddTypedApiClient<IStandardTestApiClient, StandardTestApiClient, ApiClientOptions, ApiClientOptionsBuilder>(options => options
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiKey));

        // Assert - Both should produce equivalent configurations
        var serviceProvider1 = services1.BuildServiceProvider();
        var serviceProvider2 = services2.BuildServiceProvider();

        var options1 = serviceProvider1.GetRequiredService<ApiClientOptions>();
        var options2 = serviceProvider2.GetRequiredService<ApiClientOptions>();

        Assert.Equal(options1.BaseUrl, options2.BaseUrl);
        Assert.Equal(options1.AuthenticationOptions.Count, options2.AuthenticationOptions.Count);

        var client1 = serviceProvider1.GetService<IStandardTestApiClient>();
        var client2 = serviceProvider2.GetService<IStandardTestApiClient>();

        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.Equal(client1.GetType(), client2.GetType());
    }

    #endregion

}
