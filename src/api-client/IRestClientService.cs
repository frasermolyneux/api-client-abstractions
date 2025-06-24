using RestSharp;

namespace MxIO.ApiClient;

/// <summary>
/// Interface for a service that executes RestSharp requests.
/// This abstraction allows for better testability and consistent request handling.
/// </summary>
public interface IRestClientService : IDisposable
{
    /// <summary>
    /// Executes the specified REST request asynchronously.
    /// </summary>
    /// <param name="baseUrl">The base URL for the API endpoint.</param>
    /// <param name="request">The REST request to execute.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the REST response.</returns>
    Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default);
}
