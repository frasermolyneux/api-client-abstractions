using RestSharp;

namespace MX.Api.Client;

/// <summary>
/// Service for executing RestSharp requests.
/// Provides simplified resource management compared to the singleton approach.
/// </summary>
public class RestClientService : IRestClientService, IDisposable
{
    private readonly TimeSpan defaultTimeout = TimeSpan.FromMinutes(5);
    private readonly Dictionary<string, RestClient> clientCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object lockObject = new();
    private bool disposed;

    /// <summary>
    /// Executes the specified REST request asynchronously.
    /// Creates and caches RestClient instances for each base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL for the API endpoint.</param>
    /// <param name="request">The REST request to execute.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the REST response.</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the class has been disposed.</exception>
    public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(RestClientService));
        }

        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        ArgumentNullException.ThrowIfNull(request);

        // Get or create client for this base URL
        var client = GetOrCreateClient(baseUrl);

        // Execute the request
        return client.ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets an existing client or creates a new one for the given base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>A RestClient for the specified base URL.</returns>
    private RestClient GetOrCreateClient(string baseUrl)
    {
        // Thread-safe check and create if needed
        lock (lockObject)
        {
            if (!clientCache.TryGetValue(baseUrl, out var client))
            {
                // Create new client with default settings
                var options = new RestClientOptions(baseUrl)
                {
                    ThrowOnDeserializationError = true,
                    Timeout = defaultTimeout
                };

                client = new RestClient(options);
                clientCache[baseUrl] = client;
            }

            return client;
        }
    }

    /// <summary>
    /// Disposes all REST client instances managed by this service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    /// <param name="disposing">True to dispose managed resources, otherwise false.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            // Dispose all cached clients
            lock (lockObject)
            {
                foreach (var client in clientCache.Values)
                {
                    client.Dispose();
                }

                clientCache.Clear();
            }

            disposed = true;
        }
    }
}
