using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// Tests to validate the test infrastructure is working correctly
/// </summary>
public class InfrastructureTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public InfrastructureTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestServer_ShouldStart_Successfully()
    {
        // Arrange & Act
        using var client = _factory.CreateClient();

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.BaseAddress);

        // Let's also test a simple request to check if the server is responsive
        var response = await client.GetAsync("/api/weather/health");

        // We expect this to work or at least not fail with "status code 0"
        Assert.NotEqual(0, (int)response.StatusCode);
    }

    [Fact]
    public Task TestServer_BaseAddress_ShouldBeValid()
    {
        // Arrange & Act
        using var client = _factory.CreateClient();

        // Assert
        Assert.NotNull(client.BaseAddress);
        Assert.StartsWith("http", client.BaseAddress.ToString());

        return Task.CompletedTask;
    }
}
