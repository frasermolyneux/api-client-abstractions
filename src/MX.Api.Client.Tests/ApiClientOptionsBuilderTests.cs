using MX.Api.Client.Configuration;
using MX.Api.Client.Tests.TestClients;
using MX.Caching.Abstractions;
using Xunit;

namespace MX.Api.Client.Tests;

public class ApiClientOptionsBuilderTests
{
    private interface IOverloadedApi
    {
        Task<string> Get(int id);

        Task<string> Get(string id);
    }

    private interface ISynchronousApi
    {
        string Get();
    }

    private interface INonGenericTaskApi
    {
        Task ExecuteAsync();
    }

    [Fact]
    public void WithApiKeyAuthentication_DefaultLocation_SetsHeaderLocation()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key");
        var options = builder.Build();

        // Assert
        var apiKeyOptions = Assert.Single(options.AuthenticationOptions);
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(apiKeyOptions);
        Assert.Equal("test-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_QueryParameterLocation_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("my-api-key", "key", ApiKeyLocation.QueryParameter);
        var options = builder.Build();

        // Assert
        var apiKeyOptions = Assert.Single(options.AuthenticationOptions);
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(apiKeyOptions);
        Assert.Equal("my-api-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.QueryParameter, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_CustomHeaderName_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key", "X-Custom-Key");
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal("test-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("X-Custom-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_ExplicitHeaderLocation_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key", "X-Api-Key", ApiKeyLocation.Header);
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
        Assert.Equal("X-Api-Key", apiKeyAuth.HeaderName);
    }

    [Fact]
    public void WithSubscriptionKey_DefaultLocation_SetsHeaderLocation()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://api.example.com")
               .WithSubscriptionKey("sub-key");
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal("sub-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_NullApiKey_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        _ = Assert.Throws<ArgumentNullException>(() =>
            builder.WithApiKeyAuthentication(null!));
    }

