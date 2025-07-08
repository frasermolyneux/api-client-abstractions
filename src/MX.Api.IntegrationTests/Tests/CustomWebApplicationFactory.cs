using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                webBuilder.UseStartup<TestStartup>();
                webBuilder.UseEnvironment("Testing");
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Additional test-specific service configuration can go here
        });
    }
}
