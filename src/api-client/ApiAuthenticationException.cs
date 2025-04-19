namespace MxIO.ApiClient;

/// <summary>
/// Exception thrown when authentication fails during API operations.
/// </summary>
/// <remarks>
/// This exception is used to differentiate authentication failures from other types of API errors.
/// </remarks>
public class ApiAuthenticationException : ApplicationException
{
    /// <summary>
    /// Gets the audience for which authentication failed.
    /// </summary>
    public string Audience { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="audience">The audience for which authentication failed.</param>
    /// <param name="innerException">The inner exception, if available.</param>
    public ApiAuthenticationException(
        string message,
        string audience,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Audience = audience;
    }
}