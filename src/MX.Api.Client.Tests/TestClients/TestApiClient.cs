using Microsoft.Extensions.Logging;
using MX.Api.Abstractions;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;
using RestSharp;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test model for custom data
/// </summary>
public class CustomData
{
    public bool Custom { get; set; }
}

/// <summary>
/// Test API client interface
/// </summary>
public interface ITestApiClient
{
    /// <summary>
    /// Test method
    /// </summary>
    Task<string> GetDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets custom data
    /// </summary>
    Task<ApiResult<CustomData>> GetCustomDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Test API client implementation
/// </summary>
public class TestApiClient : BaseApi<TestApiOptions>, ITestApiClient
{
    /// <summary>
    /// Initializes a new instance of the TestApiClient
    /// </summary>
    public TestApiClient(
        ILogger<BaseApi<TestApiOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        TestApiOptions options)
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

    /// <summary>
    /// Gets custom data
    /// </summary>
    public async Task<ApiResult<CustomData>> GetCustomDataAsync(CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("custom-endpoint", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<CustomData>();
    }
}
