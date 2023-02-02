using RestSharp;

namespace MxIO.ApiClient
{
    public interface IRestClientSingleton
    {
        void ConfigureBaseUrl(string baseUrl);
        Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);
    }
}
