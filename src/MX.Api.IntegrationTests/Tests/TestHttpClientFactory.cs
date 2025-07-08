using System.Net.Http;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// Test implementation of IHttpClientFactory that returns the test server's HttpClient
/// </summary>
public class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public TestHttpClientFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}
