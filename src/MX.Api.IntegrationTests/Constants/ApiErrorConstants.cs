namespace MX.Api.IntegrationTests.Constants;

/// <summary>
/// Constants for API error codes and messages used across the integration test dummy APIs
/// </summary>
public static class ApiErrorConstants
{
    /// <summary>
    /// Error codes
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Unauthorized access error code
        /// </summary>
        public const string Unauthorized = "UNAUTHORIZED";

        /// <summary>
        /// Resource not found error code
        /// </summary>
        public const string NotFound = "NOT_FOUND";

        /// <summary>
        /// Invalid API key error code
        /// </summary>
        public const string InvalidApiKey = "INVALID_API_KEY";

        /// <summary>
        /// Resource already exists error code
        /// </summary>
        public const string ResourceExists = "RESOURCE_EXISTS";
    }

    /// <summary>
    /// Error messages
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Generic unauthorized access message
        /// </summary>
        public const string UnauthorizedAccess = "Unauthorized access";

        /// <summary>
        /// Invalid or missing authentication message
        /// </summary>
        public const string InvalidOrMissingAuthentication = "Invalid or missing authentication";

        /// <summary>
        /// Authorization header required message
        /// </summary>
        public const string AuthorizationHeaderRequired = "Authorization header is required";

        /// <summary>
        /// API key required message
        /// </summary>
        public const string ApiKeyRequired = "API key is required";

        /// <summary>
        /// Invalid API key message
        /// </summary>
        public const string InvalidApiKey = "Invalid API key";

        /// <summary>
        /// Resource not found message template
        /// </summary>
        public const string ResourceNotFound = "{0} with ID {1} not found";

        /// <summary>
        /// Resource already exists message template
        /// </summary>
        public const string ResourceAlreadyExists = "{0} with {1} {2} already exists";
    }

    /// <summary>
    /// Error details/descriptions
    /// </summary>
    public static class ErrorDetails
    {
        /// <summary>
        /// Bearer token required detail
        /// </summary>
        public const string BearerTokenRequired = "Bearer token is required";

        /// <summary>
        /// Bearer token missing detail
        /// </summary>
        public const string BearerTokenMissing = "Bearer token is missing";

        /// <summary>
        /// Invalid bearer token detail
        /// </summary>
        public const string InvalidBearerToken = "The provided Bearer token is not valid";

        /// <summary>
        /// X-API-Key header missing detail
        /// </summary>
        public const string XApiKeyHeaderMissing = "X-API-Key header is missing";

        /// <summary>
        /// Invalid API key detail
        /// </summary>
        public const string InvalidApiKeyProvided = "The provided API key is not valid";

        /// <summary>
        /// Resource does not exist detail template
        /// </summary>
        public const string ResourceDoesNotExist = "The requested {0} does not exist";

        /// <summary>
        /// Resource already exists detail template
        /// </summary>
        public const string ResourceAlreadyExistsDetail = "A {0} with the specified {1} already exists in the system";
    }
}
