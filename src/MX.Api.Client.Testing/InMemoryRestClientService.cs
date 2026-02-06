using System.Net;
using RestSharp;

namespace MX.Api.Client.Testing;

/// <summary>
/// In-memory implementation of IRestClientService for testing purposes.
/// Allows configuring expected responses without making actual HTTP calls.
/// </summary>
/// <example>
/// <code>
/// // Setup test responses
/// var testService = new InMemoryRestClientService();
/// testService.AddResponse("/api/users/123", new RestResponse
/// {
///     StatusCode = HttpStatusCode.OK,
///     Content = "{\"id\": \"123\", \"name\": \"John Doe\"}"
/// });
/// 
/// // Use in your tests
/// services.AddSingleton&lt;IRestClientService&gt;(testService);
/// </code>
/// </example>
public class InMemoryRestClientService : IRestClientService
{
    private readonly Dictionary<string, RestResponse> _responses = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<RestRequest, RestResponse>> _responseFunctions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Resource, RestRequest Request)> _executedRequests = new();
    private RestResponse? _defaultResponse;
    private bool _disposed;

    /// <summary>
    /// Gets all requests that have been executed.
    /// Useful for verifying that expected API calls were made during tests.
    /// </summary>
    public IReadOnlyList<(string Resource, RestRequest Request)> ExecutedRequests => _executedRequests.AsReadOnly();

    /// <summary>
    /// Adds a predefined response for a specific resource path.
    /// </summary>
    /// <param name="resource">The API resource path (e.g., "/api/users/123")</param>
    /// <param name="response">The response to return when this resource is requested</param>
    /// <example>
    /// <code>
    /// testService.AddResponse("/api/users/123", new RestResponse
    /// {
    ///     StatusCode = HttpStatusCode.OK,
    ///     Content = "{\"id\": \"123\", \"name\": \"John Doe\"}"
    /// });
    /// </code>
    /// </example>
    public void AddResponse(string resource, RestResponse response)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);
        ArgumentNullException.ThrowIfNull(response);

        _responses[resource] = response;
    }

    /// <summary>
    /// Adds a function that generates responses dynamically based on the request.
    /// Useful for scenarios where you need to inspect the request or return different responses based on query parameters.
    /// </summary>
    /// <param name="resource">The API resource path</param>
    /// <param name="responseFunction">A function that takes a RestRequest and returns a RestResponse</param>
    /// <example>
    /// <code>
    /// testService.AddResponseFunction("/api/users", request =>
    /// {
    ///     var pageSize = request.Parameters.FirstOrDefault(p => p.Name == "pageSize")?.Value;
    ///     return new RestResponse { StatusCode = HttpStatusCode.OK, Content = $"{{\"pageSize\": {pageSize}}}" };
    /// });
    /// </code>
    /// </example>
    public void AddResponseFunction(string resource, Func<RestRequest, RestResponse> responseFunction)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);
        ArgumentNullException.ThrowIfNull(responseFunction);

        _responseFunctions[resource] = responseFunction;
    }

    /// <summary>
    /// Sets a default response to return when no specific response is configured for a resource.
    /// If not set, a 404 NotFound response is returned for unconfigured resources.
    /// </summary>
    /// <param name="response">The default response</param>
    public void SetDefaultResponse(RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        _defaultResponse = response;
    }

    /// <summary>
    /// Clears all configured responses and executed request history.
    /// </summary>
    public void Clear()
    {
        _responses.Clear();
        _responseFunctions.Clear();
        _executedRequests.Clear();
        _defaultResponse = null;
    }

    /// <summary>
    /// Verifies that a specific resource was called at least once.
    /// </summary>
    /// <param name="resource">The resource path to verify</param>
    /// <returns>True if the resource was called, false otherwise</returns>
    public bool WasCalled(string resource)
    {
        return _executedRequests.Any(r => r.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that a specific resource was called exactly the specified number of times.
    /// </summary>
    /// <param name="resource">The resource path to verify</param>
    /// <param name="times">The expected number of calls</param>
    /// <returns>True if called exactly the specified number of times, false otherwise</returns>
    public bool WasCalledTimes(string resource, int times)
    {
        var count = _executedRequests.Count(r => r.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));
        return count == times;
    }

    /// <inheritdoc/>
    public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(InMemoryRestClientService));
        }

        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        ArgumentNullException.ThrowIfNull(request);

        // Record the request
        _executedRequests.Add((request.Resource, request));

        // Check for a response function first (more specific)
        if (_responseFunctions.TryGetValue(request.Resource, out var responseFunction))
        {
            return Task.FromResult(responseFunction(request));
        }

        // Check for a predefined response
        if (_responses.TryGetValue(request.Resource, out var response))
        {
            return Task.FromResult(response);
        }

        // Return default response if configured
        if (_defaultResponse != null)
        {
            return Task.FromResult(_defaultResponse);
        }

        // Return a 404 response if no match found
        return Task.FromResult(new RestResponse
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = $"No response configured for resource: {request.Resource}",
            ResponseStatus = ResponseStatus.Completed
        });
    }

    /// <inheritdoc/>
    public Task<RestResponse> ExecuteWithNamedOptionsAsync(string optionsName, RestRequest request, CancellationToken cancellationToken = default)
    {
        // For the in-memory implementation, treat this the same as ExecuteAsync
        // The base URL is irrelevant for testing, so we use a placeholder
        return ExecuteAsync("https://test.local", request, cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Clear();
            _disposed = true;
        }
    }
}
