using RestSharp;

namespace MxIO.ApiClient
{
    public interface IRestClientSingleton
    {
        Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default);
    }
}
