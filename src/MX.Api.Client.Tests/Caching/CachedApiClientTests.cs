using System.Net;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client.Auth;
using MX.Api.Client.Caching;
using MX.Api.Client.Extensions;
using MX.Api.Client.Tests.TestClients;
using MX.Caching.Abstractions;

using RestSharp;

using Xunit;

namespace MX.Api.Client.Tests.Caching;

public class CachedApiClientTests
{
    [Fact]
    public async Task AddTypedApiClient_AddDoesNotSupersedeLibraryNotCachedGuard()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddDefaultCachePolicies<ICountingApiClient>(defaults => defaults
            .NotCached<ICountingApiClient, Task<string>>(
                api => api.GetValueAsync("value", default)));
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(builder => builder
            .WithBaseUrl("https://example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(caching => caching
                .UseLibraryDefaults()
                .Add<ICountingApiClient, Task<string>>(
                    api => api.GetValueAsync("value", default),
                    new CachePolicy
                    {
                        Tier = CacheTier.InProcess,
                        Ttl = TimeSpan.FromMinutes(5),
                    })));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetValueAsync("value", CancellationToken.None);
        var second = await client.GetValueAsync("value", CancellationToken.None);

        Assert.NotEqual(first, second);
        Assert.Equal(0, cache.FactoryCallCount);
        Assert.Null(cache.Policy);
    }

    [Fact]
    public async Task AddTypedApiClient_OverrideSupersedesLibraryNotCachedGuard()
    {
        var overridePolicy = new CachePolicy
        {
            Tier = CacheTier.InProcess,
            Ttl = TimeSpan.FromMinutes(7),
            Tags = ["consumer"],
        };
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddDefaultCachePolicies<ICountingApiClient>(defaults => defaults
            .NotCached<ICountingApiClient, Task<string>>(
                api => api.GetValueAsync("value", default)));
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(builder => builder
            .WithBaseUrl("https://example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(caching => caching
                .UseLibraryDefaults()
                .Override<ICountingApiClient, Task<string>>(
                    api => api.GetValueAsync("value", default),
                    overridePolicy)));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetValueAsync("value", CancellationToken.None);
        var second = await client.GetValueAsync("value", CancellationToken.None);

        Assert.Equal(first, second);
        Assert.Equal(1, cache.FactoryCallCount);
        Assert.Equal(overridePolicy, cache.Policy);
    }

    [Fact]
    public async Task AddTypedApiClient_DisableBypassesLibraryDefault()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddDefaultCachePolicies<ICountingApiClient>(defaults => defaults
            .InMemory<ICountingApiClient, Task<string>>(
                api => api.GetValueAsync("value", default),
                TimeSpan.FromMinutes(5)));
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(builder => builder
            .WithBaseUrl("https://example.com")
            .WithCachePartition("tenant:123")
            .WithCaching(caching => caching
                .UseLibraryDefaults()
                .Disable<ICountingApiClient, Task<string>>(
                    api => api.GetValueAsync("value", default))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetValueAsync("value", CancellationToken.None);
        var second = await client.GetValueAsync("value", CancellationToken.None);

        Assert.NotEqual(first, second);
        Assert.Equal(0, cache.FactoryCallCount);
        Assert.Null(cache.Policy);
    }

    [Fact]
    public async Task AddTypedApiClient_ConfiguredTaskMethod_UsesCache()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.InMemory<ICountingApiClient, Task<string>>(
                    client => client.GetValueAsync("value", default),
                    TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();
        using var cancellationTokenSource = new CancellationTokenSource();

        var first = await client.GetValueAsync("value", cancellationTokenSource.Token);
        var second = await client.GetValueAsync("value", cancellationTokenSource.Token);

        Assert.Equal("value:1", first);
        Assert.Equal(first, second);
        Assert.Equal(1, cache.FactoryCallCount);
        Assert.Equal(CacheTier.InProcess, cache.Policy?.Tier);
        Assert.Equal(cancellationTokenSource.Token, cache.CancellationToken);
        var key = Assert.IsType<CacheKey>(cache.Key);
        Assert.StartsWith("mx-api:v2:", key.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("value", key.Value, StringComparison.Ordinal);
        Assert.Equal(204, key.Value.Length);
    }

    [Fact]
    public async Task AddTypedApiClient_SameEndpointMethodAndArguments_UsesDeterministicCacheKey()
    {
        var firstKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "tenant:123", "value");
        var secondKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "tenant:123", "value");

        Assert.Equal(firstKey, secondKey);
    }

    [Fact]
    public async Task AddTypedApiClient_DifferentCachePartitions_UseDistinctCacheKeys()
    {
        var firstKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "first-secret-identity", "value");
        var secondKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "second-secret-identity", "value");

        Assert.NotEqual(firstKey, secondKey);
        Assert.DoesNotContain("first-secret-identity", firstKey, StringComparison.Ordinal);
        Assert.DoesNotContain("second-secret-identity", secondKey, StringComparison.Ordinal);
        Assert.Equal(204, firstKey.Length);
        Assert.Equal(204, secondKey.Length);
    }

    [Fact]
    public async Task AddTypedApiClient_DifferentArguments_UseDistinctCacheKeys()
    {
        var firstKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "tenant:123", "first-secret");
        var secondKey = await CaptureCacheKeyAsync("https://api.example.com/v1", "tenant:123", "second-secret");

        Assert.NotEqual(firstKey, secondKey);
        Assert.DoesNotContain("first-secret", firstKey, StringComparison.Ordinal);
        Assert.DoesNotContain("second-secret", secondKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddTypedApiClient_DifferentEndpoints_UseDistinctCacheKeys()
    {
        var firstKey = await CaptureCacheKeyAsync("https://first.example.com/v1", "tenant:123", "value");
        var secondKey = await CaptureCacheKeyAsync("https://second.example.com/v1", "tenant:123", "value");

        Assert.NotEqual(firstKey, secondKey);
        Assert.DoesNotContain("first.example.com", firstKey, StringComparison.Ordinal);
        Assert.DoesNotContain("second.example.com", secondKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddTypedApiClient_EndpointQueryAndFragment_DoNotAffectCacheKey()
    {
        var firstKey = await CaptureCacheKeyAsync(
            "https://api.example.com/v1?api-key=first-secret#first-fragment",
            "tenant:123",
            "value");
        var secondKey = await CaptureCacheKeyAsync(
            "https://api.example.com/v1?api-key=second-secret#second-fragment",
            "tenant:123",
            "value");

        Assert.Equal(firstKey, secondKey);
        Assert.DoesNotContain("first-secret", firstKey, StringComparison.Ordinal);
        Assert.DoesNotContain("second-secret", secondKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddTypedApiClient_LargeArgument_UsesBoundedCacheKey()
    {
        var argument = new string('x', 100_000);

        var key = await CaptureCacheKeyAsync("https://api.example.com/v1", "tenant:123", argument);

        Assert.Equal(204, key.Length);
        Assert.DoesNotContain(argument, key, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddTypedApiClient_ConcurrentSameKeyCalls_InvokeUnderlyingClientOnce()
    {
        var state = new GatedApiClientState();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddTypedApiClient<IGatedApiClient, GatedApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.InMemory<IGatedApiClient, Task<string>>(
                    client => client.GetValueAsync("value", default),
                    TimeSpan.FromMinutes(5))));
        _ = services.AddSingleton<IRestClientService>(new GatedRestClientService(state));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IGatedApiClient>();
        var calls = Enumerable.Range(0, 5)
            .Select(_ => client.GetValueAsync("value", CancellationToken.None))
            .ToArray();

        _ = await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        try
        {
            Assert.Equal(1, state.CallCount);
        }
        finally
        {
            _ = state.Release.TrySetResult(true);
        }

        var results = await Task.WhenAll(calls);

        Assert.All(results, result => Assert.Equal("value:1", result));
        Assert.Equal(1, state.CallCount);
    }

    [Fact]
    public async Task AddTypedApiClient_ConfiguredTags_PassThroughToCache()
    {
        string[] tags = ["client", "operation"];
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.Add<ICountingApiClient, Task<string>>(
                    client => client.GetValueAsync("value", default),
                    new CachePolicy
                    {
                        Tier = CacheTier.InProcess,
                        Ttl = TimeSpan.FromMinutes(5),
                        Tags = tags,
                    })));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        _ = await client.GetValueAsync("value", CancellationToken.None);

        Assert.Equal(tags, cache.Policy?.Tags);
    }

    [Fact]
    public async Task AddTypedApiClient_InternalServerErrorResult_DoesNotCache()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.InMemory<ICountingApiClient, Task<ApiResult<string>>>(
                    client => client.GetResult(default),
                    TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetResult();
        var second = await client.GetResult();

        Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, second.StatusCode);
        Assert.Equal(2, cache.FactoryCallCount);
    }

    [Fact]
    public async Task AddTypedApiClient_SuccessfulResult_UsesCache()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.InMemory<ICountingApiClient, Task<ApiResult<string>>>(
                    client => client.GetSuccessfulResult(default),
                    TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetSuccessfulResult();
        var second = await client.GetSuccessfulResult();

        Assert.True(first.IsSuccess);
        Assert.Equal("value:1", first.Result?.Data);
        Assert.Same(first, second);
        Assert.Equal(1, cache.FactoryCallCount);
    }

    [Fact]
    public async Task AddTypedApiClient_NotFoundResult_UsesCache()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching.InMemory<ICountingApiClient, Task<ApiResult<string>>>(
                    client => client.GetNotFoundResult(default),
                    TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var first = await client.GetNotFoundResult();
        var second = await client.GetNotFoundResult();

        Assert.True(first.IsNotFound);
        Assert.Same(first, second);
        Assert.Equal(1, cache.FactoryCallCount);
    }

    [Fact]
    public async Task AddTypedApiClient_ConfiguredOverloads_UseDistinctCacheEntries()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching
                    .InMemory<ICountingApiClient, Task<string>>(
                        client => client.GetOverloadedValueAsync("1", default),
                        TimeSpan.FromMinutes(5))
                    .InMemory<ICountingApiClient, Task<string>>(
                        client => client.GetOverloadedValueAsync(1, default),
                        TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var firstString = await client.GetOverloadedValueAsync("1");
        var secondString = await client.GetOverloadedValueAsync("1");
        var firstInteger = await client.GetOverloadedValueAsync(1);
        var secondInteger = await client.GetOverloadedValueAsync(1);

        Assert.Equal("string:1:1", firstString);
        Assert.Equal(firstString, secondString);
        Assert.Equal("integer:1:2", firstInteger);
        Assert.Equal(firstInteger, secondInteger);
        Assert.Equal(2, cache.FactoryCallCount);
        Assert.Equal(2, cache.EntryCount);
    }

    [Fact]
    public async Task AddTypedApiClient_IdenticalInheritedMethods_UseDistinctCacheEntries()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<IInheritedApiClient, InheritedApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching
                    .InMemory<IFirstInheritedApiClient, Task<string>>(
                        client => client.GetValueAsync("shared", default),
                        TimeSpan.FromMinutes(5))
                    .InMemory<ISecondInheritedApiClient, Task<string>>(
                        client => client.GetValueAsync("shared", default),
                        TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IInheritedApiClient>();

        var firstClient = (IFirstInheritedApiClient)client;
        var secondClient = (ISecondInheritedApiClient)client;
        var firstResult = await firstClient.GetValueAsync("shared");
        var secondResult = await secondClient.GetValueAsync("shared");
        var cachedFirstResult = await firstClient.GetValueAsync("shared");
        var cachedSecondResult = await secondClient.GetValueAsync("shared");

        Assert.Equal("first:shared:1", firstResult);
        Assert.Equal("second:shared:2", secondResult);
        Assert.Equal(firstResult, cachedFirstResult);
        Assert.Equal(secondResult, cachedSecondResult);
        Assert.Equal(2, cache.FactoryCallCount);
        Assert.Equal(2, cache.EntryCount);
    }

    [Fact]
    public async Task AddTypedApiClient_DifferentClosedGenericMethods_UseDistinctCacheEntries()
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl("https://api.example.com")
                .WithCachePartition("tenant:123")
                .WithCaching(caching => caching
                    .InMemory<ICountingApiClient, Task<string>>(
                        client => client.GetGenericValueAsync<int>("shared", default),
                        TimeSpan.FromMinutes(5))
                    .InMemory<ICountingApiClient, Task<string>>(
                        client => client.GetGenericValueAsync<string>("shared", default),
                        TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        var firstInteger = await client.GetGenericValueAsync<int>("shared");
        var secondInteger = await client.GetGenericValueAsync<int>("shared");
        var firstString = await client.GetGenericValueAsync<string>("shared");
        var secondString = await client.GetGenericValueAsync<string>("shared");

        Assert.Equal("Int32:shared:1", firstInteger);
        Assert.Equal(firstInteger, secondInteger);
        Assert.Equal("String:shared:2", firstString);
        Assert.Equal(firstString, secondString);
        Assert.Equal(2, cache.FactoryCallCount);
        Assert.Equal(2, cache.EntryCount);
    }

    [Fact]
    public void GetMethodIdentity_SameFullNameFromDifferentAssemblies_UsesDistinctIdentities()
    {
        var firstMethod = CreateDynamicInterfaceMethod("First.Cache.Contracts");
        var secondMethod = CreateDynamicInterfaceMethod("Second.Cache.Contracts");

        var firstIdentity = CachedApiClientProxy<ICountingApiClient>.GetMethodIdentity(firstMethod);
        var secondIdentity = CachedApiClientProxy<ICountingApiClient>.GetMethodIdentity(secondMethod);

        Assert.NotEqual(firstIdentity, secondIdentity);
    }

    private static MethodInfo CreateDynamicInterfaceMethod(string assemblyName)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule(assemblyName);
        var type = module.DefineType(
            "Shared.Contracts.ICollidingApiClient",
            TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
        _ = type.DefineMethod(
            "GetValueAsync",
            MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual,
            typeof(Task<string>),
            [typeof(string)]);

        return type.CreateType()!.GetMethod("GetValueAsync")!;
    }

    private static async Task<string> CaptureCacheKeyAsync(
        string baseUrl,
        string cachePartition,
        string argument)
    {
        var cache = new RecordingCache();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddSingleton<IMxCache>(cache);
        _ = services.AddTypedApiClient<ICountingApiClient, CountingApiClient, TestApiOptions, TestApiOptionsBuilder>(
            options => options
                .WithBaseUrl(baseUrl)
                .WithCachePartition(cachePartition)
                .WithCaching(caching => caching.InMemory<ICountingApiClient, Task<string>>(
                    client => client.GetValueAsync("value", default),
                    TimeSpan.FromMinutes(5))));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ICountingApiClient>();

        _ = await client.GetValueAsync(argument, CancellationToken.None);

        return Assert.IsType<string>(cache.Key?.Value);
    }

    private interface ICountingApiClient
    {
        Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default);

        Task<ApiResult<string>> GetResult(
            CancellationToken cancellationToken = default);

        Task<ApiResult<string>> GetSuccessfulResult(
            CancellationToken cancellationToken = default);

        Task<ApiResult<string>> GetNotFoundResult(
            CancellationToken cancellationToken = default);

        Task<string> GetOverloadedValueAsync(
            string value,
            CancellationToken cancellationToken = default);

        Task<string> GetOverloadedValueAsync(
            int value,
            CancellationToken cancellationToken = default);

        Task<string> GetGenericValueAsync<T>(
            string value,
            CancellationToken cancellationToken = default);
    }

    private interface IFirstInheritedApiClient
    {
        Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default);
    }

    private interface ISecondInheritedApiClient
    {
        Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default);
    }

    private interface IInheritedApiClient : IFirstInheritedApiClient, ISecondInheritedApiClient
    {
    }

    private sealed class InheritedApiClient(
        ILogger<BaseApi<TestApiOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        TestApiOptions options)
        : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), IInheritedApiClient
    {
        private int _callCount;

        Task<string> IFirstInheritedApiClient.GetValueAsync(
            string value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"first:{value}:{_callCount}");
        }

        Task<string> ISecondInheritedApiClient.GetValueAsync(
            string value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"second:{value}:{_callCount}");
        }
    }

    private sealed class CountingApiClient(
        ILogger<BaseApi<TestApiOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        TestApiOptions options)
        : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), ICountingApiClient
    {
        private int _callCount;

        public Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"{value}:{_callCount}");
        }

        public Task<ApiResult<string>> GetResult(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult(
                new ApiResult<string>(HttpStatusCode.InternalServerError));
        }

        public Task<ApiResult<string>> GetSuccessfulResult(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult(
                new ApiResult<string>(
                    HttpStatusCode.OK,
                    new ApiResponse<string>($"value:{_callCount}")));
        }

        public Task<ApiResult<string>> GetNotFoundResult(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult(
                new ApiResult<string>(HttpStatusCode.NotFound));
        }

        public Task<string> GetOverloadedValueAsync(
            string value,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"string:{value}:{_callCount}");
        }

        public Task<string> GetOverloadedValueAsync(
            int value,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"integer:{value}:{_callCount}");
        }

        public Task<string> GetGenericValueAsync<T>(
            string value,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callCount++;
            return Task.FromResult($"{typeof(T).Name}:{value}:{_callCount}");
        }
    }

    private interface IGatedApiClient
    {
        Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default);
    }

    private sealed class GatedApiClient(
        ILogger<BaseApi<TestApiOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        TestApiOptions options)
        : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), IGatedApiClient
    {
        private readonly GatedApiClientState _state = ((GatedRestClientService)restClientService).State;

        public async Task<string> GetValueAsync(
            string value,
            CancellationToken cancellationToken = default)
        {
            var invocation = Interlocked.Increment(ref _state.CallCount);
            _ = _state.Started.TrySetResult(true);
            _ = await _state.Release.Task.WaitAsync(cancellationToken);
            return $"{value}:{invocation}";
        }
    }

    private sealed class GatedRestClientService(GatedApiClientState state) : IRestClientService
    {
        public GatedApiClientState State { get; } = state;

        public Task<RestResponse> ExecuteAsync(
            string baseUrl,
            RestRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<RestResponse> ExecuteWithNamedOptionsAsync(
            string optionsName,
            RestRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }

    private sealed class GatedApiClientState
    {
        public int CallCount;

        public TaskCompletionSource<bool> Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class RecordingCache : IMxCache
    {
        private readonly Dictionary<CacheKey, object?> _values = [];

        public CacheKey? Key { get; private set; }

        public CachePolicy? Policy { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public int FactoryCallCount { get; private set; }

        public int EntryCount => _values.Count;

        public async Task<T> GetOrCreateAsync<T>(
            CacheKey key,
            CachePolicy policy,
            Func<CancellationToken, ValueTask<T>> factory,
            CancellationToken cancellationToken = default)
        {
            Key = key;
            Policy = policy;
            CancellationToken = cancellationToken;

            if (_values.TryGetValue(key, out var value))
            {
                return (T)value!;
            }

            FactoryCallCount++;
            var created = await factory(cancellationToken);
            _values[key] = created;
            return created;
        }

        public Task<CacheReadResult<T>> TryGetAsync<T>(
            CacheKey key,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SetAsync<T>(
            CacheKey key,
            T value,
            CachePolicy policy,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAsync(
            CacheKey key,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RemoveByTagAsync(
            string tag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
