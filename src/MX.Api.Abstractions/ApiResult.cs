using System.Net;

using Newtonsoft.Json;

namespace MX.Api.Abstractions;

/// <summary>
/// Represents the result of an API operation including both the business data and HTTP semantics.
/// This class provides separation between business concerns and HTTP transport concerns.
/// </summary>
public class ApiResult : IApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty(PropertyName = "statusCode")]
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the API response contained in this result.
    /// </summary>
    [JsonProperty(PropertyName = "result")]
    public ApiResponse? Result { get; set; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response was successful.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => StatusCode == HttpStatusCode.OK ||
                            StatusCode == HttpStatusCode.Created ||
                            StatusCode == HttpStatusCode.Accepted;

    /// <summary>
    /// Gets a value indicating whether the HTTP response indicates a resource was not found.
    /// </summary>
    [JsonIgnore]
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Gets a value indicating whether the HTTP response indicates a conflict occurred.
    /// </summary>
    [JsonIgnore]
    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult"/> class.
    /// </summary>
    public ApiResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public ApiResult(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult"/> class with the specified status code and API response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="result">The API response.</param>
    public ApiResult(HttpStatusCode statusCode, ApiResponse? result) : this(statusCode)
    {
        Result = result;
    }
}

/// <summary>
/// Represents the result of a strongly-typed API operation including both the business data and HTTP semantics.
/// This class provides separation between business concerns and HTTP transport concerns.
/// </summary>
/// <typeparam name="T">The type of data in the API response.</typeparam>
public class ApiResult<T> : ApiResult, IApiResult<T>
{
    /// <summary>
    /// Gets or sets the API response contained in this result.
    /// </summary>
    [JsonProperty(PropertyName = "result")]
    public new ApiResponse<T>? Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult{T}"/> class.
    /// </summary>
    public ApiResult() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public ApiResult(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with the specified status code and API response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="result">The API response.</param>
    public ApiResult(HttpStatusCode statusCode, ApiResponse<T>? result) : base(statusCode)
    {
        Result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the HTTP response was successful and contains a non-null API response.
    /// </summary>
    [JsonIgnore]
    public new bool IsSuccess => base.IsSuccess && Result != null;
}
