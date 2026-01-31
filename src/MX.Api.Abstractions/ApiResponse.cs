using System.Net;
using Newtonsoft.Json;

namespace MX.Api.Abstractions;

/// <summary>
/// Represents a response from an API without data.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets or sets the collection of errors.
    /// </summary>
    [JsonProperty(PropertyName = "errors")]
    public ApiError[]? Errors { get; set; }

    /// <summary>
    /// Gets or sets the metadata dictionary.
    /// </summary>
    [JsonProperty(PropertyName = "metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse"/> class with the specified error.
    /// </summary>
    /// <param name="error">An error object.</param>
    public ApiResponse(ApiError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Errors = [error];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse"/> class with the specified errors.
    /// </summary>
    /// <param name="errors">An array of errors.</param>
    public ApiResponse(ApiError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }
}

/// <summary>
/// Represents a response from an API.
/// </summary>
/// <typeparam name="T">The type of the data in the response.</typeparam>
public class ApiResponse<T>
{
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
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified data.
    /// </summary>
    /// <param name="data">The data returned by the API.</param>
    public ApiResponse(T? data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified error.
    /// </summary>
    /// <param name="error">An error object.</param>
    public ApiResponse(ApiError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Errors = [error];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified errors.
    /// </summary>
    /// <param name="errors">An array of errors.</param>
    public ApiResponse(ApiError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified data and error.
    /// </summary>
    /// <param name="data">The data returned by the API.</param>
    /// <param name="error">An error object.</param>
    public ApiResponse(T? data, ApiError error) : this(data)
    {
        ArgumentNullException.ThrowIfNull(error);
        Errors = [error];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with the specified data and errors.
    /// </summary>
    /// <param name="data">The data returned by the API.</param>
    /// <param name="errors">An array of errors.</param>
    public ApiResponse(T? data, ApiError[] errors) : this(data)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }
}


