using MX.Api.Client.Configuration;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// Test API options builder for unit testing
/// </summary>
public class TestApiOptionsBuilder : ApiClientOptionsBuilder<TestApiOptions, TestApiOptionsBuilder>
{
    /// <summary>
    /// Configures the test feature
    /// </summary>
    /// <param name="enableTestFeature">Whether to enable the test feature</param>
    /// <returns>The builder instance for method chaining</returns>
    public TestApiOptionsBuilder WithTestFeature(bool enableTestFeature = true)
    {
        Options.EnableTestFeature = enableTestFeature;
        return this;
    }
}
