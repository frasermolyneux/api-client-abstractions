using RestSharp;

namespace MxIO.ApiClient
{
    /// <summary>
    /// Singleton implementation for managing RestClient instances.
    /// Ensures that only one RestClient is created per base URL for optimal resource usage.
    /// </summary>
    public class RestClientSingleton : IRestClientSingleton
    {
        private static readonly Dictionary<string, RestClient> instances = new Dictionary<string, RestClient>();
        private static readonly object padlock = new object();

        /// <summary>
        /// Creates a new RestClient instance for the given base URL.
        /// This method is virtual to allow overriding in tests.
        /// </summary>
        /// <param name="baseUrl">The base URL for the client</param>
        /// <returns>A new RestClient instance</returns>
        protected virtual RestClient CreateRestClient(string baseUrl)
        {
            return new RestClient(baseUrl);
        }

        /// <summary>
        /// Executes the specified REST request asynchronously using a cached RestClient instance.
        /// </summary>
        /// <param name="baseUrl">The base URL for the API endpoint.</param>
        /// <param name="request">The REST request to execute.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the REST response.</returns>
        public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
        {
            RestClient client;

            lock (padlock)
            {
                if (!instances.TryGetValue(baseUrl, out client!))
                {
                    client = CreateRestClient(baseUrl);
                    instances.Add(baseUrl, client);
                }
            }

            return client.ExecuteAsync(request, cancellationToken);
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
}
