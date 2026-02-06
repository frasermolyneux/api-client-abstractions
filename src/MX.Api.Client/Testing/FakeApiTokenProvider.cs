using MX.Api.Client.Auth;

namespace MX.Api.Client.Testing;

/// <summary>
/// Fake implementation of IApiTokenProvider for testing purposes.
/// Returns predefined tokens without making actual authentication calls.
/// </summary>
/// <example>
/// <code>
/// var fakeTokenProvider = new FakeApiTokenProvider();
/// fakeTokenProvider.SetToken("api://my-api", "fake-test-token-123");
/// 
/// // Use in your tests
/// services.AddSingleton&lt;IApiTokenProvider&gt;(fakeTokenProvider);
/// </code>
/// </example>
public class FakeApiTokenProvider : IApiTokenProvider
{
    private readonly Dictionary<string, string> _tokens = new(StringComparer.OrdinalIgnoreCase);
    private string? _defaultToken;
    private readonly List<(string Audience, DateTime RequestedAt)> _tokenRequests = new();

    /// <summary>
    /// Gets all token requests that have been made.
    /// Useful for verifying authentication behavior in tests.
    /// </summary>
    public IReadOnlyList<(string Audience, DateTime RequestedAt)> TokenRequests => _tokenRequests.AsReadOnly();

    /// <summary>
    /// Sets a token to return for a specific audience.
    /// </summary>
    /// <param name="audience">The API audience (e.g., "api://my-api")</param>
    /// <param name="token">The token to return</param>
    /// <example>
    /// <code>
    /// fakeTokenProvider.SetToken("api://users-api", "user-api-token-123");
    /// fakeTokenProvider.SetToken("api://orders-api", "order-api-token-456");
    /// </code>
    /// </example>
    public void SetToken(string audience, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(audience);
        ArgumentException.ThrowIfNullOrEmpty(token);

        _tokens[audience] = token;
    }

    /// <summary>
    /// Sets a default token to return when no specific token is configured for an audience.
    /// If not set and no audience-specific token is configured, returns "fake-test-token".
    /// </summary>
    /// <param name="token">The default token</param>
    public void SetDefaultToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        _defaultToken = token;
    }

    /// <summary>
    /// Clears all configured tokens and request history.
    /// </summary>
    public void Clear()
    {
        _tokens.Clear();
        _tokenRequests.Clear();
        _defaultToken = null;
    }

    /// <summary>
    /// Verifies that a token was requested for a specific audience.
    /// </summary>
    /// <param name="audience">The audience to verify</param>
    /// <returns>True if a token was requested for this audience, false otherwise</returns>
    public bool WasTokenRequested(string audience)
    {
        return _tokenRequests.Any(r => r.Audience.Equals(audience, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that a token was requested for a specific audience exactly the specified number of times.
    /// </summary>
    /// <param name="audience">The audience to verify</param>
    /// <param name="times">The expected number of requests</param>
    /// <returns>True if requested exactly the specified number of times, false otherwise</returns>
    public bool WasTokenRequestedTimes(string audience, int times)
    {
        var count = _tokenRequests.Count(r => r.Audience.Equals(audience, StringComparison.OrdinalIgnoreCase));
        return count == times;
    }

    /// <inheritdoc/>
    public Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(audience);

        // Record the request
        _tokenRequests.Add((audience, DateTime.UtcNow));

        // Check for audience-specific token
        if (_tokens.TryGetValue(audience, out var token))
        {
            return Task.FromResult(token);
        }

        // Return default token if set
        if (_defaultToken != null)
        {
            return Task.FromResult(_defaultToken);
        }

        // Return a generic fake token
        return Task.FromResult("fake-test-token");
    }
}
