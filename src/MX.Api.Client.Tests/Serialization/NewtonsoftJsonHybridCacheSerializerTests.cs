using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MX.Api.Abstractions;
using MX.Api.Client.Serialization;
using MX.Caching;
using MX.Caching.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace MX.Api.Client.Tests.Serialization;

public class NewtonsoftJsonHybridCacheSerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_ExplicitPropertyNameAndInternalSetter_RoundTrips()
    {
        // Arrange
        var serializer = new NewtonsoftJsonHybridCacheSerializer<RestrictedSetterDto>();
        var target = new ArrayBufferWriter<byte>();
        var value = new RestrictedSetterDto
        {
            DisplayName = "cached value"
        };

        // Act
        serializer.Serialize(value, target);
        var result = serializer.Deserialize(new ReadOnlySequence<byte>(target.WrittenMemory));

        // Assert
        var json = Encoding.UTF8.GetString(target.WrittenSpan);
        Assert.Contains("\"display_name\":\"cached value\"", json, StringComparison.Ordinal);
        Assert.Equal(value.DisplayName, result.DisplayName);
    }

    [Fact]
    public void SerializeAndDeserialize_ApiResponseGraph_RoundTrips()
    {
        // Arrange
        var serializer = new NewtonsoftJsonHybridCacheSerializer<ApiResponse<string>>();
        var target = new ArrayBufferWriter<byte>();
        var value = new ApiResponse<string>("response data")
        {
            Errors =
            [
                new ApiError("outer", "Outer error", "request")
                {
                    Details = [new ApiError("inner", "Inner error")]
                }
            ],
            Pagination = new ApiPagination(20, 10, 2, 5),
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "cache"
            }
        };

        // Act
        serializer.Serialize(value, target);
        var result = serializer.Deserialize(new ReadOnlySequence<byte>(target.WrittenMemory));

        // Assert
        Assert.Equal("response data", result.Data);
        Assert.Equal("outer", Assert.Single(result.Errors!).Code);
        Assert.Equal("inner", Assert.Single(result.Errors![0].Details!).Code);
        Assert.Equal(20, result.Pagination!.TotalCount);
        Assert.True(result.Pagination.HasMore);
        Assert.Equal("cache", result.Metadata!["source"]);
    }

    [Fact]
    public void TryCreateSerializer_RegisteredAwaitedPayload_ReturnsNewtonsoftSerializerOnlyForThatType()
    {
        // Arrange
        var typeRegistry = new HybridCacheSerializerTypeRegistry();
        typeRegistry.AddMethods(typeof(IRestrictedSetterApiClient).GetMethods());
        var factory = new NewtonsoftJsonHybridCacheSerializerFactory(typeRegistry);

        // Act
        var created = factory.TryCreateSerializer<RestrictedSetterDto>(out var serializer);
        var unrelatedTypeCreated = factory.TryCreateSerializer<Guid>(out _);

        // Assert
        Assert.True(created);
        _ = Assert.IsType<NewtonsoftJsonHybridCacheSerializer<RestrictedSetterDto>>(serializer);
        Assert.False(unrelatedTypeCreated);
    }

    [Fact]
    public void Deserialize_MultiSegmentSequence_ReturnsValue()
    {
        // Arrange
        var serializer = new NewtonsoftJsonHybridCacheSerializer<RestrictedSetterDto>();
        var first = new BufferSegment(Encoding.UTF8.GetBytes("{\"display_"));
        var last = first.Append(Encoding.UTF8.GetBytes("name\":\"segmented\"}"));
        var source = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

        // Act
        var result = serializer.Deserialize(source);

        // Assert
        Assert.Equal("segmented", result.DisplayName);
    }

    [Fact]
    public async Task DistributedTier_RegisteredApiResultPayload_RoundTripsWithNewtonsoftSerializer()
    {
        // Arrange
        var distributedCache = new SharedDistributedCache();
        var cacheKey = CacheKeyBuilder.Create("v1", "serializer", "integration");
        var policy = new CachePolicy
        {
            Tier = CacheTier.Distributed,
            Ttl = TimeSpan.FromMinutes(1)
        };

        await using (var firstProvider = CreateServiceProvider(distributedCache))
        {
            var cache = firstProvider.GetRequiredService<IMxCache>();
            _ = await cache.GetOrCreateAsync(
                cacheKey,
                policy,
                _ => ValueTask.FromResult(new ApiResult<RestrictedSetterDto>(
                    HttpStatusCode.OK,
                    new ApiResponse<RestrictedSetterDto>(new RestrictedSetterDto
                    {
                        DisplayName = "cached value"
                    }))));
        }

        await using var secondProvider = CreateServiceProvider(distributedCache);
        var secondCache = secondProvider.GetRequiredService<IMxCache>();
        var factoryCalled = false;

        // Act
        var result = await secondCache.GetOrCreateAsync(
            cacheKey,
            policy,
            _ =>
            {
                factoryCalled = true;
                return ValueTask.FromException<ApiResult<RestrictedSetterDto>>(
                    new InvalidOperationException("The distributed cache entry was not used."));
            });

        // Assert
        Assert.False(factoryCalled);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("cached value", result.Result!.Data!.DisplayName);
    }

    private static ServiceProvider CreateServiceProvider(IDistributedCache distributedCache)
    {
        var typeRegistry = new HybridCacheSerializerTypeRegistry();
        typeRegistry.AddMethods(typeof(IRestrictedSetterApiClient).GetMethods());

        var services = new ServiceCollection();
        _ = services.AddSingleton(distributedCache);
        _ = services.AddSingleton(typeRegistry);
        _ = services.AddSingleton<Microsoft.Extensions.Caching.Hybrid.IHybridCacheSerializerFactory,
            NewtonsoftJsonHybridCacheSerializerFactory>();
        _ = services.AddMxCaching();

        return services.BuildServiceProvider();
    }

    private sealed class RestrictedSetterDto
    {
        [JsonProperty("display_name")]
        public string? DisplayName { get; internal set; }
    }

    private interface IRestrictedSetterApiClient
    {
        Task<RestrictedSetterDto> GetAsync();

        Task<ApiResult<RestrictedSetterDto>> GetResultAsync();
    }

    private sealed class SharedDistributedCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _entries = new();

        public byte[]? Get(string key)
        {
            return _entries.TryGetValue(key, out var value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _ = _entries.TryRemove(key, out _);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _entries[key] = value;
        }

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }

    private sealed class BufferSegment : ReadOnlySequenceSegment<byte>
    {
        public BufferSegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        public BufferSegment Append(ReadOnlyMemory<byte> memory)
        {
            var segment = new BufferSegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;
            return segment;
        }
    }
}
