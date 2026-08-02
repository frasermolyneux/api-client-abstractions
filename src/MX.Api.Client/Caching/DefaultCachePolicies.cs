using System.Collections.ObjectModel;
using System.Reflection;

using MX.Caching.Abstractions;

namespace MX.Api.Client.Caching;

/// <summary>
/// Contains the default cache policies supplied by a typed API client library.
/// </summary>
/// <typeparam name="TClient">The typed API client contract.</typeparam>
public sealed class DefaultCachePolicies<TClient>
    where TClient : class
{
    internal DefaultCachePolicies(IReadOnlyDictionary<MethodInfo, CachePolicy> policies)
    {
        Policies = new ReadOnlyDictionary<MethodInfo, CachePolicy>(
            new Dictionary<MethodInfo, CachePolicy>(policies));
    }

    /// <summary>
    /// Gets the default policies keyed by the exact client operation method.
    /// </summary>
    public IReadOnlyDictionary<MethodInfo, CachePolicy> Policies { get; }
}
