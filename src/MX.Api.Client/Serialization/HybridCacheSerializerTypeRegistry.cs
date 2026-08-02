using System.Collections.Concurrent;
using System.Reflection;

namespace MX.Api.Client.Serialization;

internal sealed class HybridCacheSerializerTypeRegistry
{
    private readonly ConcurrentDictionary<Type, byte> _types = new();

    internal void AddMethods(IEnumerable<MethodInfo> methods)
    {
        foreach (var method in methods)
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                _ = _types.TryAdd(returnType.GetGenericArguments()[0], 0);
            }
        }
    }

    internal bool Contains(Type type)
    {
        return _types.ContainsKey(type);
    }
}
