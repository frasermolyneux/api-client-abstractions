using RestSharp;

namespace MxIO.ApiClient
{
    public class RestClientSingleton : IRestClientSingleton
    {
        private static string? BaseUrl { get; set; }

        private static RestClient? instance = null;
        private static readonly object padlock = new object();

        public void ConfigureBaseUrl(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
        {
            return Instance.ExecuteAsync(request, cancellationToken);
        }

        private static RestClient Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RestClient(BaseUrl ?? "");
                    }
                    return instance;
                }
            }
        }
    }
}
