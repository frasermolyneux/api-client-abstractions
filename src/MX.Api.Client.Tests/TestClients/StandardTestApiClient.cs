using Microsoft.Extensions.Logging;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using RestSharp;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test API client interface for standard options
/// </summary>
public interface IStandardTestApiClient
{
    /// <summary>
    /// Test method
    /// </summary>
    Task<string> GetDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Test API client implementation that uses standard ApiClientOptions
/// </summary>
/// <remarks>
/// Initializes a new instance of the StandardTestApiClient
/// </remarks>
public class StandardTestApiClient(
    ILogger<BaseApi<ApiClientOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    ApiClientOptions options) : BaseApi<ApiClientOptions>(logger, apiTokenProvider, restClientService, options), IStandardTestApiClient
{

    /// <summary>
    /// Test method implementation
    /// </summary>
    public async Task<string> GetDataAsync(CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("test", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.Content ?? string.Empty;
    }
}
