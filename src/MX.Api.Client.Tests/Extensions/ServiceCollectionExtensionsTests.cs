using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using Xunit;

namespace MX.Api.Client.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApiClient_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApiClient();

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Verify IMemoryCache is registered
        var memoryCacheDescriptor = Assert.Single(services, s => s.ServiceType == typeof(IMemoryCache));
        Assert.Equal(ServiceLifetime.Singleton, memoryCacheDescriptor.Lifetime);

        // Verify IRestClientService is registered with correct implementation and lifetime
        var restClientDescriptor = Assert.Single(services, s => s.ServiceType == typeof(IRestClientService));
        Assert.Equal(typeof(RestClientService), restClientDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, restClientDescriptor.Lifetime);
    }

    [Fact]
    public void AddApiClient_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddApiClient());
    }

    [Fact]
    public void WithOptions_ConfiguresOptions_WhenOptionsInstanceProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ApiClientOptions
        {
            BaseUrl = "https://api.example.com",
            MaxRetryCount = 5
        };

        // Act
        var result = services.WithOptions(options);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.Equal(options.BaseUrl, configuredOptions.BaseUrl);
        Assert.Equal(options.MaxRetryCount, configuredOptions.MaxRetryCount);
    }

    [Fact]
    public void WithOptions_RegistersTokenProviders_WhenUsingEntraIdAuth()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var options = new ApiClientOptions
        {
            BaseUrl = "https://api.example.com",
            AuthenticationOptions = new AzureCredentialAuthenticationOptions
            {
                ApiAudience = "api://resource"
            }
        };

        // Act
        services.WithOptions(options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify IMemoryCache is registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());

        // Verify token providers are registered
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        Assert.IsType<DefaultTokenCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());
    }

    [Fact]
    public void WithOptions_RegistersClientCredentialProvider_WhenUsingClientCredentialAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var options = new ApiClientOptions
        {
            BaseUrl = "https://api.example.com"
        };

        var clientCredOptions = new ClientCredentialAuthenticationOptions
        {
            ApiAudience = "api://resource",
            TenantId = "tenant-id",
            ClientId = "client-id"
        };
        clientCredOptions.SetClientSecret("client-secret");
        options.AuthenticationOptions = clientCredOptions;

        // Act
        services.WithOptions(options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify IMemoryCache is registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());

        // Verify token providers are registered
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        Assert.IsType<ClientCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());
    }

    [Fact]
    public void WithOptions_ConfiguresOptions_WhenActionProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<ApiClientOptions> configureAction = opt =>
        {
            opt.BaseUrl = "https://api.example.com";
            opt.MaxRetryCount = 5;
        };

        // Act
        var result = services.WithOptions(configureAction);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.Equal("https://api.example.com", configuredOptions.BaseUrl);
        Assert.Equal(5, configuredOptions.MaxRetryCount);
    }

    [Fact]
    public void WithOptions_ThrowsArgumentException_WhenInvalidParameterType()
    {
        // Arrange
        var services = new ServiceCollection();
        var invalidParameter = "invalid parameter type";        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => services.WithOptions(invalidParameter));
        Assert.Contains("must be either an ApiClientOptions instance or an Action<ApiClientOptions>", ex.Message);
    }

    [Theory]
    [InlineData(true, false)]  // services is null
    [InlineData(false, true)]  // optionsOrConfigureAction is null
    public void WithOptions_ThrowsArgumentNullException_WhenParametersAreNull(
        bool nullServices, bool nullOptions)
    {
        // Arrange
        IServiceCollection? services = nullServices ? null : new ServiceCollection();
        object? options = nullOptions ? null : new ApiClientOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithOptions(options!));
    }

    [Fact]
    public void WithBaseUrl_ConfiguresBaseUrl()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUrl = "https://api.example.com";

        // Act
        var result = services.WithBaseUrl(baseUrl);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.Equal(baseUrl, options.BaseUrl);
    }

    [Fact]
    public void WithBaseUrl_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var baseUrl = "https://api.example.com";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithBaseUrl(baseUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WithBaseUrl_ThrowsArgumentException_WhenBaseUrlIsNullOrEmpty(string? baseUrl)
    {
        // Arrange
        var services = new ServiceCollection();        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => services.WithBaseUrl(baseUrl!));
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Fact]
    public void WithBaseUrl_AppliesAdditionalConfiguration_WhenConfigureOptionsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUrl = "https://api.example.com";
        Action<ApiClientOptions> configureOptions = opt =>
        {
            opt.MaxRetryCount = 5;
        };

        // Act
        services.WithBaseUrl(baseUrl, configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.Equal(baseUrl, options.BaseUrl);
        Assert.Equal(5, options.MaxRetryCount);
    }

    [Fact]
    public void WithApiKeyAuthentication_ConfiguresApiKeyAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        // Act
        var result = services.WithApiKeyAuthentication(apiKey);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions);
        var authOptions = (ApiKeyAuthenticationOptions)options.AuthenticationOptions;
        Assert.Equal(apiKey, authOptions.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", authOptions.HeaderName); // Default header name
    }

    [Fact]
    public void WithApiKeyAuthentication_ConfiguresCustomHeaderName()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var headerName = "X-API-Key";

        // Act
        services.WithApiKeyAuthentication(apiKey, headerName);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;

        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions);
        var authOptions = (ApiKeyAuthenticationOptions)options.AuthenticationOptions;
        Assert.Equal(apiKey, authOptions.GetApiKeyAsString());
        Assert.Equal(headerName, authOptions.HeaderName);
    }

    [Fact]
    public void WithApiKeyAuthentication_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var apiKey = "test-api-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithApiKeyAuthentication(apiKey));
    }

    [Fact]
    public void WithAzureCredentials_ConfiguresAzureCredentialAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var apiAudience = "api://resource";

        // Act
        var result = services.WithAzureCredentials(apiAudience);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Verify services are registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct authentication options
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;
        Assert.IsType<AzureCredentialAuthenticationOptions>(options.AuthenticationOptions);
        var authOptions = (AzureCredentialAuthenticationOptions)options.AuthenticationOptions;
        Assert.Equal(apiAudience, authOptions.ApiAudience);
    }

    [Fact]
    public void WithAzureCredentials_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var apiAudience = "api://resource";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithAzureCredentials(apiAudience));
    }

    [Fact]
    public void WithAzureCredentials_WithOptions_ConfiguresAzureCredentialAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var apiAudience = "api://resource";
        Action<DefaultAzureCredentialOptions> configureOptions = opt =>
        {
            opt.ExcludeEnvironmentCredential = true;
            opt.ExcludeSharedTokenCacheCredential = true;
        };

        // Act
        var result = services.WithAzureCredentials(apiAudience, configureOptions);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Verify services are registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct authentication options
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;
        Assert.IsType<AzureCredentialAuthenticationOptions>(options.AuthenticationOptions);
        var authOptions = (AzureCredentialAuthenticationOptions)options.AuthenticationOptions;
        Assert.Equal(apiAudience, authOptions.ApiAudience);

        // Verify DefaultAzureCredentialOptions are configured
        var credentialOptions = serviceProvider.GetRequiredService<IOptions<DefaultAzureCredentialOptions>>().Value;
        Assert.True(credentialOptions.ExcludeEnvironmentCredential);
        Assert.True(credentialOptions.ExcludeSharedTokenCacheCredential);
    }

    [Theory]
    [InlineData(true, false)]  // services is null
    [InlineData(false, true)]  // configureCredentialOptions is null
    public void WithAzureCredentials_WithOptions_ThrowsArgumentNullException_WhenParametersAreNull(
        bool nullServices, bool nullConfigureOptions)
    {
        // Arrange
        IServiceCollection? services = nullServices ? null : new ServiceCollection();
        var apiAudience = "api://resource";
        Action<DefaultAzureCredentialOptions>? configureOptions = nullConfigureOptions ? null : opt => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithAzureCredentials(apiAudience, configureOptions!));
    }

    [Fact]
    public void WithClientCredentials_ConfiguresClientCredentialAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var apiAudience = "api://resource";
        var tenantId = "tenant-id";
        var clientId = "client-id";
        var clientSecret = "client-secret";

        // Act
        var result = services.WithClientCredentials(apiAudience, tenantId, clientId, clientSecret);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        // Verify services are registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct implementation types
        Assert.IsType<ClientCredentialProvider>(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.IsType<ApiTokenProvider>(serviceProvider.GetService<IApiTokenProvider>());

        // Verify correct authentication options
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;
        Assert.IsType<ClientCredentialAuthenticationOptions>(options.AuthenticationOptions);
        var authOptions = (ClientCredentialAuthenticationOptions)options.AuthenticationOptions;
        Assert.Equal(apiAudience, authOptions.ApiAudience);
        Assert.Equal(tenantId, authOptions.TenantId);
        Assert.Equal(clientId, authOptions.ClientId);
        Assert.Equal(clientSecret, authOptions.GetClientSecretAsString());
    }

    [Fact]
    public void WithClientCredentials_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var apiAudience = "api://resource";
        var tenantId = "tenant-id";
        var clientId = "client-id";
        var clientSecret = "client-secret";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithClientCredentials(apiAudience, tenantId, clientId, clientSecret));
    }

    [Theory]
    [InlineData(null, "tenant-id", "client-id", "client-secret")]  // apiAudience is null
    [InlineData("", "tenant-id", "client-id", "client-secret")]    // apiAudience is empty
    [InlineData("api://resource", null, "client-id", "client-secret")]  // tenantId is null
    [InlineData("api://resource", "", "client-id", "client-secret")]    // tenantId is empty
    [InlineData("api://resource", "tenant-id", null, "client-secret")]  // clientId is null
    [InlineData("api://resource", "tenant-id", "", "client-secret")]    // clientId is empty
    [InlineData("api://resource", "tenant-id", "client-id", null)]  // clientSecret is null
    [InlineData("api://resource", "tenant-id", "client-id", "")]    // clientSecret is empty
    public void WithClientCredentials_ThrowsArgumentException_WhenRequiredParametersAreNullOrEmpty(
        string? apiAudience, string? tenantId, string? clientId, string? clientSecret)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.WithClientCredentials(apiAudience!, tenantId!, clientId!, clientSecret!));
    }
}
