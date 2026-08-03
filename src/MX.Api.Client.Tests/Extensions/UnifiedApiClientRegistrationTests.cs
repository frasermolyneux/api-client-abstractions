using Microsoft.Extensions.DependencyInjection;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.Client.Tests.TestClients;
using MX.Caching.Abstractions;
using Xunit;

namespace MX.Api.Client.Tests.Extensions;

/// <summary>
/// DI-composition regression tests for the "unified API client" registration scenario.
/// A downstream unified client registers many typed sub-API clients by passing the SAME
/// configureOptions delegate to <c>AddTypedApiClient</c> for each sub-API. When that delegate
/// contains cache expressions against multiple sub-APIs, the classic per-client scope check
/// throws when the delegate is executed for a sibling sub-API — aborting host startup.
///
/// These tests lock in the new <see cref="SharedCacheConfiguration"/> capability that captures
/// the delegate once and applies only the matching operations per registered sub-API, and
/// prove the old <see cref="ApiClientOptionsBuilder{TOptions, TBuilder}.WithCaching(Action{CacheBuilder})"/>
/// path continues to throw so single-client typo safety is preserved.
/// </summary>
public class UnifiedApiClientRegistrationTests
{
    /// <summary>
    /// Reproduces the production incident: sharing one <c>WithCaching(...)</c> delegate that mixes
    /// cache expressions from multiple sub-APIs across per-client registrations must still throw.
    /// This is intentional — proves the safety net for genuine single-client typos is intact.
    /// </summary>
    [Fact]
    public void SharedWithCachingDelegate_AcrossMultipleTypedClients_StillThrows_ProvingRepro()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(new FakeMxCache());

        Action<TestApiOptionsBuilder> shared = SharedConfigureThatThrows;

        // First typed client registration executes the delegate scoped to ISubApiA. The ISubApiB
        // expression inside the delegate hits the classic scope check and throws immediately.
        _ = Assert.Throws<ArgumentException>(() =>
            services.AddTypedApiClient<ISubApiA, SubApiA, TestApiOptions, TestApiOptionsBuilder>(shared));

