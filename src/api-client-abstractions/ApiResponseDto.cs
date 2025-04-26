using System.Net;

using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Interface for API responses.
/// </summary>
public interface IApiResponseDto
{
    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets or sets the collection of error messages.
    /// </summary>
    List<string> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the response was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates a resource was not found.
    /// </summary>
    bool IsNotFound { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates a conflict occurred.
    /// </summary>
    bool IsConflict { get; }
}

/// <summary>
/// Generic interface for API responses with a strongly-typed result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public interface IApiResponseDto<T> : IApiResponseDto
{
    /// <summary>
    /// Gets or sets the result data.
    /// </summary>
    T? Result { get; }
}

/// <summary>
/// Base class for API responses.
/// </summary>
public record ApiResponseDto : IApiResponseDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiResponseDto()
    {
        Errors = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public ApiResponseDto(HttpStatusCode statusCode)
        : this()
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto"/> class with the specified status code and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="errors">A list of error messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when errors is null.</exception>
    public ApiResponseDto(HttpStatusCode statusCode, List<string> errors)
        : this(statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty(PropertyName = "statusCode")]
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the collection of error messages.
    /// </summary>
    [JsonProperty(PropertyName = "errors")]
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the response was successful.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => (StatusCode == HttpStatusCode.OK ||
                              StatusCode == HttpStatusCode.Created ||
                              StatusCode == HttpStatusCode.Accepted) &&
                             (Errors == null || !Errors.Any());

    /// <summary>
    /// Gets a value indicating whether the response indicates a resource was not found.
    /// </summary>
    [JsonIgnore]
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Gets a value indicating whether the response indicates a conflict occurred.
    /// </summary>
    [JsonIgnore]
    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;
}

/// <summary>
/// Generic API response with a strongly-typed result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public record ApiResponseDto<T> : ApiResponseDto, IApiResponseDto<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiResponseDto()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public ApiResponseDto(HttpStatusCode statusCode)
        : base(statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class with the specified status code and result.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="result">The result data.</param>
    public ApiResponseDto(HttpStatusCode statusCode, T? result)
        : base(statusCode)
    {
        Result = result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class with the specified status code, result, and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="result">The result data.</param>
    /// <param name="errors">A list of error messages.</param>
    /// <exception cref="ArgumentNullException">Thrown if errors is null.</exception>
    public ApiResponseDto(HttpStatusCode statusCode, T? result, List<string> errors)
        : base(statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Result = result;
        Errors = errors;
    }

    /// <summary>
    /// Gets or sets the result data.
    /// </summary>
    [JsonProperty(PropertyName = "result")]
    public T? Result { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the response was successful and contains a non-null result.
    /// </summary>
    [JsonIgnore]
    public new bool IsSuccess => (StatusCode == HttpStatusCode.OK ||
                                  StatusCode == HttpStatusCode.Created ||
                                  StatusCode == HttpStatusCode.Accepted) &&
                                 !Errors.Any() &&
                                 Result != null;
}
