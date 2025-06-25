using System.Net;

using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Represents a response from an API.
/// </summary>
/// <typeparam name="T">The type of the data in the response.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty(PropertyName = "statusCode")]
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the data returned by the API.
    /// </summary>
    [JsonProperty(PropertyName = "data")]
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the collection of errors.
    /// </summary>
    [JsonProperty(PropertyName = "errors")]
    public ApiError[]? Errors { get; set; }

    /// <summary>
    /// Gets or sets the pagination information.
    /// </summary>
    [JsonProperty(PropertyName = "pagination")]
    public ApiPagination? Pagination { get; set; }

    /// <summary>
    /// Gets or sets the metadata dictionary.
    /// </summary>
    [JsonProperty(PropertyName = "metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public ApiResponse(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code and data.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="data">The data returned by the API.</param>
    public ApiResponse(HttpStatusCode statusCode, T? data) : this(statusCode)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code and error.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="error">An error object.</param>
    public ApiResponse(HttpStatusCode statusCode, ApiError error) : this(statusCode)
    {
        ArgumentNullException.ThrowIfNull(error);
        Errors = new[] { error };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code and errors.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="errors">An array of errors.</param>
    public ApiResponse(HttpStatusCode statusCode, ApiError[] errors) : this(statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code, data, and error.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="data">The data returned by the API.</param>
    /// <param name="error">An error object.</param>
    public ApiResponse(HttpStatusCode statusCode, T? data, ApiError error) : this(statusCode, data)
    {
        ArgumentNullException.ThrowIfNull(error);
        Errors = new[] { error };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified status code, data, and errors.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="data">The data returned by the API.</param>
    /// <param name="errors">An array of errors.</param>
    public ApiResponse(HttpStatusCode statusCode, T? data, ApiError[] errors) : this(statusCode, data)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }
}


