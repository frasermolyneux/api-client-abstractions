using MX.Api.Abstractions;

namespace MX.Api.IntegrationTests.TestApp;

/// <summary>
/// Test web application startup class
/// </summary>
public class TestStartup
{
    /// <summary>
    /// Configures services
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddLogging(builder => builder.AddConsole());

        // Add controllers from all dummy API assemblies
        services.AddMvc()
            .AddApplicationPart(typeof(DummyApis.WeatherApi.Controllers.WeatherController).Assembly)
            .AddApplicationPart(typeof(DummyApis.UserApi.Controllers.UsersController).Assembly)
            .AddApplicationPart(typeof(DummyApis.ProductApi.Controllers.ProductsController).Assembly);
    }

    /// <summary>
    /// Configures the application
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
