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
public class StandardTestApiClient : BaseApi<ApiClientOptions>, IStandardTestApiClient
{
    /// <summary>
    /// Initializes a new instance of the StandardTestApiClient
    /// </summary>
    public StandardTestApiClient(
        ILogger<BaseApi<ApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        ApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

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
