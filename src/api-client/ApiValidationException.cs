using System.Net;

namespace MxIO.ApiClient;

/// <summary>
/// Exception thrown when API validation fails.
/// </summary>
/// <remarks>
/// This exception is used when the API returns validation errors,
/// typically with a 400 Bad Request or 422 Unprocessable Entity status code.
/// </remarks>
public class ApiValidationException : ApiException
{
    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="resource">The resource that was being accessed.</param>
    /// <param name="method">The HTTP method that was being used.</param>
    /// <param name="validationErrors">Dictionary of field names and their validation error messages.</param>
    /// <param name="statusCode">The HTTP status code, defaults to 400 Bad Request.</param>
    /// <param name="responseContent">The response content, if available.</param>
    /// <param name="innerException">The inner exception, if available.</param>
    public ApiValidationException(
        string message,
        string resource,
        string method,
        IDictionary<string, IEnumerable<string>> validationErrors,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        string? responseContent = null,
        Exception? innerException = null)
        : base(message, resource, method, statusCode, responseContent, innerException)
    {
        ValidationErrors = new Dictionary<string, IEnumerable<string>>(validationErrors);
    }

    /// <summary>
    /// Gets a flattened list of all validation error messages.
    /// </summary>
    /// <returns>An array of all validation error messages.</returns>
    public string[] GetAllValidationMessages()
    {
        return ValidationErrors
            .SelectMany(kvp => kvp.Value.Select(message => $"{kvp.Key}: {message}"))
            .ToArray();
    }
}