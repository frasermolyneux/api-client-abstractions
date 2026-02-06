using Microsoft.Extensions.Logging;
using MX.Api.Abstractions;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using RestSharp;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test model for test client responses
/// </summary>
public class TestModel
{
    public string? Id { get; set; }
    public string? Value { get; set; }
}

/// <summary>
/// Test API client interface for standard options
/// </summary>
public interface IStandardTestApiClient
{
    /// <summary>
    /// Test method
    /// </summary>
    Task<string> GetDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a test resource by ID
    /// </summary>
    Task<ApiResult<TestModel>> GetTestResourceAsync(string id, CancellationToken cancellationToken = default);
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

    /// <summary>
    /// Gets a test resource by ID
    /// </summary>
    public async Task<ApiResult<TestModel>> GetTestResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("test-resource", Method.Get, cancellationToken);
        request.AddQueryParameter("id", id);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<TestModel>();
    }
}
