using MX.Caching.Abstractions;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Identifies how a consumer cache policy participates in policy resolution.
/// </summary>
public enum CachePolicyOperationKind
{
    /// <summary>
    /// Replaces any library cache policy, including a not-cached guard.
    /// </summary>
    Override,

    /// <summary>
    /// Adds a consumer cache policy when the library permits caching.
    /// </summary>
    Add,

    /// <summary>
    /// Disables caching for the API method.
    /// </summary>
    Disable,
}

/// <summary>
/// Describes a consumer cache policy operation for an exact API method.
/// </summary>
public sealed record CachePolicyOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CachePolicyOperation"/> class.
    /// </summary>
    /// <param name="kind">The operation applied during policy resolution.</param>
    /// <param name="policy">The policy associated with an add or override operation.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when a disable operation has a policy or an add or override operation has no policy.
    /// </exception>
    public CachePolicyOperation(CachePolicyOperationKind kind, CachePolicy? policy)
    {
        var isDisable = kind == CachePolicyOperationKind.Disable;
        var hasPolicy = policy is not null;

        if (isDisable == hasPolicy)
        {
            throw new ArgumentException(
                "Disable operations cannot have a policy; add and override operations require one.",
                nameof(policy));
        }

        Kind = kind;
        Policy = policy;
    }

    /// <summary>
    /// Gets the operation applied during policy resolution.
    /// </summary>
    public CachePolicyOperationKind Kind { get; }

    /// <summary>
    /// Gets the policy associated with an add or override operation.
    /// </summary>
    public CachePolicy? Policy { get; }
}
