using System.Net;

namespace MxIO.ApiClient;

/// <summary>
/// Exception thrown when an API operation fails.
/// </summary>
public class ApiException : ApplicationException
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the resource that was being accessed when the exception occurred.
    /// </summary>
    public string Resource { get; }

    /// <summary>
    /// Gets the HTTP method that was being used when the exception occurred.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the response content, if available.
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="resource">The resource that was being accessed.</param>
    /// <param name="method">The HTTP method that was being used.</param>
    /// <param name="statusCode">The HTTP status code, if available.</param>
    /// <param name="responseContent">The response content, if available.</param>
    /// <param name="innerException">The inner exception, if available.</param>
    public ApiException(
        string message,
        string resource,
        string method,
        HttpStatusCode? statusCode = null,
        string? responseContent = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Resource = resource;
        Method = method;
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}