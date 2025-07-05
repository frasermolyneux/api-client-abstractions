using System.Net;

using Microsoft.AspNetCore.Mvc;

using MX.Api.Abstractions;

namespace MX.Api.Web.Extensions;

/// <summary>
/// Extension methods for transforming API results into ASP.NET Core action results.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Converts an API result with a generic API response to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the data in the API response.</typeparam>
    /// <param name="apiResult">The API result to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the API result. If the result contains an API response,
    /// that will be the response body; otherwise, an empty result will be returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResult is null.</exception>
    public static IActionResult ToHttpResult<T>(this IApiResult<T> apiResult)
    {
        ArgumentNullException.ThrowIfNull(apiResult);

        if (apiResult.Result != null)
        {
            return new ObjectResult(apiResult.Result)
            {
                StatusCode = (int)apiResult.StatusCode
            };
        }

        return new StatusCodeResult((int)apiResult.StatusCode);
    }

    /// <summary>
    /// Converts a base API result to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <param name="apiResult">The API result to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the API result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResult is null.</exception>
    public static IActionResult ToHttpResult(this IApiResult apiResult)
    {
        ArgumentNullException.ThrowIfNull(apiResult);

        return new StatusCodeResult((int)apiResult.StatusCode);
    }

    /// <summary>
    /// Creates a new API result with the specified HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <returns>A new API result with the specified status code.</returns>
    public static ApiResult CreateApiResult(this HttpStatusCode statusCode)
    {
        return new ApiResult(statusCode);
    }

    /// <summary>
    /// Creates a new API result with a generic API response.
    /// </summary>
    /// <typeparam name="T">The type of the data in the API response.</typeparam>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <param name="apiResponse">The API response to include.</param>
    /// <returns>A new API result with the specified status code and API response.</returns>
    public static ApiResult<T> CreateApiResult<T>(this HttpStatusCode statusCode, ApiResponse<T> apiResponse)
    {
        return new ApiResult<T>(statusCode, apiResponse);
    }
}
