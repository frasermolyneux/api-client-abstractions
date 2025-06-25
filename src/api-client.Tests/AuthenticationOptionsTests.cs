using Xunit;

namespace MxIO.ApiClient.Tests;

public class AuthenticationOptionsTests
{
    [Fact]
    public void ApiKeyAuthenticationOptions_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new ApiKeyAuthenticationOptions();

        // Assert
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal("Ocp-Apim-Subscription-Key", options.HeaderName);
        Assert.Equal(AuthenticationType.ApiKey, options.AuthenticationType);
    }

    [Fact]
    public void ApiKeyAuthenticationOptions_PropertiesCanBeSet()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions();
        var apiKey = "test-api-key-123";
        var headerName = "X-API-Key";

        // Act
        options.ApiKey = apiKey;
        options.HeaderName = headerName;

        // Assert
        Assert.Equal(apiKey, options.ApiKey);
        Assert.Equal(headerName, options.HeaderName);
        Assert.Equal(AuthenticationType.ApiKey, options.AuthenticationType);
    }

    [Fact]
    public void AzureCredentialAuthenticationOptions_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new AzureCredentialAuthenticationOptions();

        // Assert
        Assert.Equal(string.Empty, options.ApiAudience);
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);
    }

    [Fact]
    public void AzureCredentialAuthenticationOptions_PropertiesCanBeSet()
    {
        // Arrange
        var options = new AzureCredentialAuthenticationOptions();
        var apiAudience = "https://api.example.com";

        // Act
        options.ApiAudience = apiAudience;

        // Assert
        Assert.Equal(apiAudience, options.ApiAudience);
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);
    }

    [Fact]
    public void ClientCredentialAuthenticationOptions_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new ClientCredentialAuthenticationOptions();

        // Assert
        Assert.Equal(string.Empty, options.ApiAudience);
        Assert.Equal(string.Empty, options.TenantId);
        Assert.Equal(string.Empty, options.ClientId);
        Assert.Equal(string.Empty, options.ClientSecret);
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);
    }

    [Fact]
    public void ClientCredentialAuthenticationOptions_PropertiesCanBeSet()
    {
        // Arrange
        var options = new ClientCredentialAuthenticationOptions();
        var apiAudience = "https://api.example.com";
        var tenantId = "tenant-id-123";
        var clientId = "client-id-123";
        var clientSecret = "client-secret-123";

        // Act
        options.ApiAudience = apiAudience;
        options.TenantId = tenantId;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;

        // Assert
        Assert.Equal(apiAudience, options.ApiAudience);
        Assert.Equal(tenantId, options.TenantId);
        Assert.Equal(clientId, options.ClientId);
        Assert.Equal(clientSecret, options.ClientSecret);
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);
    }
}
