using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MX.Api.Client.Caching;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.Client.Tests.TestClients;
using MX.Caching.Abstractions;
using Xunit;

namespace MX.Api.Client.Tests;

public class DefaultCachePolicyTests
{
    private static readonly MethodInfo GetDataMethod = typeof(ITestApiClient)
        .GetMethod(nameof(ITestApiClient.GetDataAsync))!;

    [Fact]
    public void AddDefaultCachePolicies_RegistersPoliciesForExactClientAndMethod()
    {
        var services = new ServiceCollection();

        _ = services.AddDefaultCachePolicies<ITestApiClient>(cache => cache
            .InMemory<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                TimeSpan.FromMinutes(3)));

        using var serviceProvider = services.BuildServiceProvider();
        var defaults = serviceProvider.GetRequiredService<DefaultCachePolicies<ITestApiClient>>();

        var policy = Assert.Single(defaults.Policies);
        Assert.Same(GetDataMethod, policy.Key);
        Assert.Equal(CacheTier.InProcess, policy.Value.Tier);
    }

    [Fact]
    public void AddDefaultCachePolicies_IsolatesDefaultsByClientType()
    {
        var services = new ServiceCollection();

        _ = services.AddDefaultCachePolicies<ITestApiClient>(cache => cache
            .InMemory<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                TimeSpan.FromMinutes(3)));

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<DefaultCachePolicies<ITestApiClient>>());
        Assert.Null(serviceProvider.GetService<DefaultCachePolicies<IStandardTestApiClient>>());
    }

    [Fact]
    public void Resolve_WithoutOptIn_IgnoresLibraryDefault()
    {
        var options = new TestApiOptions();
        var libraryDefault = CreatePolicy(CacheTier.InProcess);

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            libraryDefault);

        Assert.Equal(CachePolicy.NotCached, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithOptIn_UsesLibraryDefault()
    {
        var options = BuildOptions(cache => cache.UseLibraryDefaults());
        var libraryDefault = CreatePolicy(CacheTier.InProcess);

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            libraryDefault);

        Assert.Same(libraryDefault, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithOptOutAfterOptIn_IgnoresLibraryDefault()
    {
        var options = BuildOptions(cache => cache
            .UseLibraryDefaults()
            .WithoutLibraryDefaults());
        var libraryDefault = CreatePolicy(CacheTier.InProcess);

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            libraryDefault);

        Assert.Equal(CachePolicy.NotCached, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithDisable_DisablesCaching()
    {
        var options = BuildOptions(cache => cache
            .UseLibraryDefaults()
            .Disable<ITestApiClient, Task<string>>(api => api.GetDataAsync(default)));

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            CreatePolicy(CacheTier.InProcess));

        Assert.Equal(CachePolicy.NotCached, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithAddAndNoLibraryDefault_UsesAddedPolicy()
    {
        var addedPolicy = CreatePolicy(CacheTier.Distributed);
        var options = BuildOptions(cache => cache
            .Add<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                addedPolicy));

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            libraryDefault: null);

        Assert.Same(addedPolicy, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithAdd_DoesNotSupersedeLibraryNotCachedGuard()
    {
        var addedPolicy = CreatePolicy(CacheTier.Distributed);
        var options = BuildOptions(cache => cache
            .UseLibraryDefaults()
            .Add<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                addedPolicy));

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            CachePolicy.NotCached);

        Assert.Equal(CachePolicy.NotCached, selection.LibraryDefault);
        Assert.Null(selection.ConsumerOverride);
    }

    [Fact]
    public void Resolve_WithOverride_SupersedesLibraryNotCachedGuard()
    {
        var overridePolicy = CreatePolicy(CacheTier.Distributed);
        var options = BuildOptions(cache => cache
            .UseLibraryDefaults()
            .Override<ITestApiClient, Task<string>>(
                api => api.GetDataAsync(default),
                overridePolicy));

        var selection = EffectiveCachePolicySelector.Resolve(
            GetDataMethod,
            options,
            CachePolicy.NotCached);

        Assert.Equal(CachePolicy.NotCached, selection.LibraryDefault);
        Assert.Same(overridePolicy, selection.ConsumerOverride);
    }

    private static TestApiOptions BuildOptions(Action<CacheBuilder> configure)
    {
        var builder = new TestApiOptionsBuilder();
        _ = builder.WithBaseUrl("https://example.com");
        _ = builder.WithCachePartition("test");
        _ = builder.WithCaching(configure);
        return builder.Build();
    }

    private static CachePolicy CreatePolicy(CacheTier tier)
    {
        return new CachePolicy
        {
            Tier = tier,
            Ttl = TimeSpan.FromMinutes(5),
        };
    }
}
