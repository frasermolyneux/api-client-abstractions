using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;
using MX.Api.Web.Extensions;

namespace ExampleUsage;

/// <summary>
/// Example controller demonstrating the use of ApiResponse extension methods.
/// This shows how the extension methods can clean up controller code by providing
/// simple conversion from ApiResponse to ApiResult.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IExampleApiClient _apiClient;

    public ExampleController(IExampleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Example of using ToApiResultWithErrorHandling for smart status code handling.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var response = await _apiClient.GetItemAsync(id);

        // Before: Manual status code handling
        // if (response.Errors?.Length > 0)
        //     return BadRequest(response);
        // if (response.Data == null)
        //     return NotFound(response);
        // return Ok(response);

        // After: Clean one-liner with smart error handling
        return response.ToApiResultWithErrorHandling().ToHttpResult();
    }

    /// <summary>
    /// Example of using ToCreatedResult for creation operations.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
    {
        var response = await _apiClient.CreateItemAsync(request);

        // Before: Manual status handling
        // if (response.Errors?.Length > 0)
        //     return BadRequest(response);
        // return Created($"api/example/{response.Data?.Id}", response);

        // After: Clean and consistent
        return response.ToCreatedResult().ToHttpResult();
    }

    /// <summary>
    /// Example of using ToAcceptedResult for update operations.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemRequest request)
    {
        var response = await _apiClient.UpdateItemAsync(id, request);

        return response.ToAcceptedResult().ToHttpResult();
    }

    /// <summary>
    /// Example of using specific status code methods for different scenarios.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var response = await _apiClient.DeleteItemAsync(id);

        // You can still use specific status codes when needed
        if (response.Errors?.Any(e => e.Code == "ITEM_NOT_FOUND") == true)
            return response.ToNotFoundResult().ToHttpResult();

        if (response.Errors?.Any(e => e.Code == "ITEM_IN_USE") == true)
            return response.ToConflictResult().ToHttpResult();

        return response.ToApiResultWithErrorHandling().ToHttpResult();
    }

    /// <summary>
    /// Example of using custom status codes when the smart handling isn't sufficient.
    /// </summary>
    [HttpPost("{id}/validate")]
    public async Task<IActionResult> ValidateItem(int id)
    {
        var response = await _apiClient.ValidateItemAsync(id);

        // Use custom status code for specific business logic
        return response.ToApiResult(System.Net.HttpStatusCode.Accepted).ToHttpResult();
    }
}

/// <summary>
/// Mock interface for demonstration purposes.
/// </summary>
public interface IExampleApiClient
{
    Task<ApiResponse<Item>> GetItemAsync(int id);
    Task<ApiResponse<Item>> CreateItemAsync(CreateItemRequest request);
    Task<ApiResponse<Item>> UpdateItemAsync(int id, UpdateItemRequest request);
    Task<ApiResponse> DeleteItemAsync(int id);
    Task<ApiResponse<ValidationResult>> ValidateItemAsync(int id);
}

/// <summary>
/// Mock classes for demonstration purposes.
/// </summary>
public class Item
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class CreateItemRequest
{
    public string? Name { get; set; }
}

public class UpdateItemRequest
{
    public string? Name { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string[]? Issues { get; set; }
}
