using System.Net;

using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions.V2;

/// <summary>
/// Interface for the HTTP response wrapper that contains an API response.
/// </summary>
public interface IHttpResponseWrapper
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
}

/// <summary>
/// Interface for the HTTP response wrapper that contains a generic API response.
/// </summary>
/// <typeparam name="T">The type of data in the API response.</typeparam>
public interface IHttpResponseWrapper<T> : IHttpResponseWrapper
{
    /// <summary>
    /// Gets the API response contained in this HTTP response wrapper.
    /// </summary>
    ApiResponse<T>? Result { get; }
}

/// <summary>
/// A wrapper for HTTP responses that includes an API response.
/// This class provides a separation between HTTP-level concerns and the API response model.
/// </summary>
public class HttpResponseWrapper : IHttpResponseWrapper
{
    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty(PropertyName = "statusCode")]
    public HttpStatusCode StatusCode { get; set; }

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
    /// Initializes a new instance of the <see cref="HttpResponseWrapper"/> class.
    /// </summary>
    public HttpResponseWrapper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseWrapper"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HttpResponseWrapper(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// A wrapper for HTTP responses that includes a strongly-typed API response.
/// This class provides a separation between HTTP-level concerns and the API response model.
/// </summary>
/// <typeparam name="T">The type of data in the API response.</typeparam>
public class HttpResponseWrapper<T> : HttpResponseWrapper, IHttpResponseWrapper<T>
{
    /// <summary>
    /// Gets or sets the API response contained in this HTTP response wrapper.
    /// </summary>
    [JsonProperty(PropertyName = "result")]
    public ApiResponse<T>? Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseWrapper{T}"/> class.
    /// </summary>
    public HttpResponseWrapper() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseWrapper{T}"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HttpResponseWrapper(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseWrapper{T}"/> class with the specified status code and API response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="result">The API response.</param>
    public HttpResponseWrapper(HttpStatusCode statusCode, ApiResponse<T>? result) : base(statusCode)
    {
        Result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the HTTP response was successful and contains a non-null API response.
    /// </summary>
    [JsonIgnore]
    public new bool IsSuccess => base.IsSuccess && Result != null;
}
