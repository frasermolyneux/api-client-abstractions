using RestSharp;

namespace MxIO.ApiClient
{
    public class RestClientSingleton : IRestClientSingleton
    {
        private static Dictionary<string, RestClient> instances = new Dictionary<string, RestClient>();
        private static readonly object padlock = new object();

        public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
        {
            lock (padlock)
            {
                if (!instances.ContainsKey(baseUrl))
                {
                    instances.Add(baseUrl, new RestClient(baseUrl));
                }
            }

            return instances[baseUrl].ExecuteAsync(request, cancellationToken);
        }
    }
}
