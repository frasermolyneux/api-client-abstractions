namespace MxIO.ApiClient
{
    public interface IApiClientOptions
    {
        public string BaseUrl { get; }
        public string ApiKey { get; }
        public string ApiAudience { get; }
        public string? ApiPathPrefix { get; }
    }
}
