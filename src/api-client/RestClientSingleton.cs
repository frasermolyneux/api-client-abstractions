using RestSharp;
using System.Collections.Concurrent;

namespace MxIO.ApiClient;

/// <summary>
/// Singleton implementation for managing RestClient instances.
/// Ensures that only one RestClient is created per base URL for optimal resource usage.
/// </summary>
public class RestClientSingleton : IRestClientSingleton, IDisposable
{
    private static readonly ConcurrentDictionary<string, RestClient> instances = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, DateTime> lastAccessTimes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object padlock = new();
    private readonly TimeSpan defaultTimeout = TimeSpan.FromMinutes(5);
    private readonly TimeSpan clientExpirationTime = TimeSpan.FromHours(1);
    private bool disposed;

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

        var options = new RestClientOptions(baseUrl)
        {
            ThrowOnDeserializationError = true,
            Timeout = defaultTimeout
        };

        return new RestClient(options);
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
    /// <exception cref="ObjectDisposedException">Thrown when the class has been disposed.</exception>
    public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(RestClientSingleton));
        }

        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        ArgumentNullException.ThrowIfNull(request);

        RestClient client = GetOrCreateClient(baseUrl);

        // Update last access time for this client
        lastAccessTimes[baseUrl] = DateTime.UtcNow;

        // Cleanup expired clients occasionally (1% chance per request)
        if (Random.Shared.Next(1, 100) == 1)
        {
            CleanupExpiredClients();
        }

        return client.ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets an existing client or creates a new one if none exists for the given base URL.
    /// Thread-safe implementation using ConcurrentDictionary.
    /// </summary>
    /// <param name="baseUrl">The base URL for the client.</param>
    /// <returns>A RestClient instance for the specified base URL.</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl is null or empty.</exception>
    private RestClient GetOrCreateClient(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        // Using GetOrAdd from ConcurrentDictionary for atomic get-or-create operation
        var client = instances.GetOrAdd(baseUrl, key =>
        {
            var newClient = CreateRestClient(key);
            lastAccessTimes[key] = DateTime.UtcNow;
            return newClient;
        });

        return client;
    }

    /// <summary>
    /// Removes and disposes clients that haven't been used for the expiration time.
    /// </summary>
    private void CleanupExpiredClients()
    {
        var now = DateTime.UtcNow;
        var expiredUrls = lastAccessTimes
            .Where(kvp => (now - kvp.Value) > clientExpirationTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var url in expiredUrls)
        {
            if (instances.TryRemove(url, out var client))
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception)
                {
                    // Suppress exceptions during cleanup
                }

                lastAccessTimes.TryRemove(url, out _);
            }
        }
    }

    /// <summary>
    /// Clears all client instances. Primarily used for testing purposes.
    /// </summary>
    public static void ClearInstances()
    {
        lock (padlock)
        {
            // Dispose clients before clearing
            foreach (var client in instances.Values)
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception)
                {
                    // Suppress exceptions during cleanup
                    // We don't want disposal failures to prevent cleanup
                }
            }

            instances.Clear();
            lastAccessTimes.Clear();
        }
    }

    /// <summary>
    /// Disposes all REST client instances managed by this singleton.
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
            ClearInstances();
            disposed = true;
        }
    }
}
