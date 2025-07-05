using System.Net;

namespace MX.Api.Abstractions;

/// <summary>
/// Interface for API operation results that contain an API response.
/// </summary>
public interface IApiResult
{
    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response indicates a resource was not found.
    /// </summary>
    bool IsNotFound { get; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response indicates a conflict occurred.
    /// </summary>
    bool IsConflict { get; }

    /// <summary>
    /// Gets the API response contained in this result.
    /// </summary>
    ApiResponse? Result { get; }
}

/// <summary>
/// Interface for API operation results that contain a strongly-typed API response.
/// </summary>
/// <typeparam name="T">The type of data in the API response.</typeparam>
public interface IApiResult<T> : IApiResult
{
    /// <summary>
    /// Gets the API response contained in this result.
    /// </summary>
    new ApiResponse<T>? Result { get; }
}