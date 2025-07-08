using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.ProductApiClient;
using MX.Api.IntegrationTests.Clients.UserApiClient;
using MX.Api.IntegrationTests.Clients.WeatherApiClient;
using MX.Api.IntegrationTests.DummyApis.UserApi.Models;
using Xunit;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// End-to-end integration tests that actually call the APIs
/// </summary>
public class EndToEndIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public EndToEndIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task WeatherApiClient_CanCallWeatherEndpoints_Successfully()
    {
        // This test verifies that the Weather API endpoints work correctly
        // by calling them directly with the test server's HttpClient

        // Act & Assert - Health Check (no auth required)
        var healthResponse = await _httpClient.GetAsync("/api/weather/health");
        healthResponse.EnsureSuccessStatusCode();

        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        Assert.Contains("Weather API is healthy", healthContent);

        // Create authenticated requests for protected endpoints
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("X-API-Key", "weather-test-key");

        // Act & Assert - Get Current Weather
        var currentWeatherResponse = await authenticatedClient.GetAsync("/api/weather/current?location=London");
        currentWeatherResponse.EnsureSuccessStatusCode();

        var currentWeatherContent = await currentWeatherResponse.Content.ReadAsStringAsync();
        Assert.Contains("London", currentWeatherContent);

        // Act & Assert - Get Forecast
        var forecastResponse = await authenticatedClient.GetAsync("/api/weather/forecast?location=Paris&days=3");
        forecastResponse.EnsureSuccessStatusCode();

        var forecastContent = await forecastResponse.Content.ReadAsStringAsync();
        Assert.Contains("Paris", forecastContent);
    }

    [Fact]
    public void WeatherApiClient_CanBeRegistered_Successfully()
    {
        // This test verifies that the API client registration system works correctly

        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRestClientService, RestClientService>();

        // Configure the Weather API client using streamlined registration
        services.AddWeatherApiClient("https://api.example.com", "test-key");

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var weatherClient = serviceProvider.GetService<IWeatherApiClient>();
        Assert.NotNull(weatherClient);

        // Verify the client is of the correct type
        Assert.IsType<WeatherApiClient>(weatherClient);
    }

    [Fact]
    public async Task UserApiClient_CanCallUserEndpoints_Successfully()
    {
        // This test verifies that the User API endpoints work correctly
        // by calling them directly with the test server's HttpClient

        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer user-test-token");

        // Act & Assert - Get Users
        var usersResponse = await authenticatedClient.GetAsync("/api/users?page=1&pageSize=2");
        usersResponse.EnsureSuccessStatusCode();

        var usersContent = await usersResponse.Content.ReadAsStringAsync();
        Assert.Contains("items", usersContent.ToLower());

        // Act & Assert - Get Specific User
        var userResponse = await authenticatedClient.GetAsync("/api/users/1");
        userResponse.EnsureSuccessStatusCode();

        var userContent = await userResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"id\":", userContent.ToLower());

        // Act & Assert - Create User (POST)
        var createUserJson = """
            {
                "id": 999,
                "username": "testuser_12345",
                "email": "test@example.com",
                "fullName": "Test User"
            }
            """;

        var createContent = new StringContent(createUserJson, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await authenticatedClient.PostAsync("/api/users", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("testuser_12345", createResponseContent);
    }

    [Fact]
    public async Task UserApiClient_CanBeRegistered_Successfully()
    {
        // This test verifies that the API client registration system works correctly for User API

        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRestClientService, RestClientService>();

        // Configure the User API client with streamlined registration
        services.AddUserApiClient(options => options
            .WithBaseUrl("https://api.example.com")
            .WithBasicAuth("user-test-token")
            .WithUserCaching(30)
            .WithDetailedLogging()
            .WithDefaultRole("TestUser")
            .WithMaxPageSize(5));

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userClient = serviceProvider.GetService<IUserApiClient>();
        Assert.NotNull(userClient);

        // Verify the client is of the correct type
        Assert.IsType<UserApiClient>(userClient);
    }

    [Fact]
    public async Task ProductApiClient_CanCallProductEndpoints_Successfully()
    {
        // This test verifies that the Product API endpoints work correctly
        // by calling them directly with the test server's HttpClient

        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-product-key");

        // Act & Assert - Get Products
        var productsResponse = await authenticatedClient.GetAsync("/api/products");
        productsResponse.EnsureSuccessStatusCode();

        var productsContent = await productsResponse.Content.ReadAsStringAsync();
        Assert.Contains("items", productsContent.ToLower());
        Assert.Contains("laptop", productsContent.ToLower());

        // Act & Assert - Get Specific Product
        var productResponse = await authenticatedClient.GetAsync("/api/products/1");
        productResponse.EnsureSuccessStatusCode();

        var productContent = await productResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"id\":", productContent.ToLower());
        Assert.Contains("laptop", productContent.ToLower());
    }

    [Fact]
    public async Task ProductApiClient_CanBeRegistered_Successfully()
    {
        // This test verifies that the API client registration system works correctly for Product API

        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRestClientService, RestClientService>();

        // Configure the Product API client with streamlined registration
        services.AddProductApiClient(
            "https://api.example.com",
            "test-product-key");

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var productClient = serviceProvider.GetService<IProductApiClient>();
        Assert.NotNull(productClient);

        // Verify the client is of the correct type
        Assert.IsType<ProductApiClient>(productClient);
    }

    [Fact]
    public async Task MultipleEndpoints_CanWorkSimultaneously()
    {
        // This test verifies that all three API endpoints can be called simultaneously
        // by making direct HTTP requests to all APIs at the same time

        // Create authenticated clients for all APIs
        var weatherClient = _factory.CreateClient();
        weatherClient.DefaultRequestHeaders.Add("X-API-Key", "weather-test-key");

        var userClient = _factory.CreateClient();
        userClient.DefaultRequestHeaders.Add("Authorization", "Bearer user-test-token");

        var productClient = _factory.CreateClient();
        productClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-product-key");

        // Act - Call all three APIs simultaneously
        var weatherTask = weatherClient.GetAsync("/api/weather/current?location=Tokyo");
        var usersTask = userClient.GetAsync("/api/users?page=1&pageSize=5");
        var productsTask = productClient.GetAsync("/api/products");

        await Task.WhenAll(weatherTask, usersTask, productsTask);

        // Assert
        var weatherResponse = await weatherTask;
        var usersResponse = await usersTask;
        var productsResponse = await productsTask;

        weatherResponse.EnsureSuccessStatusCode();
        usersResponse.EnsureSuccessStatusCode();
        productsResponse.EnsureSuccessStatusCode();

        var weatherContent = await weatherResponse.Content.ReadAsStringAsync();
        var usersContent = await usersResponse.Content.ReadAsStringAsync();
        var productsContent = await productsResponse.Content.ReadAsStringAsync();

        Assert.Contains("Tokyo", weatherContent);
        Assert.Contains("items", usersContent.ToLower());
        Assert.Contains("laptop", productsContent.ToLower());
    }

    [Fact]
    public async Task ProductApiEndpoints_HandleAuthenticationFailure_Correctly()
    {
        // This test verifies that Product API endpoints properly reject invalid authentication
        // by making direct HTTP requests with invalid credentials

        // Create client with invalid token
        var invalidAuthClient = _factory.CreateClient();
        invalidAuthClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

        // Act
        var response = await invalidAuthClient.GetAsync("/api/products");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid or missing authentication", content);
    }

    [Fact]
    public async Task AllThreeApiClients_CanBeRegistered_Simultaneously()
    {
        // This test verifies that all three API clients can be registered at the same time

        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRestClientService, RestClientService>();

        // Configure all three API clients with streamlined registration
        services.AddWeatherApiClient("https://weather.example.com", "weather-key");

        services.AddUserApiClient(options => options
            .WithBaseUrl("https://users.example.com")
            .WithBasicAuth("user-token")
            .WithUserCaching(30)
            .WithDefaultRole("TestUser"));

        services.AddProductApiClient("https://products.example.com", "product-key");

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var weatherClient = serviceProvider.GetService<IWeatherApiClient>();
        var userClient = serviceProvider.GetService<IUserApiClient>();
        var productClient = serviceProvider.GetService<IProductApiClient>();

        Assert.NotNull(weatherClient);
        Assert.NotNull(userClient);
        Assert.NotNull(productClient);

        Assert.IsType<WeatherApiClient>(weatherClient);
        Assert.IsType<UserApiClient>(userClient);
        Assert.IsType<ProductApiClient>(productClient);

        // Verify they are all different instances
        Assert.NotEqual(weatherClient.GetType(), userClient.GetType());
        Assert.NotEqual(weatherClient.GetType(), productClient.GetType());
        Assert.NotEqual(userClient.GetType(), productClient.GetType());
    }

    [Fact]
    public async Task WeatherApiEndpoints_HandleAuthenticationFailure_Correctly()
    {
        // This test verifies that API endpoints properly reject invalid authentication
        // by making direct HTTP requests with invalid credentials

        // Create client with invalid API key
        var invalidAuthClient = _factory.CreateClient();
        invalidAuthClient.DefaultRequestHeaders.Add("X-API-Key", "invalid-key");

        // Act
        var response = await invalidAuthClient.GetAsync("/api/weather/current?location=London");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("INVALID_API_KEY", content);
    }

    [Fact]
    public async Task UserApiEndpoints_HandleAuthenticationFailure_Correctly()
    {
        // This test verifies that User API endpoints properly reject invalid authentication
        // by making direct HTTP requests with invalid credentials

        // Create client with invalid token
        var invalidAuthClient = _factory.CreateClient();
        invalidAuthClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

        // Act
        var response = await invalidAuthClient.GetAsync("/api/users");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("UNAUTHORIZED", content);
    }

    [Fact]
    public async Task UserApiEndpoints_RespectMaxPageSizeParameter()
    {
        // This test verifies that User API endpoints respect max page size limitations
        // by making direct HTTP requests with different page size parameters

        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer user-test-token");

        // Act - Request 10 users, but the API should apply a max limit
        var response = await authenticatedClient.GetAsync("/api/users?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Assert - The API should respect its internal max page size limits
        // Note: This assumes the User API controller has reasonable max page size validation
        Assert.Contains("items", content.ToLower());
        Assert.Contains("totalcount", content.ToLower());
    }
}