    [Fact]
    public void WithApiKeyAuthentication_EmptyApiKey_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        _ = Assert.Throws<ArgumentException>(() =>
            builder.WithApiKeyAuthentication(""));
    }

    [Fact]
    public void WithApiKeyAuthentication_EmptyHeaderName_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        _ = Assert.Throws<ArgumentException>(() =>
            builder.WithApiKeyAuthentication("test-key", ""));
    }

    [Fact]
    public void Build_WithoutCaching_HasNoCachePolicies()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .Build();

        Assert.Empty(options.CachePolicies);
    }

    [Fact]
    public void WithCachePartition_PersistsConfiguredPartition()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .Build();

        Assert.Equal("tenant:123", options.CachePartition);
    }

    [Fact]
    public void Build_WithCachingAndNoCachePartition_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCaching(cache => cache.InMemory<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                TimeSpan.FromMinutes(10)));

        var exception = Assert.Throws<ArgumentException>(builder.Build);

        Assert.Equal("CachePartition", exception.ParamName);
    }

    [Fact]
    public void WithCaching_InMemory_PersistsExactMethodPolicy()
    {
        var ttl = TimeSpan.FromMinutes(10);
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.InMemory<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                ttl))
            .Build();

        var policy = Assert.Single(options.CachePolicies).Value;
        Assert.True(policy.Enabled);
        Assert.Equal(CacheTier.InProcess, policy.Tier);
        Assert.Equal(ttl, policy.Ttl);
    }

    [Fact]
    public void WithCaching_Distributed_PersistsLifetime()
    {
        var ttl = TimeSpan.FromHours(1);
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.Distributed<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                ttl))
            .Build();

        var policy = Assert.Single(options.CachePolicies).Value;
        Assert.Equal(CacheTier.Distributed, policy.Tier);
        Assert.Equal(ttl, policy.Ttl);
    }

    [Fact]
    public void WithCaching_Tiered_PersistsTierLifetimes()
    {
        var l1Ttl = TimeSpan.FromMinutes(2);
        var l2Ttl = TimeSpan.FromMinutes(30);
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.Tiered<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                l1Ttl,
                l2Ttl))
            .Build();

        var policy = Assert.Single(options.CachePolicies).Value;
        Assert.Equal(CacheTier.Tiered, policy.Tier);
        Assert.Equal(l1Ttl, policy.L1Ttl);
        Assert.Equal(l2Ttl, policy.L2Ttl);
    }

    [Fact]
    public void WithCaching_NotCached_PersistsBypassPolicy()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.NotCached<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default)))
            .Build();

        var policy = Assert.Single(options.CachePolicies).Value;
        Assert.False(policy.Enabled);
        Assert.Equal(CacheTier.None, policy.Tier);
    }

    [Fact]
    public void WithCaching_Override_PersistsExplicitOverrideOperation()
    {
        var policy = new CachePolicy
        {
            Tier = CacheTier.Distributed,
            Ttl = TimeSpan.FromMinutes(20),
        };
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.Override<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                policy))
            .Build();

        var operation = Assert.Single(options.CachePolicyOperations).Value;
        Assert.Equal(CachePolicyOperationKind.Override, operation.Kind);
        Assert.Same(policy, operation.Policy);
    }

    [Fact]
    public void WithCaching_Add_PersistsAddOperation()
    {
        var policy = new CachePolicy
        {
            Tier = CacheTier.InProcess,
            Ttl = TimeSpan.FromMinutes(5),
        };
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.Add<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                policy))
            .Build();

        var operation = Assert.Single(options.CachePolicyOperations).Value;
        Assert.Equal(CachePolicyOperationKind.Add, operation.Kind);
        Assert.Same(policy, operation.Policy);
    }

    [Fact]
    public void WithCaching_Disable_PersistsDisableOperationWithoutPolicy()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.Disable<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default)))
            .Build();

        var operation = Assert.Single(options.CachePolicyOperations).Value;
        Assert.Equal(CachePolicyOperationKind.Disable, operation.Kind);
        Assert.Null(operation.Policy);
    }

    [Fact]
    public void WithCaching_UseLibraryDefaults_OptsIn()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.UseLibraryDefaults())
            .Build();

        Assert.True(options.UseLibraryCacheDefaults);
    }

    [Fact]
    public void WithCaching_WithoutLibraryDefaults_OptsOutAfterOptIn()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCaching(cache => cache
                .UseLibraryDefaults()
                .WithoutLibraryDefaults())
            .Build();

        Assert.False(options.UseLibraryCacheDefaults);
    }

    [Fact]
    public void WithCaching_OverloadedMethods_UsesExactMethodIdentity()
    {
        var options = new TestApiOptionsBuilder()
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache
                .InMemory<IOverloadedApi, Task<string>>(api => api.Get(1), TimeSpan.FromMinutes(1))
                .Distributed<IOverloadedApi, Task<string>>(api => api.Get("one"), TimeSpan.FromMinutes(2)))
            .Build();

        Assert.Equal(2, options.CachePolicies.Count);
        Assert.Contains(options.CachePolicies, entry =>
            entry.Key.GetParameters()[0].ParameterType == typeof(int) &&
            entry.Value.Tier == CacheTier.InProcess);
        Assert.Contains(options.CachePolicies, entry =>
            entry.Key.GetParameters()[0].ParameterType == typeof(string) &&
            entry.Value.Tier == CacheTier.Distributed);
    }

    [Fact]
    public void WithCaching_SynchronousMethod_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithCaching(cache =>
            cache.InMemory<ISynchronousApi, string>(
                api => api.Get(),
                TimeSpan.FromMinutes(1))));

        Assert.Contains("Only API methods returning Task<T>", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WithCaching_NonGenericTaskMethod_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithCaching(cache =>
            cache.InMemory<INonGenericTaskApi, Task>(
                api => api.ExecuteAsync(),
                TimeSpan.FromMinutes(1))));

        Assert.Contains("Only API methods returning Task<T>", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WithCaching_NullConfigure_ThrowsArgumentNullException()
    {
        var builder = new TestApiOptionsBuilder();

        _ = Assert.Throws<ArgumentNullException>(() => builder.WithCaching(null!));
    }

    [Fact]
    public void WithCaching_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new TestApiOptionsBuilder();

        _ = Assert.Throws<ArgumentNullException>(() => builder.WithCaching(cache =>
            cache.InMemory<ITestApiClient, Task<string>>(null!, TimeSpan.FromMinutes(1))));
    }

    [Fact]
    public void WithCaching_MethodOutsideApiInterface_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();

        _ = Assert.Throws<ArgumentException>(() => builder.WithCaching(cache =>
            cache.InMemory<ITestApiClient, Task<string>>(
                _ => Task.FromResult("value"),
                TimeSpan.FromMinutes(1))));
    }

    [Fact]
    public void WithCaching_ReturnsConcreteBuilderForFluentChaining()
    {
        var builder = new TestApiOptionsBuilder()
            .WithCachePartition("tenant:123")
            .WithCaching(cache => cache.NotCached<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default)))
            .WithTestFeature();

        var options = builder
            .WithBaseUrl("https://api.example.com")
            .Build();

        Assert.True(options.EnableTestFeature);
        _ = Assert.Single(options.CachePolicies);
    }
}
