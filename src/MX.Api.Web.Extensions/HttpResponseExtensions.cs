using System.Net;

using Microsoft.AspNetCore.Mvc;

using MX.Api.Abstractions;

namespace MX.Api.Web.Extensions;

/// <summary>
/// Extension methods for transforming HTTP response wrappers into ASP.NET Core action results.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Converts an HTTP response wrapper with a generic API response to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the data in the API response.</typeparam>
    /// <param name="responseWrapper">The HTTP response wrapper to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the HTTP response. If the wrapper contains an API response,
    /// that will be the response body; otherwise, an empty result will be returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when responseWrapper is null.</exception>
    public static IActionResult ToHttpResult<T>(this IHttpResponseWrapper<T> responseWrapper)
    {
        ArgumentNullException.ThrowIfNull(responseWrapper);

        if (responseWrapper.Result != null)
        {
            return new ObjectResult(responseWrapper.Result)
            {
                StatusCode = (int)responseWrapper.StatusCode
            };
        }

        return new StatusCodeResult((int)responseWrapper.StatusCode);
    }

    /// <summary>
    /// Converts a base HTTP response wrapper to an HTTP action result with the appropriate status code.
    /// </summary>
    /// <param name="responseWrapper">The HTTP response wrapper to convert.</param>
    /// <returns>
    /// An IActionResult with the status code from the HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when responseWrapper is null.</exception>
    public static IActionResult ToHttpResult(this IHttpResponseWrapper responseWrapper)
    {
        ArgumentNullException.ThrowIfNull(responseWrapper);

        return new StatusCodeResult((int)responseWrapper.StatusCode);
    }

    /// <summary>
    /// Creates a new HTTP response wrapper with the specified HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <returns>A new HTTP response wrapper with the specified status code.</returns>
    public static HttpResponseWrapper CreateHttpResponse(this HttpStatusCode statusCode)
    {
        return new HttpResponseWrapper(statusCode);
    }

    /// <summary>
    /// Creates a new HTTP response wrapper with a generic API response.
    /// </summary>
    /// <typeparam name="T">The type of the data in the API response.</typeparam>
    /// <param name="statusCode">The HTTP status code to set.</param>
    /// <param name="apiResponse">The API response to include.</param>
    /// <returns>A new HTTP response wrapper with the specified status code and API response.</returns>
    public static HttpResponseWrapper<T> CreateHttpResponse<T>(this HttpStatusCode statusCode, ApiResponse<T> apiResponse)
    {
        return new HttpResponseWrapper<T>(statusCode, apiResponse);
    }
}

/// <summary>
/// Extension methods for creating API responses.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Creates a new API response.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <returns>A new API response.</returns>
    public static ApiResponse<T> CreateApiResponse<T>()
    {
        return new ApiResponse<T>();
    }

    /// <summary>
    /// Creates a new API response with the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The data object to include.</param>
    /// <returns>A new API response with the specified data.</returns>
    public static ApiResponse<T> CreateApiResponse<T>(T? data)
    {
        return new ApiResponse<T>(data);
    }

    /// <summary>
    /// Creates a new API response with the specified data and pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The data object to include.</param>
    /// <param name="pagination">The pagination information.</param>
    /// <returns>A new API response with the specified data and pagination.</returns>
    public static ApiResponse<T> CreateApiResponse<T>(T? data, ApiPagination pagination)
    {
        var response = new ApiResponse<T>(data);
        response.Pagination = pagination;
        return response;
    }

    /// <summary>
    /// Creates a new API response with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="error">The error object to include.</param>
    /// <returns>A new API response with the specified error.</returns>
    public static ApiResponse<T> CreateApiErrorResponse<T>(ApiError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new ApiResponse<T>(default, new[] { error });
    }

    /// <summary>
    /// Creates a new API response with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="message">The error message.</param>
    /// <returns>A new API response with the specified error message.</returns>
    public static ApiResponse<T> CreateApiErrorResponse<T>(string message)
    {
        return new ApiResponse<T>(default, new ApiError("Error", message));
    }

    /// <summary>
    /// Creates a new API response for a collection of items with pagination information.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    /// <param name="items">The collection of items.</param>
    /// <param name="totalCount">The total count of records available.</param>
    /// <param name="filteredCount">The count of records after filtering.</param>
    /// <param name="skip">The number of records skipped.</param>
    /// <param name="top">The number of records to take.</param>
    /// <returns>A new API response with the specified collection and pagination information.</returns>
    public static ApiResponse<CollectionModel<T>> CreateApiCollectionResponse<T>(
        IEnumerable<T>? items,
        int totalCount,
        int filteredCount,
        int skip,
        int top)
    {
        var collection = new CollectionModel<T>(items, totalCount, filteredCount);
        var response = new ApiResponse<CollectionModel<T>>
        {
            Data = collection,
            Pagination = new ApiPagination(totalCount, filteredCount, skip, top)
        };
        return response;
    }

    /// <summary>
    /// Creates a new API response for a count-only response.
    /// </summary>
    /// <param name="count">The count value.</param>
    /// <returns>A new API response with the specified count as data.</returns>
    public static ApiResponse<int> CreateApiCountResponse(int count)
    {
        return new ApiResponse<int>(count);
    }
}
