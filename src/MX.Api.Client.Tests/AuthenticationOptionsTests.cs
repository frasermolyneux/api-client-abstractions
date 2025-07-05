using MX.Api.Client.Configuration;
using Xunit;

namespace MX.Api.Client.Tests;

public class AuthenticationOptionsTests
{
    [Fact]
    public void ApiKeyAuthenticationOptions_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new ApiKeyAuthenticationOptions();

        // Assert
        Assert.Null(options.ApiKey);
        Assert.Equal("Ocp-Apim-Subscription-Key", options.HeaderName);
        Assert.Equal(AuthenticationType.ApiKey, options.AuthenticationType);
        Assert.Equal(string.Empty, options.GetApiKeyAsString());
    }

    [Fact]
    public void ApiKeyAuthenticationOptions_SetApiKey_StoresSecurely()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions();
        var apiKey = "test-api-key-123";
        var headerName = "X-API-Key";

        // Act
        options.SetApiKey(apiKey);
        options.HeaderName = headerName;

        // Assert
        Assert.Equal(apiKey, options.GetApiKeyAsString());
        Assert.Equal(headerName, options.HeaderName);
        Assert.Equal(AuthenticationType.ApiKey, options.AuthenticationType);

        // Clean up
        options.Dispose();
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
        Assert.Null(options.ClientSecret);
        Assert.Equal(string.Empty, options.GetClientSecretAsString());
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);
    }

    [Fact]
    public void ClientCredentialAuthenticationOptions_SetClientSecret_StoresSecurely()
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
        options.SetClientSecret(clientSecret);

        // Assert
        Assert.Equal(apiAudience, options.ApiAudience);
        Assert.Equal(tenantId, options.TenantId);
        Assert.Equal(clientId, options.ClientId);
        Assert.Equal(clientSecret, options.GetClientSecretAsString());
        Assert.Equal(AuthenticationType.EntraId, options.AuthenticationType);

        // Clean up
        options.Dispose();
    }
}
