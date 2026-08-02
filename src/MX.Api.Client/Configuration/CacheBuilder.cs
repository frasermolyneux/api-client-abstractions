using System.Linq.Expressions;
using MX.Caching.Abstractions;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Configures cache policies for exact API methods.
/// </summary>
public sealed class CacheBuilder
{
    private readonly ApiClientOptionsBase _options;
    private readonly Type? _configuredClientType;

    internal CacheBuilder(ApiClientOptionsBase options, Type? configuredClientType = null)
    {
        _options = options;
        _configuredClientType = configuredClientType;
    }

    /// <summary>
    /// Configures an API method to use the in-process cache.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <param name="ttl">The cache lifetime.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder InMemory<TApi, TResult>(Expression<Func<TApi, TResult>> method, TimeSpan ttl)
    {
        return Add(method, new CachePolicy
        {
            Tier = CacheTier.InProcess,
            Ttl = ttl,
        });
    }

    /// <summary>
    /// Configures an API method to use the distributed cache.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <param name="ttl">The cache lifetime.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder Distributed<TApi, TResult>(Expression<Func<TApi, TResult>> method, TimeSpan ttl)
    {
        return Add(method, new CachePolicy
        {
            Tier = CacheTier.Distributed,
            Ttl = ttl,
        });
    }

    /// <summary>
    /// Configures an API method to use both in-process and distributed caches.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <param name="l1Ttl">The in-process cache lifetime.</param>
    /// <param name="l2Ttl">The distributed cache lifetime.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder Tiered<TApi, TResult>(
        Expression<Func<TApi, TResult>> method,
        TimeSpan l1Ttl,
        TimeSpan l2Ttl)
    {
        return Add(method, new CachePolicy
        {
            Tier = CacheTier.Tiered,
            L1Ttl = l1Ttl,
            L2Ttl = l2Ttl,
        });
    }

    /// <summary>
    /// Configures an API method to bypass caching.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder NotCached<TApi, TResult>(Expression<Func<TApi, TResult>> method)
    {
        return Disable(method);
    }

    /// <summary>
    /// Configures an API method with a policy that replaces any library cache policy.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <param name="policy">The replacement cache policy.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder Override<TApi, TResult>(
        Expression<Func<TApi, TResult>> method,
        CachePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return SetOperation(method, new CachePolicyOperation(CachePolicyOperationKind.Override, policy));
    }

    /// <summary>
    /// Configures an API method with a consumer cache policy when the library permits caching.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <param name="policy">The cache policy to add.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder Add<TApi, TResult>(
        Expression<Func<TApi, TResult>> method,
        CachePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return SetOperation(method, new CachePolicyOperation(CachePolicyOperationKind.Add, policy));
    }

    /// <summary>
    /// Disables caching for an API method.
    /// </summary>
    /// <typeparam name="TApi">The API interface type.</typeparam>
    /// <typeparam name="TResult">The API method return type.</typeparam>
    /// <param name="method">An expression that invokes the API method.</param>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder Disable<TApi, TResult>(Expression<Func<TApi, TResult>> method)
    {
        return SetOperation(
            method,
            new CachePolicyOperation(CachePolicyOperationKind.Disable, policy: null));
    }

    /// <summary>
    /// Enables cache policies registered by the API client library.
    /// </summary>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder UseLibraryDefaults()
    {
        _options.SetUseLibraryCacheDefaults(enabled: true);
        return this;
    }

    /// <summary>
    /// Disables cache policies registered by the API client library.
    /// </summary>
    /// <returns>The current cache builder.</returns>
    public CacheBuilder WithoutLibraryDefaults()
    {
        _options.SetUseLibraryCacheDefaults(enabled: false);
        return this;
    }

    private CacheBuilder SetOperation<TApi, TResult>(
        Expression<Func<TApi, TResult>> expression,
        CachePolicyOperation operation)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.Body is not MethodCallExpression methodCall ||
            methodCall.Method.DeclaringType is null ||
            !methodCall.Method.DeclaringType.IsAssignableFrom(typeof(TApi)))
        {
            throw new ArgumentException(
                $"The expression must directly invoke a method declared by {typeof(TApi).FullName}.",
                nameof(expression));
        }

        if (_configuredClientType is not null &&
            !methodCall.Method.DeclaringType.IsAssignableFrom(_configuredClientType))
        {
            throw new ArgumentException(
                $"The expression must invoke a method declared by {_configuredClientType.FullName} or one of its inherited interfaces.",
                nameof(expression));
        }

        var returnType = methodCall.Method.ReturnType;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            throw new ArgumentException(
                "Only API methods returning Task<T> can be configured for caching.",
                nameof(expression));
        }

        _options.SetCachePolicyOperation(methodCall.Method, operation);
        return this;
    }
}