        static void SharedConfigureThatThrows(TestApiOptionsBuilder builder)
        {
            _ = builder
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:1")
                .WithCaching(cache => cache
                    .UseLibraryDefaults()
                    .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromSeconds(60))
                    .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));
        }
    }

    /// <summary>
    /// The fix: swap <c>WithCaching(...)</c> for <c>WithSharedCaching(sharedCacheConfiguration)</c>.
    /// The unified client keeps its one-delegate-for-all pattern; each per-client registration only
    /// applies the operations whose declaring interface is assignable from the current typed client.
    /// </summary>
    [Fact]
    public void SharedWithCachingDelegate_ViaSharedCacheConfiguration_ComposesAllTypedClientsCleanly()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(new FakeMxCache());

        var sharedCache = new SharedCacheConfiguration(cache => cache
            .UseLibraryDefaults()
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromSeconds(60))
            .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));

        // Register three sub-APIs against the same shared delegate.
        _ = services.AddTypedApiClient<ISubApiA, SubApiA, TestApiOptions, TestApiOptionsBuilder>(Shared);
        _ = services.AddTypedApiClient<ISubApiB, SubApiB, TestApiOptions, TestApiOptionsBuilder>(Shared);
        _ = services.AddTypedApiClient<ISubApiC, SubApiC, TestApiOptions, TestApiOptionsBuilder>(Shared);

        // Finalize: assert every captured operation matched at least one registered client.
        sharedCache.ValidateAllOperationsMatched();

        // BuildServiceProvider must succeed — this is the exact assertion that would have caught the incident.
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ISubApiA>());
        Assert.NotNull(provider.GetService<ISubApiB>());
        Assert.NotNull(provider.GetService<ISubApiC>());

        void Shared(TestApiOptionsBuilder builder)
        {
            _ = builder
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:1")
                .WithSharedCaching(sharedCache);
        }
    }

    /// <summary>
    /// Each cache operation must land ONLY on the typed client whose interface declares it — no cross-client bleed.
    /// </summary>
    [Fact]
    public void SharedCacheConfiguration_OperationsLandOnMatchingClientOnly_NoCrossClientBleed()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(new FakeMxCache());

        var sharedCache = new SharedCacheConfiguration(cache => cache
            .UseLibraryDefaults()
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromSeconds(60))
            .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));

        Action<TestApiOptionsBuilder> shared = Shared;

        _ = services.AddTypedApiClient<ISubApiA, SubApiA, TestApiOptions, TestApiOptionsBuilder>(shared);
        _ = services.AddTypedApiClient<ISubApiB, SubApiB, TestApiOptions, TestApiOptionsBuilder>(shared);
        _ = services.AddTypedApiClient<ISubApiC, SubApiC, TestApiOptions, TestApiOptionsBuilder>(shared);

        using var provider = services.BuildServiceProvider();
        var allOptions = provider.GetServices<TestApiOptions>().ToList();

        // Three registrations, three distinct options singletons.
        Assert.Equal(3, allOptions.Count);

        // Every options instance has UseLibraryCacheDefaults because the shared configuration opted in.
        Assert.All(allOptions, o => Assert.True(o.UseLibraryCacheDefaults));

        // Two ops total across the three registrations; the third client (ISubApiC) is unaffected by consumer ops.
        var totalOperations = allOptions.Sum(o => o.CachePolicyOperations.Count);
        Assert.Equal(2, totalOperations);

        foreach (var options in allOptions)
        {
            Assert.All(options.CachePolicyOperations, kvp => Assert.NotNull(kvp.Key.DeclaringType));
        }

        // Verify per-interface landing precisely.
        var byDeclaringType = allOptions
            .SelectMany(o => o.CachePolicyOperations.Select(kvp => (Options: o, kvp.Key, kvp.Value)))
            .GroupBy(x => x.Key.DeclaringType!)
            .ToDictionary(g => g.Key, g => g.ToList());

        Assert.True(byDeclaringType.ContainsKey(typeof(ISubApiA)));
        Assert.True(byDeclaringType.ContainsKey(typeof(ISubApiB)));
        Assert.False(byDeclaringType.ContainsKey(typeof(ISubApiC)),
            "ISubApiC has no shared cache operation, so no options instance should carry one for it.");
        Assert.Equal(CachePolicyOperationKind.Add, byDeclaringType[typeof(ISubApiA)][0].Value.Kind);
        Assert.Equal(CachePolicyOperationKind.Disable, byDeclaringType[typeof(ISubApiB)][0].Value.Kind);

        // ISubApiC still opted into library defaults but has no explicit consumer operations.
        var subCOptions = allOptions.Single(o =>
            o.CachePolicyOperations.All(x => x.Key.DeclaringType != typeof(ISubApiA))
            && o.CachePolicyOperations.All(x => x.Key.DeclaringType != typeof(ISubApiB)));
        Assert.Empty(subCOptions.CachePolicyOperations);
        Assert.True(subCOptions.UseLibraryCacheDefaults);

        void Shared(TestApiOptionsBuilder builder)
        {
            _ = builder
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:1")
                .WithSharedCaching(sharedCache);
        }
    }

    /// <summary>
    /// Negative test: an operation targeting an interface that is NOT a registered sub-API must still surface
    /// as a clear error via <see cref="SharedCacheConfiguration.ValidateAllOperationsMatched"/>, so real typos
    /// are not silently swallowed.
    /// </summary>
    [Fact]
    public void ValidateAllOperationsMatched_UnregisteredSubApi_SurfacesClearError()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(new FakeMxCache());

        var sharedCache = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromSeconds(60))
            .InMemory<IUnregisteredSubApi, Task<string>>(api => api.GetUnregistered(1, default), TimeSpan.FromSeconds(60)));

        Action<TestApiOptionsBuilder> shared = Shared;

        _ = services.AddTypedApiClient<ISubApiA, SubApiA, TestApiOptions, TestApiOptionsBuilder>(shared);

        var ex = Assert.Throws<InvalidOperationException>(sharedCache.ValidateAllOperationsMatched);
        Assert.Contains(nameof(IUnregisteredSubApi), ex.Message, StringComparison.Ordinal);

        void Shared(TestApiOptionsBuilder builder)
        {
            _ = builder
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:1")
                .WithSharedCaching(sharedCache);
        }
    }

    /// <summary>
    /// A minimal <see cref="IMxCache"/> so BuildServiceProvider succeeds without pulling any real cache infrastructure.
    /// </summary>
    private sealed class FakeMxCache : IMxCache
    {
        public Task<T> GetOrCreateAsync<T>(
            CacheKey key,
            CachePolicy policy,
            Func<CancellationToken, ValueTask<T>> factory,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("DI-composition tests do not exercise cache reads.");
        }

        public Task<CacheReadResult<T>> TryGetAsync<T>(CacheKey key, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SetAsync<T>(CacheKey key, T value, CachePolicy policy, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
