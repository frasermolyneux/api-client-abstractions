using System.Net;

using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Base class for API responses.
/// </summary>
public record ApiResponseDto
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
    /// Initializes a new instance of the <see cref="ApiResponseDto"/> class with the specified status code and error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="error">An error message describing the issue.</param>
    public ApiResponseDto(HttpStatusCode statusCode, string error)
        : this(statusCode)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Errors.Add(error);
        }
    }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty(PropertyName = "statusCode")]
    public HttpStatusCode StatusCode { get; internal set; }

    /// <summary>
    /// Gets or sets the collection of error messages.
    /// </summary>
    [JsonProperty(PropertyName = "errors")]
    public List<string> Errors { get; internal set; } = new();

    /// <summary>
    /// Gets a value indicating whether the response was successful.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => StatusCode == HttpStatusCode.OK && !Errors.Any();

    /// <summary>
    /// Gets a value indicating whether the response indicates a resource was not found.
    /// </summary>
    [JsonIgnore]
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
}

/// <summary>
/// Generic API response with a strongly-typed result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public record ApiResponseDto<T> : ApiResponseDto
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
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class with the specified status code and error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="error">An error message describing the issue.</param>
    public ApiResponseDto(HttpStatusCode statusCode, string error)
        : base(statusCode, error)
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
    public new bool IsSuccess => StatusCode == HttpStatusCode.OK && !Errors.Any() && Result != null;
}
