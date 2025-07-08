using MX.Api.Client.Configuration;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test API options for unit testing
/// </summary>
public class TestApiOptions : ApiClientOptionsBase
{
    /// <summary>
    /// Test-specific property
    /// </summary>
    public bool EnableTestFeature { get; set; }
}
