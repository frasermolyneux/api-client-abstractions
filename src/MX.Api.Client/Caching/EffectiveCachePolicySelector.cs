using System.Reflection;

using MX.Api.Client.Configuration;
using MX.Caching.Abstractions;

namespace MX.Api.Client.Caching;

internal static class EffectiveCachePolicySelector
{
    internal static EffectiveCachePolicySelection Resolve(
        MethodInfo method,
        ApiClientOptionsBase options,
        CachePolicy? libraryDefault)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(options);

        var selectedDefault = options.UseLibraryCacheDefaults && libraryDefault is not null
            ? libraryDefault
            : CachePolicy.NotCached;

        return !options.CachePolicyOperations.TryGetValue(method, out var operation)
            ? new EffectiveCachePolicySelection(selectedDefault, ConsumerOverride: null)
            : operation.Kind switch
            {
                CachePolicyOperationKind.Disable => new EffectiveCachePolicySelection(
                    CachePolicy.NotCached,
                    ConsumerOverride: null),
                CachePolicyOperationKind.Override => new EffectiveCachePolicySelection(
                    selectedDefault,
                    operation.Policy),
                CachePolicyOperationKind.Add when selectedDefault == CachePolicy.NotCached &&
                    options.UseLibraryCacheDefaults &&
                    libraryDefault == CachePolicy.NotCached => new EffectiveCachePolicySelection(
                        CachePolicy.NotCached,
                        ConsumerOverride: null),
                CachePolicyOperationKind.Add => new EffectiveCachePolicySelection(
                    operation.Policy!,
                    ConsumerOverride: null),
                _ => throw new InvalidOperationException($"Unsupported cache policy operation kind '{operation.Kind}'."),
            };
    }
}

internal sealed record EffectiveCachePolicySelection(
    CachePolicy LibraryDefault,
    CachePolicy? ConsumerOverride);
