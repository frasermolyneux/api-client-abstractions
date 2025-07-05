using System.Net;

using MX.Api.Abstractions;

namespace MX.Api.Web.Extensions;

/// <summary>
/// Extension methods for converting API responses to API results.
/// These extensions help transform API client responses into standardized API results for use in controllers.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Converts an API response to an API result with the specified HTTP status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <param name="statusCode">The HTTP status code to associate with the result.</param>
    /// <returns>An ApiResult containing the response and status code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToApiResult(this ApiResponse apiResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(statusCode, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to a generic API result with the specified HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <param name="statusCode">The HTTP status code to associate with the result.</param>
    /// <returns>An ApiResult&lt;T&gt; containing the response and status code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToApiResult<T>(this ApiResponse<T> apiResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(statusCode, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result with a status determined by the presence of errors.
    /// Returns HTTP 200 OK if no errors are present, or HTTP 400 Bad Request if errors exist.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with status based on error presence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToApiResultWithErrorHandling(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        var statusCode = apiResponse.Errors?.Length > 0
            ? HttpStatusCode.BadRequest
            : HttpStatusCode.OK;

        return new ApiResult(statusCode, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result with a status determined by the presence of errors and data.
    /// Returns HTTP 200 OK if no errors and data is present, HTTP 404 Not Found if no errors but data is null,
    /// or HTTP 400 Bad Request if errors exist.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with status based on error presence and data availability.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToApiResultWithErrorHandling<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        HttpStatusCode statusCode;

        if (apiResponse.Errors?.Length > 0)
        {
            statusCode = HttpStatusCode.BadRequest;
        }
        else if (apiResponse.Data == null)
        {
            statusCode = HttpStatusCode.NotFound;
        }
        else
        {
            statusCode = HttpStatusCode.OK;
        }

        return new ApiResult<T>(statusCode, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result for a successful creation operation.
    /// Returns HTTP 201 Created status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with HTTP 201 Created status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToCreatedResult(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(HttpStatusCode.Created, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result for a successful creation operation.
    /// Returns HTTP 201 Created status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with HTTP 201 Created status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToCreatedResult<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(HttpStatusCode.Created, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result for a successful update operation.
    /// Returns HTTP 202 Accepted status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with HTTP 202 Accepted status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToAcceptedResult(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(HttpStatusCode.Accepted, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result for a successful update operation.
    /// Returns HTTP 202 Accepted status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with HTTP 202 Accepted status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToAcceptedResult<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(HttpStatusCode.Accepted, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result indicating a resource was not found.
    /// Returns HTTP 404 Not Found status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with HTTP 404 Not Found status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToNotFoundResult(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(HttpStatusCode.NotFound, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result indicating a resource was not found.
    /// Returns HTTP 404 Not Found status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with HTTP 404 Not Found status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToNotFoundResult<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(HttpStatusCode.NotFound, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result indicating a validation error or bad request.
    /// Returns HTTP 400 Bad Request status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with HTTP 400 Bad Request status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToBadRequestResult(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(HttpStatusCode.BadRequest, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result indicating a validation error or bad request.
    /// Returns HTTP 400 Bad Request status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with HTTP 400 Bad Request status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToBadRequestResult<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(HttpStatusCode.BadRequest, apiResponse);
    }

    /// <summary>
    /// Converts an API response to an API result indicating a conflict occurred.
    /// Returns HTTP 409 Conflict status code.
    /// </summary>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult with HTTP 409 Conflict status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult ToConflictResult(this ApiResponse apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult(HttpStatusCode.Conflict, apiResponse);
    }

    /// <summary>
    /// Converts a generic API response to an API result indicating a conflict occurred.
    /// Returns HTTP 409 Conflict status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the API response.</typeparam>
    /// <param name="apiResponse">The API response to convert.</param>
    /// <returns>An ApiResult&lt;T&gt; with HTTP 409 Conflict status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when apiResponse is null.</exception>
    public static ApiResult<T> ToConflictResult<T>(this ApiResponse<T> apiResponse)
    {
        ArgumentNullException.ThrowIfNull(apiResponse);

        return new ApiResult<T>(HttpStatusCode.Conflict, apiResponse);
    }
}
