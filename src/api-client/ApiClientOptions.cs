namespace MxIO.ApiClient
{
    public class ApiClientOptions
    {
        public string BaseUrl { get; }
        public string ApiKey { get; }
        public string ApiAudience { get; }
        public string? ApiPathPrefix { get; } = null;

        public ApiClientOptions(string baseUrl, string apiKey, string apiAudience)
        {
            BaseUrl = baseUrl;
            ApiKey = apiKey;
            ApiAudience = apiAudience;
        }

        public ApiClientOptions(string baseUrl, string apiKey, string apiAudience, string apiPathPrefix) : this(baseUrl, apiKey, apiAudience)
        {
            ApiPathPrefix = apiPathPrefix;
        }
    }
}
