using MX.Api.Client.Configuration;
using MX.Api.Client.Tests.TestClients;
using MX.Caching.Abstractions;
using Xunit;

namespace MX.Api.Client.Tests;

/// <summary>
/// Unit tests for <see cref="SharedCacheConfiguration"/> and the
/// <c>ApiClientOptionsBuilder&lt;,&gt;.WithSharedCaching</c> filtering / validation behavior.
/// </summary>
public class SharedCacheConfigurationTests
{
    [Fact]
    public void Ctor_NullConfigure_ThrowsArgumentNullException()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new SharedCacheConfiguration(null!));
    }

    [Fact]
    public void WithSharedCaching_NullConfiguration_ThrowsArgumentNullException()
    {
        var builder = new TestApiOptionsBuilder();

        _ = Assert.Throws<ArgumentNullException>(() => builder.WithSharedCaching(null!));
    }

    [Fact]
    public void WithSharedCaching_AppliesOnlyOperationsMatchingConfiguredClientType()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1))
            .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));

        var builderA = new TestApiOptionsBuilder();
        builderA.SetConfiguredClientType(typeof(ISubApiA));
        var optionsA = builderA
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:1")
            .WithSharedCaching(shared)
            .Build();

        var opA = Assert.Single(optionsA.CachePolicyOperations);
        Assert.Equal(typeof(ISubApiA), opA.Key.DeclaringType);
        Assert.Equal(CachePolicyOperationKind.Add, opA.Value.Kind);
        Assert.Equal(CacheTier.InProcess, opA.Value.Policy!.Tier);
    }

    [Fact]
    public void WithSharedCaching_SkipsSiblingSubApiOperationsInsteadOfThrowing()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1))
            .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));

        var builderB = new TestApiOptionsBuilder();
        builderB.SetConfiguredClientType(typeof(ISubApiB));

        // Would have thrown under the classic WithCaching(...) scope check.
        var optionsB = builderB
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:1")
            .WithSharedCaching(shared)
            .Build();

        var opB = Assert.Single(optionsB.CachePolicyOperations);
        Assert.Equal(typeof(ISubApiB), opB.Key.DeclaringType);
        Assert.Equal(CachePolicyOperationKind.Disable, opB.Value.Kind);
    }

    [Fact]
    public void WithSharedCaching_UseLibraryDefaults_AppliesToEveryOptedInClient()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .UseLibraryDefaults()
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1)));

        var builderA = new TestApiOptionsBuilder();
        builderA.SetConfiguredClientType(typeof(ISubApiA));
        var optionsA = builderA
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:1")
            .WithSharedCaching(shared)
            .Build();

        var builderB = new TestApiOptionsBuilder();
        builderB.SetConfiguredClientType(typeof(ISubApiB));
        var optionsB = builderB
            .WithBaseUrl("https://api.example.com")
            .WithCachePartition("tenant:1")
            .WithSharedCaching(shared)
            .Build();

        Assert.True(optionsA.UseLibraryCacheDefaults);
        Assert.True(optionsB.UseLibraryCacheDefaults);
    }

    [Fact]
    public void ValidateAllOperationsMatched_WithNoOperationsAndNoApplication_Succeeds()
    {
        var shared = new SharedCacheConfiguration(_ => { });

        shared.ValidateAllOperationsMatched();
    }

    [Fact]
    public void ValidateAllOperationsMatched_NeverApplied_ThrowsWhenOperationsWereCaptured()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1)));

        var ex = Assert.Throws<InvalidOperationException>(shared.ValidateAllOperationsMatched);
        Assert.Contains("never applied", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateAllOperationsMatched_AllOperationsMatched_Succeeds()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1))
            .NotCached<ISubApiB, Task<string>>(api => api.GetB(2, default)));

        var builderA = new TestApiOptionsBuilder();
        builderA.SetConfiguredClientType(typeof(ISubApiA));
        _ = builderA.WithBaseUrl("https://api.example.com").WithCachePartition("t").WithSharedCaching(shared);

        var builderB = new TestApiOptionsBuilder();
        builderB.SetConfiguredClientType(typeof(ISubApiB));
        _ = builderB.WithBaseUrl("https://api.example.com").WithCachePartition("t").WithSharedCaching(shared);

        shared.ValidateAllOperationsMatched();
    }

    [Fact]
    public void ValidateAllOperationsMatched_UnregisteredInterface_ThrowsWithClearMessage()
    {
        var shared = new SharedCacheConfiguration(cache => cache
            .InMemory<ISubApiA, Task<string>>(api => api.GetA(1, default), TimeSpan.FromMinutes(1))
            .InMemory<IUnregisteredSubApi, Task<string>>(api => api.GetUnregistered(1, default), TimeSpan.FromMinutes(1)));

        var builderA = new TestApiOptionsBuilder();
        builderA.SetConfiguredClientType(typeof(ISubApiA));
        _ = builderA.WithBaseUrl("https://api.example.com").WithCachePartition("t").WithSharedCaching(shared);

        var ex = Assert.Throws<InvalidOperationException>(shared.ValidateAllOperationsMatched);
        Assert.Contains(nameof(IUnregisteredSubApi), ex.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(IUnregisteredSubApi.GetUnregistered), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SharedCacheConfiguration_Ctor_SynchronousMethod_ThrowsAtCaptureTime()
    {
        // Genuine typos in the delegate itself still surface eagerly.
        _ = Assert.Throws<ArgumentException>(() => new SharedCacheConfiguration(cache =>
            cache.InMemory<ISynchronousShim, string>(api => api.Get(), TimeSpan.FromMinutes(1))));
    }

    /// <summary>
    /// Regression: existing single-client <c>WithCaching(...)</c> continues to throw on a real scope mismatch,
    /// preserving typo safety.
    /// </summary>
    [Fact]
    public void WithCaching_SingleClient_ScopeMismatch_StillThrows()
    {
        var builderA = new TestApiOptionsBuilder();
        builderA.SetConfiguredClientType(typeof(ISubApiA));

        _ = Assert.Throws<ArgumentException>(() => builderA.WithCaching(cache => cache
            .InMemory<ISubApiB, Task<string>>(api => api.GetB(1, default), TimeSpan.FromMinutes(1))));
    }

    private interface ISynchronousShim
    {
        string Get();
    }
}
