using Microsoft.AspNetCore.Mvc.Testing;
using MX.Api.IntegrationTests.TestApp;

namespace MX.Api.IntegrationTests.Tests;

/// <summary>
/// Custom WebApplicationFactory for testing
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                _ = webBuilder.UseStartup<TestStartup>();
                _ = webBuilder.UseEnvironment("Testing");
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.ConfigureServices(services =>
        {
            // Additional test-specific service configuration can go here
        });
    }
}
