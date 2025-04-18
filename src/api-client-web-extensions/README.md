# MxIO.ApiClient.WebExtensions

Extensions for converting API response objects to HTTP results in ASP.NET Core applications. This package simplifies the process of transforming API client responses into appropriate HTTP responses.

## Features

- Extension methods to convert ApiResponseDto objects to IActionResult
- Automatic HTTP status code mapping based on API response status
- Simplifies API controller implementations

## Installation

```
dotnet add package MxIO.ApiClient.WebExtensions
```

## Basic Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly IMyApiClient _apiClient;

    public MyController(IMyApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Get data from your API client
        var response = await _apiClient.GetDataAsync();
        
        // Convert the API response to an appropriate HTTP response
        return response.ToHttpResult();
    }
}
```

## License

GPL-3.0-only