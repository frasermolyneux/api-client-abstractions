using System.Net;
using MX.Api.Abstractions;

namespace MX.Api.Client;

/// <summary>
/// An HTTP failure retaining the status and structured API error response.
/// </summary>
public sealed class ApiHttpRequestException(
    string message,
    HttpStatusCode statusCode,
    ApiResponse? errorResponse) : HttpRequestException(message, null, statusCode)
{
    /// <summary>
    /// The API error envelope, when available. Consumers must select safe errors before displaying them.
    /// </summary>
    public ApiResponse? ErrorResponse { get; } = errorResponse;
}
