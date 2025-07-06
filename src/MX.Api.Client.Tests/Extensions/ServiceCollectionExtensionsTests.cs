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

        // Verify IRestClientService is registered with correct lifetime
        var restClientDescriptor = Assert.Single(services, s => s.ServiceType == typeof(IRestClientService));
        Assert.Equal(ServiceLifetime.Singleton, restClientDescriptor.Lifetime);
        Assert.NotNull(restClientDescriptor.ImplementationFactory); // Should be using a factory for DI
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
            BaseUrl = "https://api.example.com"
        };
        options.AuthenticationOptions.Add(new AzureCredentialAuthenticationOptions
        {
            ApiAudience = "api://resource"
        });

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
        options.AuthenticationOptions.Add(clientCredOptions);

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

        Assert.Single(options.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions.First());
        var authOptions = (ApiKeyAuthenticationOptions)options.AuthenticationOptions.First();
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

        Assert.Single(options.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions.First());
        var authOptions = (ApiKeyAuthenticationOptions)options.AuthenticationOptions.First();
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
        Assert.Single(options.AuthenticationOptions);
        Assert.IsType<AzureCredentialAuthenticationOptions>(options.AuthenticationOptions.First());
        var authOptions = (AzureCredentialAuthenticationOptions)options.AuthenticationOptions.First();
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
        Assert.Single(options.AuthenticationOptions);
        Assert.IsType<AzureCredentialAuthenticationOptions>(options.AuthenticationOptions.First());
        var authOptions = (AzureCredentialAuthenticationOptions)options.AuthenticationOptions.First();
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
        Assert.Single(options.AuthenticationOptions);
        Assert.IsType<ClientCredentialAuthenticationOptions>(options.AuthenticationOptions.First());
        var authOptions = (ClientCredentialAuthenticationOptions)options.AuthenticationOptions.First();
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

    [Fact]
    public void MultipleAuthenticationMethods_CanBeConfiguredTogether()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var subscriptionKey = "subscription-key-123";
        var apiAudience = "api://resource";

        var options = new ApiClientOptions
        {
            BaseUrl = "https://api.example.com"
        };
        options.WithSubscriptionKey(subscriptionKey)
               .WithEntraIdAuthentication(apiAudience);

        // Act
        services.WithOptions(options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify authentication services are registered for Entra ID
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify multiple authentication options are configured
        var configuredOptions = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;
        Assert.Equal(2, configuredOptions.AuthenticationOptions.Count);

        var apiKeyAuth = configuredOptions.AuthenticationOptions.OfType<ApiKeyAuthenticationOptions>().First();
        Assert.Equal(subscriptionKey, apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", apiKeyAuth.HeaderName);

        var entraIdAuth = configuredOptions.AuthenticationOptions.OfType<AzureCredentialAuthenticationOptions>().First();
        Assert.Equal(apiAudience, entraIdAuth.ApiAudience);

        // Clean up
        apiKeyAuth.Dispose();
    }

    [Fact]
    public void WithBaseUrl_NamedOptions_ConfiguresNamedBaseUrl()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUrl = "https://api.example.com";
        var optionsName = "TestApi";

        // Act
        var result = services.WithBaseUrl(optionsName, baseUrl);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        Assert.Equal(baseUrl, namedOptions.BaseUrl);
    }

    [Theory]
    [InlineData(false, true)]  // optionsName is null
    [InlineData(false, false, null)]  // baseUrl is null
    [InlineData(false, false, "")]     // baseUrl is empty
    public void WithBaseUrl_NamedOptions_ThrowsArgumentException_WhenParametersAreInvalid(
        bool nullServices, bool nullOptionsName, string? baseUrl = "https://api.example.com")
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        string? optionsName = nullOptionsName ? null : "TestApi";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.WithBaseUrl(optionsName!, baseUrl!));
    }

    [Fact]
    public void WithBaseUrl_NamedOptions_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var optionsName = "TestApi";
        var baseUrl = "https://api.example.com";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithBaseUrl(optionsName, baseUrl));
    }

    [Fact]
    public void WithBaseUrl_NamedOptions_AppliesAdditionalConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUrl = "https://api.example.com";
        var optionsName = "TestApi";
        Action<ApiClientOptions> configureOptions = opt =>
        {
            opt.MaxRetryCount = 5;
        };

        // Act
        services.WithBaseUrl(optionsName, baseUrl, configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        Assert.Equal(baseUrl, namedOptions.BaseUrl);
        Assert.Equal(5, namedOptions.MaxRetryCount);
    }

    [Fact]
    public void WithApiKeyAuthentication_NamedOptions_ConfiguresNamedApiKeyAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var optionsName = "TestApi";

        // Act
        var result = services.WithApiKeyAuthentication(name: optionsName, apiKey: apiKey);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        Assert.Single(namedOptions.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(namedOptions.AuthenticationOptions.First());
        
        var authOptions = (ApiKeyAuthenticationOptions)namedOptions.AuthenticationOptions.First();
        Assert.Equal(apiKey, authOptions.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", authOptions.HeaderName); // Default header name
    }

    [Fact]
    public void WithApiKeyAuthentication_NamedOptions_ConfiguresCustomHeaderName()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var headerName = "X-API-Key";
        var optionsName = "TestApi";

        // Act
        services.WithApiKeyAuthentication(name: optionsName, apiKey: apiKey, headerName: headerName);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        var authOptions = (ApiKeyAuthenticationOptions)namedOptions.AuthenticationOptions.First();
        Assert.Equal(headerName, authOptions.HeaderName);
    }

    [Theory]
    [InlineData(false, true)]  // optionsName is null
    public void WithApiKeyAuthentication_NamedOptions_ThrowsArgumentException_WhenParametersAreInvalid(
        bool nullServices, bool nullOptionsName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        string? optionsName = nullOptionsName ? null : "TestApi";
        var apiKey = "test-api-key";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.WithApiKeyAuthentication(name: optionsName!, apiKey: apiKey));
    }

    [Fact]
    public void WithApiKeyAuthentication_NamedOptions_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var optionsName = "TestApi";
        var apiKey = "test-api-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithApiKeyAuthentication(name: optionsName, apiKey: apiKey));
    }

    [Fact]
    public void WithAzureCredentials_NamedOptions_ConfiguresNamedAzureCredentials()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var apiAudience = "api://resource";
        var optionsName = "TestApi";

        // Act
        var result = services.WithAzureCredentials(name: optionsName, apiAudience: apiAudience);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();
        
        // Verify services are registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify named options are configured
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        Assert.Single(namedOptions.AuthenticationOptions);
        Assert.IsType<AzureCredentialAuthenticationOptions>(namedOptions.AuthenticationOptions.First());
        
        var authOptions = (AzureCredentialAuthenticationOptions)namedOptions.AuthenticationOptions.First();
        Assert.Equal(apiAudience, authOptions.ApiAudience);
    }

    [Theory]
    [InlineData(false, true)]  // optionsName is null
    public void WithAzureCredentials_NamedOptions_ThrowsArgumentException_WhenParametersAreInvalid(
        bool nullServices, bool nullOptionsName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        string? optionsName = nullOptionsName ? null : "TestApi";
        var apiAudience = "api://resource";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.WithAzureCredentials(name: optionsName!, apiAudience: apiAudience));
    }

    [Fact]
    public void WithAzureCredentials_NamedOptions_ThrowsArgumentNullException_WhenServiceCollectionIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var optionsName = "TestApi";
        var apiAudience = "api://resource";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.WithAzureCredentials(name: optionsName, apiAudience: apiAudience));
    }

    [Fact]
    public void WithClientCredentials_NamedOptions_ConfiguresNamedClientCredentials()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        var apiAudience = "api://resource";
        var tenantId = "tenant-id";
        var clientId = "client-id";
        var clientSecret = "client-secret";
        var optionsName = "TestApi";

        // Act
        var result = services.WithClientCredentials(name: optionsName, apiAudience: apiAudience, tenantId: tenantId, clientId: clientId, clientSecret: clientSecret);

        // Assert
        Assert.Same(services, result); // Returns the same instance for method chaining

        var serviceProvider = services.BuildServiceProvider();

        // Verify services are registered
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ITokenCredentialProvider>());
        Assert.NotNull(serviceProvider.GetService<IApiTokenProvider>());

        // Verify named options are configured
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();
        var namedOptions = optionsSnapshot.Get(optionsName);

        Assert.Single(namedOptions.AuthenticationOptions);
        Assert.IsType<ClientCredentialAuthenticationOptions>(namedOptions.AuthenticationOptions.First());
        
        var authOptions = (ClientCredentialAuthenticationOptions)namedOptions.AuthenticationOptions.First();
        Assert.Equal(apiAudience, authOptions.ApiAudience);
        Assert.Equal(tenantId, authOptions.TenantId);
        Assert.Equal(clientId, authOptions.ClientId);
        Assert.Equal(clientSecret, authOptions.GetClientSecretAsString());
    }

    [Fact]
    public void MultipleNamedOptions_CanBeConfiguredIndependently()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required services for logging
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactoryMock.Object);
        services.AddLogging();

        // Act - Configure two different API clients
        services.WithBaseUrl(name: "GeoLocationApi", baseUrl: "https://geo.api.com")
               .WithApiKeyAuthentication(name: "GeoLocationApi", apiKey: "geo-api-key");

        services.WithBaseUrl(name: "RepositoryApi", baseUrl: "https://repo.api.com")
               .WithAzureCredentials(name: "RepositoryApi", apiAudience: "api://repo-resource");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();

        // Verify first API client configuration
        var geoOptions = optionsSnapshot.Get("GeoLocationApi");
        Assert.Equal("https://geo.api.com", geoOptions.BaseUrl);
        Assert.Single(geoOptions.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(geoOptions.AuthenticationOptions.First());

        // Verify second API client configuration
        var repoOptions = optionsSnapshot.Get("RepositoryApi");
        Assert.Equal("https://repo.api.com", repoOptions.BaseUrl);
        Assert.Single(repoOptions.AuthenticationOptions);
        Assert.IsType<AzureCredentialAuthenticationOptions>(repoOptions.AuthenticationOptions.First());

        // Verify they are completely isolated
        Assert.NotEqual(geoOptions.BaseUrl, repoOptions.BaseUrl);
        Assert.NotEqual(geoOptions.AuthenticationOptions.First().GetType(), 
                       repoOptions.AuthenticationOptions.First().GetType());
    }

    [Fact]
    public void NamedOptions_DoNotAffectDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure both default and named options
        services.WithBaseUrl("https://default.api.com")
               .WithApiKeyAuthentication("default-api-key");

        services.WithBaseUrl(name: "NamedApi", baseUrl: "https://named.api.com")
               .WithAzureCredentials(name: "NamedApi", apiAudience: "api://named-resource");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>();
        var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ApiClientOptions>>();

        // Verify default options
        var defaultOptions = options.Value;
        Assert.Equal("https://default.api.com", defaultOptions.BaseUrl);
        Assert.Single(defaultOptions.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(defaultOptions.AuthenticationOptions.First());

        // Verify named options
        var namedOptions = optionsSnapshot.Get("NamedApi");
        Assert.Equal("https://named.api.com", namedOptions.BaseUrl);
        Assert.Single(namedOptions.AuthenticationOptions);
        Assert.IsType<AzureCredentialAuthenticationOptions>(namedOptions.AuthenticationOptions.First());

        // Verify they are completely separate
        Assert.NotEqual(defaultOptions.BaseUrl, namedOptions.BaseUrl);
        Assert.NotEqual(defaultOptions.AuthenticationOptions.First().GetType(), 
                       namedOptions.AuthenticationOptions.First().GetType());
    }
}
