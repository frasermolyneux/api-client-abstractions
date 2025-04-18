using RestSharp;

namespace MxIO.ApiClient;

/// <summary>
/// Singleton implementation for managing RestClient instances.
/// Ensures that only one RestClient is created per base URL for optimal resource usage.
/// </summary>
public class RestClientSingleton : IRestClientSingleton
{
    private static readonly Dictionary<string, RestClient> instances = new();
    private static readonly object padlock = new();

    /// <summary>
    /// Creates a new RestClient instance for the given base URL.
    /// This method is virtual to allow overriding in tests.
    /// </summary>
    /// <param name="baseUrl">The base URL for the client</param>
    /// <returns>A new RestClient instance</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl is null or empty.</exception>
    protected virtual RestClient CreateRestClient(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        return new RestClient(baseUrl);
    }

    /// <summary>
    /// Executes the specified REST request asynchronously using a cached RestClient instance.
    /// </summary>
    /// <param name="baseUrl">The base URL for the API endpoint.</param>
    /// <param name="request">The REST request to execute.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the REST response.</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        ArgumentNullException.ThrowIfNull(request);

        RestClient client = GetOrCreateClient(baseUrl);
        return client.ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets an existing client or creates a new one if none exists for the given base URL.
    /// Thread-safe implementation using double-check locking pattern.
    /// </summary>
    /// <param name="baseUrl">The base URL for the client.</param>
    /// <returns>A RestClient instance for the specified base URL.</returns>
    private RestClient GetOrCreateClient(string baseUrl)
    {
        // First check without locking for performance
        if (instances.TryGetValue(baseUrl, out RestClient? client))
        {
            return client;
        }

        // If not found, lock and check again (double-check pattern)
        lock (padlock)
        {
            if (instances.TryGetValue(baseUrl, out client))
            {
                return client;
            }

            // Create new client and add to dictionary
            client = CreateRestClient(baseUrl);
            instances[baseUrl] = client;
            return client;
        }
    }

    /// <summary>
    /// Clears all client instances. Primarily used for testing purposes.
    /// </summary>
    public static void ClearInstances()
    {
        lock (padlock)
        {
            instances.Clear();
        }
    }
}
