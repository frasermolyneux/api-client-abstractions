using System.Collections.ObjectModel;
using System.Reflection;
using MX.Caching.Abstractions;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Base options class for all API clients
/// </summary>
public abstract class ApiClientOptionsBase
{
    private readonly Dictionary<MethodInfo, CachePolicy> _cachePolicies = [];
    private readonly Dictionary<MethodInfo, CachePolicyOperation> _cachePolicyOperations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClientOptionsBase"/> class.
    /// </summary>
    protected ApiClientOptionsBase()
    {
        CachePolicies = new ReadOnlyDictionary<MethodInfo, CachePolicy>(_cachePolicies);
        CachePolicyOperations = new ReadOnlyDictionary<MethodInfo, CachePolicyOperation>(_cachePolicyOperations);
    }

    /// <summary>
    /// Gets or sets the base URL of the API.
    /// </summary>
    /// <remarks>This property is required for the API client to function correctly.</remarks>
    /// <exception cref="ArgumentException">Thrown when this property is not set or is empty when used.</exception>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of authentication options for this API client.
    /// Multiple authentication methods can be applied in the order they are configured.
    /// Can be empty if no authentication is required.
    /// </summary>
    public IList<AuthenticationOptions> AuthenticationOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed API calls.
    /// </summary>
    /// <remarks>
    /// When not set or set to a value less than or equal to 0, a default of 3 retry attempts will be used.
    /// The retry mechanism uses exponential backoff with a base of 2 seconds.
    /// </remarks>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the stable, non-secret partition used to isolate cached responses.
    /// </summary>
    /// <remarks>
    /// Set this to an opaque caller, tenant, or authentication identity. The value is hashed before it is used in a cache key.
    /// </remarks>
    public string CachePartition { get; set; } = string.Empty;

    /// <summary>
    /// Gets the cache policies configured for exact API methods.
    /// </summary>
    public IReadOnlyDictionary<MethodInfo, CachePolicy> CachePolicies { get; }

    /// <summary>
    /// Gets the consumer cache policy operations configured for exact API methods.
    /// </summary>
    public IReadOnlyDictionary<MethodInfo, CachePolicyOperation> CachePolicyOperations { get; }

    /// <summary>
    /// Gets a value indicating whether registered library cache defaults should be used.
    /// </summary>
    public bool UseLibraryCacheDefaults { get; private set; }

    internal void SetCachePolicyOperation(MethodInfo method, CachePolicyOperation operation)
    {
        _cachePolicyOperations[method] = operation;
        _cachePolicies[method] = operation.Policy ?? CachePolicy.NotCached;
    }

    internal void SetUseLibraryCacheDefaults(bool enabled)
    {
        UseLibraryCacheDefaults = enabled;
    }

    /// <summary>
    /// Validates the options configuration
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new ArgumentException("BaseUrl must be provided", nameof(BaseUrl));
        }

        if ((CachePolicyOperations.Count > 0 || UseLibraryCacheDefaults)
            && string.IsNullOrWhiteSpace(CachePartition))
        {
            throw new ArgumentException(
                "CachePartition must be provided when caching is enabled",
                nameof(CachePartition));
        }
    }
}
