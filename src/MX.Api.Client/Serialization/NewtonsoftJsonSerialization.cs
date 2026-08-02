using System.Buffers;
using System.Text;

using Microsoft.Extensions.Caching.Hybrid;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MX.Api.Client.Serialization;

internal static class NewtonsoftJsonSettings
{
    internal static JsonSerializerSettings Create()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}

internal sealed class NewtonsoftJsonHybridCacheSerializer<T> : IHybridCacheSerializer<T>
{
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        var json = source.IsSingleSegment
            ? Encoding.UTF8.GetString(source.FirstSpan)
            : Encoding.UTF8.GetString(source.ToArray());

        return JsonConvert.DeserializeObject<T>(json, NewtonsoftJsonSettings.Create())!;
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        var json = JsonConvert.SerializeObject(value, NewtonsoftJsonSettings.Create());
        var byteCount = Encoding.UTF8.GetByteCount(json);
        var destination = target.GetSpan(byteCount);
        var bytesWritten = Encoding.UTF8.GetBytes(json, destination);
        target.Advance(bytesWritten);
    }
}

internal sealed class NewtonsoftJsonHybridCacheSerializerFactory(
    HybridCacheSerializerTypeRegistry typeRegistry) : IHybridCacheSerializerFactory
{
    public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T> serializer)
    {
        if (!typeRegistry.Contains(typeof(T)))
        {
            serializer = null!;
            return false;
        }

        serializer = new NewtonsoftJsonHybridCacheSerializer<T>();
        return true;
    }
}
