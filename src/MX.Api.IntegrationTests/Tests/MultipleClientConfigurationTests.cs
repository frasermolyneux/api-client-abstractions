using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.ProductApiClient;
using MX.Api.IntegrationTests.Clients.UserApiClient;
using MX.Api.IntegrationTests.Clients.WeatherApiClient;
using Xunit;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// Tests for multiple API client configuration and registration
/// </summary>
public class MultipleClientConfigurationTests
{
    [Fact]
    public void CanRegisterMultipleClients_WithDifferentRegistrationMethods()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Register Weather API client using the new simplified method
        services.AddWeatherApiClient(
            "https://weather.example.com",
            "weather-test-key");

        // Act - Register User API client using the streamlined extension method
        services.AddUserApiClient(
            "https://users.example.com",
            "user-test-token");

        // Act - Register Product API client using the new simplified method
        services.AddProductApiClient(
            "https://products.example.com",
            "product-test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all three clients are registered
        var weatherClient = serviceProvider.GetService<IWeatherApiClient>();
        var userClient = serviceProvider.GetService<IUserApiClient>();
        var productClient = serviceProvider.GetService<IProductApiClient>();

        Assert.NotNull(weatherClient);
        Assert.NotNull(userClient);
        Assert.NotNull(productClient);
        Assert.IsType<WeatherApiClient>(weatherClient);
        Assert.IsType<UserApiClient>(userClient);
        Assert.IsType<ProductApiClient>(productClient);

        // Verify they are different instances
        Assert.NotEqual(weatherClient.GetType(), userClient.GetType());
        Assert.NotEqual(weatherClient.GetType(), productClient.GetType());
        Assert.NotEqual(userClient.GetType(), productClient.GetType());
    }

    [Fact]
    public void SimplifiedRegistration_ConfiguresServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddWeatherApiClient(
            "https://weather.example.com",
            "test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all required services are registered
        Assert.NotNull(serviceProvider.GetService<IWeatherApiClient>());
        Assert.NotNull(serviceProvider.GetService<IRestClientService>());
        Assert.NotNull(serviceProvider.GetService<IHttpClientFactory>());

        // Verify the client can be instantiated
        var client = serviceProvider.GetRequiredService<IWeatherApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void CustomOptionsRegistration_ConfiguresOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Using the streamlined User API registration
        services.AddUserApiClient(options => options
            .WithBaseUrl("https://users.example.com")
            .WithBasicAuth("test-token")
            .WithUserCaching(120)
            .WithDetailedLogging()
            .WithDefaultRole("Admin")
            .WithMaxPageSize(25));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userOptions = serviceProvider.GetRequiredService<UserApiOptions>();

        Assert.Equal("https://users.example.com", userOptions.BaseUrl);
        Assert.True(userOptions.EnableUserCaching);
        Assert.Equal(120, userOptions.CacheExpirationMinutes);
        Assert.True(userOptions.EnableDetailedLogging);
        Assert.Equal("Admin", userOptions.DefaultUserRole);
        Assert.Equal(25, userOptions.MaxPageSize);
    }

    [Fact]
    public void MultipleClients_CanCoexistWithDifferentConfigurations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Register the same client type multiple times with different configurations
        services.AddApiClient<IWeatherApiClient, WeatherApiClient>(options =>
        {
            options.WithBaseUrl("https://weather1.example.com")
                   .WithApiKeyAuthentication("key1", "X-API-Key");
        });

        services.AddApiClient<IWeatherApiClient, WeatherApiClient>(options =>
        {
            options.WithBaseUrl("https://weather2.example.com")
                   .WithApiKeyAuthentication("key2", "X-API-Key");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var clients = serviceProvider.GetServices<IWeatherApiClient>().ToList();

        Assert.Equal(2, clients.Count);
        Assert.All(clients, client => Assert.IsType<WeatherApiClient>(client));
    }

    [Fact]
    public void ClientRegistration_HandlesNullConfiguration_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddApiClient<IWeatherApiClient, WeatherApiClient>(null!));
    }

    [Fact]
    public void ClientRegistration_HandlesNullServices_ThrowsException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddApiClient<IWeatherApiClient, WeatherApiClient>(options =>
                options.WithBaseUrl("https://test.com")));
    }

    [Fact]
    public void BothRegistrationMethods_ProduceEquivalentResults_ForStandardOptions()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();
        services1.AddLogging();
        services2.AddLogging();

        var baseUrl = "https://test.example.com";
        var apiKey = "test-key";

        // Act - Use simplified method
        services1.AddApiClient<IWeatherApiClient, WeatherApiClient>(options =>
        {
            options.WithBaseUrl(baseUrl)
                   .WithApiKeyAuthentication(apiKey, "X-API-Key");
        });

        // Act - Use full method with standard options
        services2.AddTypedApiClient<IWeatherApiClient, WeatherApiClient, ApiClientOptions, ApiClientOptionsBuilder>(options =>
        {
            options.WithBaseUrl(baseUrl)
                   .WithApiKeyAuthentication(apiKey, "X-API-Key");
        });

        // Assert - Both should work the same way
        var serviceProvider1 = services1.BuildServiceProvider();
        var serviceProvider2 = services2.BuildServiceProvider();

        var client1 = serviceProvider1.GetService<IWeatherApiClient>();
        var client2 = serviceProvider2.GetService<IWeatherApiClient>();

        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.Equal(client1.GetType(), client2.GetType());

        var options1 = serviceProvider1.GetService<ApiClientOptions>();
        var options2 = serviceProvider2.GetService<ApiClientOptions>();

        Assert.NotNull(options1);
        Assert.NotNull(options2);
        Assert.Equal(options1.BaseUrl, options2.BaseUrl);
    }

    [Fact]
    public void ApiClients_CanUseStreamlinedRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Use the new streamlined registration methods
        services.AddWeatherApiClient("https://weather.example.com", "weather-key");
        services.AddUserApiClient("https://users.example.com", "user-token");
        services.AddProductApiClient("https://products.example.com", "product-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var weatherClient = serviceProvider.GetService<IWeatherApiClient>();
        var userClient = serviceProvider.GetService<IUserApiClient>();
        var productClient = serviceProvider.GetService<IProductApiClient>();

        Assert.NotNull(weatherClient);
        Assert.NotNull(userClient);
        Assert.NotNull(productClient);

        // Verify User API options were set with test defaults
        var userOptions = serviceProvider.GetService<UserApiOptions>();
        Assert.NotNull(userOptions);
        Assert.True(userOptions.EnableUserCaching);
        Assert.Equal(30, userOptions.CacheExpirationMinutes);
        Assert.True(userOptions.EnableDetailedLogging);
        Assert.Equal("Member", userOptions.DefaultUserRole);
        Assert.Equal(50, userOptions.MaxPageSize);
    }
}
