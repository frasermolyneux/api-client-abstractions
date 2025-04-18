using System.Net;

using Microsoft.AspNetCore.Mvc;

using MxIO.ApiClient.Abstractions;

namespace MxIO.ApiClient.WebExtensions;

/// <summary>
/// Extension methods for transforming API response DTOs into ASP.NET Core action results.
/// </summary>
public static class ApiResponseDtoExtensions
{
    /// <summary>
    /// Converts a generic API response DTO to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the result in the API response.</typeparam>
    /// <param name="apiResponseDto">The API response to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the API response. The response body will contain
    /// the complete ApiResponseDto including any result data and error messages.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponseDto is null.</exception>
    public static IActionResult ToHttpResult<T>(this ApiResponseDto<T> apiResponseDto)
    {
        ArgumentNullException.ThrowIfNull(apiResponseDto);

        return new ObjectResult(apiResponseDto)
        {
            StatusCode = (int)apiResponseDto.StatusCode
        };
    }

    /// <summary>
    /// Converts a base API response DTO to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <param name="apiResponseDto">The API response to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the API response. The response body will contain
    /// the complete ApiResponseDto including any error messages.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponseDto is null.</exception>
    public static IActionResult ToHttpResult(this ApiResponseDto apiResponseDto)
    {
        ArgumentNullException.ThrowIfNull(apiResponseDto);

        return new ObjectResult(apiResponseDto)
        {
            StatusCode = (int)apiResponseDto.StatusCode
        };
    }

    /// <summary>
    /// Creates a new ApiResponseDto with the specified HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <returns>A new ApiResponseDto with the specified status code.</returns>
    public static ApiResponseDto CreateResponse(this HttpStatusCode statusCode)
    {
        return new ApiResponseDto(statusCode);
    }

    /// <summary>
    /// Creates a new generic ApiResponseDto with the specified HTTP status code and result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <param name="result">The result object to include.</param>
    /// <returns>A new ApiResponseDto with the specified status code and result.</returns>
    public static ApiResponseDto<T> CreateResponse<T>(this HttpStatusCode statusCode, T? result)
    {
        return new ApiResponseDto<T>(statusCode, result);
    }
}
