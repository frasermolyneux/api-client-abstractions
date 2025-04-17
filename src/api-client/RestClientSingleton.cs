using RestSharp;

namespace MxIO.ApiClient
{
    public class RestClientSingleton : IRestClientSingleton
    {
        private static Dictionary<string, RestClient> instances = new Dictionary<string, RestClient>();
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

        public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
        {
            lock (padlock)
            {
                if (!instances.ContainsKey(baseUrl))
                {
                    instances.Add(baseUrl, CreateRestClient(baseUrl));
                }
            }

            return instances[baseUrl].ExecuteAsync(request, cancellationToken);
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
