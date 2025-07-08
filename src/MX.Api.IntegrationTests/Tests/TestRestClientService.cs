using System.Net.Http;
using MX.Api.Client;
using RestSharp;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// Test implementation of IRestClientService that uses a specific HttpClient
/// </summary>
public class TestRestClientService : IRestClientService
{
    private readonly HttpClient _httpClient;
    private readonly RestClient _restClient;

    public TestRestClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Create RestClient with the test HttpClient - use the simpler constructor
        _restClient = new RestClient(_httpClient, disposeHttpClient: false);
    }

    public Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        // Ignore the baseUrl parameter since we're using the test client with a fixed base URL
        return _restClient.ExecuteAsync(request, cancellationToken);
    }

    public Task<RestResponse> ExecuteWithNamedOptionsAsync(string optionsName, RestRequest request, CancellationToken cancellationToken = default)
    {
        // For testing, treat this the same as ExecuteAsync
        return _restClient.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        _restClient?.Dispose();
    }
}
