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
}
