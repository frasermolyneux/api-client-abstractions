using System.Reflection;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Captures a single cache configuration delegate that spans multiple typed API clients so it can be
/// applied per client without throwing when an expression targets a sibling sub-API interface.
/// </summary>
/// <remarks>
/// <para>
/// Designed for "unified" client scenarios where one consumer delegate registers cache policies against
/// several typed sub-API interfaces (for example, a repository client that composes ~30 sub-APIs sharing
/// one options / builder type). Use <see cref="ApiClientOptionsBuilder{TOptions, TBuilder}.WithSharedCaching(SharedCacheConfiguration)"/>
/// from every per-client registration; each call applies only the operations whose declaring interface is
/// assignable from the current typed client and records that they matched.
/// </para>
/// <para>
/// After all typed clients are registered, call <see cref="ValidateAllOperationsMatched"/> to surface real
/// typos: any operation whose declaring interface is not assignable from any registered client's typed
/// interface will throw a descriptive <see cref="InvalidOperationException"/>. Single-client scope safety
/// in the existing <see cref="ApiClientOptionsBuilder{TOptions, TBuilder}.WithCaching(Action{CacheBuilder})"/>
/// is unchanged.
/// </para>
/// <para>
/// Instances are stateful: <see cref="ApplyTo"/> records which captured operations have matched a typed
/// client, so a single instance is intended for one registration pass against one
/// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>. Do not share an instance across
/// independent DI compositions — reusing an already-applied configuration in a second composition would leave
/// the earlier match records in place and could mask real typos. Instances are not thread-safe; the expected
/// usage is synchronous DI registration on a single thread.
/// </para>
/// </remarks>
public sealed class SharedCacheConfiguration
{
    private readonly List<CapturedOperation> _operations = [];
    private readonly bool _useLibraryDefaults;
    private readonly HashSet<MethodInfo> _matchedOperations = [];
    private bool _appliedToAnyClient;

    /// <summary>
    /// Captures the cache configuration expressed by <paramref name="configure"/> without binding it to a
    /// specific typed API client.
    /// </summary>
    /// <param name="configure">A cache configuration callback whose expressions may target multiple typed API interfaces.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    public SharedCacheConfiguration(Action<CacheBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var sink = new RecordingApiClientOptions();
        configure(new CacheBuilder(sink, configuredClientType: null));

        _useLibraryDefaults = sink.UseLibraryCacheDefaults;
        foreach (var entry in sink.CachePolicyOperations)
        {
            _operations.Add(new CapturedOperation(entry.Key, entry.Value));
        }
    }

    internal void ApplyTo(ApiClientOptionsBase options, Type? configuredClientType)
    {
        ArgumentNullException.ThrowIfNull(options);

        _appliedToAnyClient = true;

        if (_useLibraryDefaults)
        {
            options.SetUseLibraryCacheDefaults(enabled: true);
        }

        foreach (var captured in _operations)
        {
            var declaringType = captured.Method.DeclaringType;
            if (declaringType is null)
            {
                continue;
            }

            if (configuredClientType is null || declaringType.IsAssignableFrom(configuredClientType))
            {
                options.SetCachePolicyOperation(captured.Method, captured.Operation);
                _ = _matchedOperations.Add(captured.Method);
            }
        }
    }

    /// <summary>
    /// Verifies that every captured cache operation matched at least one typed API client registered via
    /// <see cref="ApiClientOptionsBuilder{TOptions, TBuilder}.WithSharedCaching(SharedCacheConfiguration)"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the shared configuration was never applied to a typed client, or when one or more operations target
    /// a declaring interface that is not assignable from any registered typed client's interface (for example, no
    /// registered sub-API implements or inherits the operation's declaring interface).
    /// </exception>
    public void ValidateAllOperationsMatched()
    {
        if (!_appliedToAnyClient)
        {
            if (_operations.Count == 0 && !_useLibraryDefaults)
            {
                return;
            }

            throw new InvalidOperationException(
                "SharedCacheConfiguration was created but never applied via WithSharedCaching(...). " +
                "Ensure every typed API client that should participate in shared caching calls WithSharedCaching(sharedCacheConfiguration).");
        }

        var unmatched = _operations
            .Where(op => !_matchedOperations.Contains(op.Method))
            .ToList();

        if (unmatched.Count == 0)
        {
            return;
        }

        var summary = string.Join(
            Environment.NewLine,
            unmatched.Select(op => "  - " + FormatMethodSignature(op.Method)));

        throw new InvalidOperationException(
            "The following shared cache operations did not match any typed API client registered via WithSharedCaching. " +
            "Verify that each declaring interface is assignable from a registered typed API client's interface " +
            "(either the same interface or one it inherits) and that the shared configuration is passed to every registration:"
            + Environment.NewLine + summary);
    }

    private static string FormatMethodSignature(MethodInfo method)
    {
        var declaringName = method.DeclaringType?.FullName ?? "<unknown>";
        var parameters = string.Join(
            ", ",
            method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
        var returnName = method.ReturnType.FullName ?? method.ReturnType.Name;
        return $"{returnName} {declaringName}.{method.Name}({parameters})";
    }

    private readonly record struct CapturedOperation(MethodInfo Method, CachePolicyOperation Operation);

    private sealed class RecordingApiClientOptions : ApiClientOptionsBase
    {
    }
}
