using Newtonsoft.Json;

namespace MX.Api.Abstractions;

/// <summary>
/// Represents an error in an API response.
/// </summary>
public class ApiError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonProperty(PropertyName = "code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target of the error, such as a field name.
    /// </summary>
    [JsonProperty(PropertyName = "target")]
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the nested error details.
    /// </summary>
    [JsonProperty(PropertyName = "details")]
    public ApiError[]? Details { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiError"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiError()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiError"/> class with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiError"/> class with the specified code, message, and target.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The target of the error, such as a field name.</param>
    public ApiError(string code, string message, string target)
    {
        Code = code;
        Message = message;
        Target = target;
    }
}
