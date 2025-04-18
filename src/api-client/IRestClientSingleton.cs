using RestSharp;

namespace MxIO.ApiClient;

/// <summary>
/// Interface for a singleton service that manages RestClient instances.
/// This abstraction allows for better testability and ensures proper client reuse across the application.
/// </summary>
public interface IRestClientSingleton
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
