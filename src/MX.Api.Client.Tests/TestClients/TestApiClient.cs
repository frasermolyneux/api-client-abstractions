using Microsoft.Extensions.Logging;
using MX.Api.Client.Auth;
using RestSharp;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test API client interface
/// </summary>
public interface ITestApiClient
{
    /// <summary>
    /// Test method
    /// </summary>
    Task<string> GetDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Test API client implementation
/// </summary>
/// <remarks>
/// Initializes a new instance of the TestApiClient
/// </remarks>
public class TestApiClient(
    ILogger<BaseApi<TestApiOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    TestApiOptions options) : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), ITestApiClient
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
