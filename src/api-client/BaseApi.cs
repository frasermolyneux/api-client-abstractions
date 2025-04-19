using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Retry;

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MxIO.ApiClient;

/// <summary>
/// Base class for API clients providing common functionality for REST API calls.
/// This class handles authentication, retries, and error management for REST API operations.
/// </summary>
public class BaseApi
{
    private const string SubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";
    private const string AuthorizationHeaderName = "Authorization";
    private const string BearerTokenPrefix = "Bearer ";

    private readonly ILogger<BaseApi> logger;
    private readonly IApiTokenProvider apiTokenProvider;
    private readonly IRestClientSingleton restClientSingleton;

    private readonly string baseUrl;
    private readonly string primaryApiKey;
    private readonly string secondaryApiKey;
    private readonly string apiAudience;

    private readonly AsyncRetryPolicy<RestResponse> retryPolicy;

    private static readonly HttpStatusCode[] SuccessStatusCodes = { HttpStatusCode.OK, HttpStatusCode.NotFound };
    private static readonly HttpStatusCode[] NoRetryStatusCodes = { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity };
    private static readonly HttpStatusCode[] ValidationStatusCodes = { HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity };

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApi"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="apiTokenProvider">The API token provider.</param>
    /// <param name="restClientSingleton">The REST client singleton.</param>
    /// <param name="options">The API client options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public BaseApi(
        ILogger<BaseApi> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientSingleton restClientSingleton,
        IOptions<ApiClientOptions> options)
    {
        this.logger = logger ?? new NullLogger<BaseApi>();
        this.apiTokenProvider = apiTokenProvider ?? throw new ArgumentNullException(nameof(apiTokenProvider));
        this.restClientSingleton = restClientSingleton ?? throw new ArgumentNullException(nameof(restClientSingleton));

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        if (string.IsNullOrEmpty(options.Value.BaseUrl))
        {
            throw new ArgumentException("BaseUrl must be specified in ApiClientOptions", nameof(options));
        }

        if (string.IsNullOrEmpty(options.Value.PrimaryApiKey))
        {
            throw new ArgumentException("PrimaryApiKey must be specified in ApiClientOptions", nameof(options));
        }

        if (string.IsNullOrEmpty(options.Value.ApiAudience))
        {
            throw new ArgumentException("ApiAudience must be specified in ApiClientOptions", nameof(options));
        }

        // Construct the base URL with optional path prefix
        baseUrl = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
            ? options.Value.BaseUrl
            : $"{options.Value.BaseUrl.TrimEnd('/')}/{options.Value.ApiPathPrefix.TrimStart('/')}";

        primaryApiKey = options.Value.PrimaryApiKey;
        secondaryApiKey = options.Value.SecondaryApiKey;
        apiAudience = options.Value.ApiAudience;

        // Configure retry policy - safely handle null responses
        int retryCount = options.Value.MaxRetryCount > 0 ? options.Value.MaxRetryCount : 3;

        retryPolicy = Policy
            .HandleResult<RestResponse>(r =>
                r is null || !NoRetryStatusCodes.Contains(r.StatusCode))
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (response, timespan, attemptCount, context) =>
                {
                    if (response?.Result is not null)
                    {
                        this.logger.LogWarning("Request failed with {StatusCode}. Waiting {Timespan} before next retry. Retry attempt {RetryCount}",
                            response.Result.StatusCode, timespan, attemptCount);
                    }
                    else
                    {
                        this.logger.LogWarning("Request failed with null response. Waiting {Timespan} before next retry. Retry attempt {RetryCount}",
                            timespan, attemptCount);
                    }
                }
            );
    }

    /// <summary>
    /// Creates a new REST request with the appropriate headers and authentication.
    /// </summary>
    /// <param name="resource">The API resource path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A configured REST request object.</returns>
    /// <exception cref="ArgumentException">Thrown if the resource is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="ApiAuthenticationException">Thrown when authentication token acquisition fails.</exception>
    public async Task<RestRequest> CreateRequestAsync(string resource, Method method, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
        }

        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        string accessToken;
        try
        {
            accessToken = await apiTokenProvider.GetAccessTokenAsync(apiAudience, cancellationToken);
        }
        catch (ApiAuthenticationException ex)
        {
            logger.LogError(ex, "Failed to get authentication token for resource '{Resource}' with audience '{Audience}'",
                resource, apiAudience);
            throw;
        }

        var request = new RestRequest(resource, method);

        request.AddHeader(SubscriptionKeyHeaderName, primaryApiKey);
        request.AddHeader(AuthorizationHeaderName, $"{BearerTokenPrefix}{accessToken}");

        return request;
    }

    /// <summary>
    /// Executes the REST request and handles errors and retries.
    /// </summary>
    /// <param name="request">The REST request to execute.</param>
    /// <param name="useSecondaryApiKey">Whether to use the secondary API key.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The REST response.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="ApiException">Thrown when the request fails with an unexpected status code.</exception>
    /// <exception cref="ApiValidationException">Thrown when the request fails due to validation errors.</exception>
    public async Task<RestResponse> ExecuteAsync(RestRequest request, bool useSecondaryApiKey = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        // Apply secondary API key if requested and available
        if (useSecondaryApiKey && !string.IsNullOrWhiteSpace(secondaryApiKey))
        {
            logger.LogInformation("Retrying with secondary API key for '{Resource}'", request.Resource);
            request.AddOrUpdateHeader(SubscriptionKeyHeaderName, secondaryApiKey);
        }

        try
        {
            // Execute the request with retry policy
            var response = await retryPolicy.ExecuteAsync(
                async (token) => await restClientSingleton.ExecuteAsync(baseUrl, request, token),
                cancellationToken);

            // Ensure response is not null to prevent NullReferenceException
            if (response is null)
            {
                logger.LogError("Received null response for {Method} to '{Resource}'", request.Method, request.Resource);
                throw new ApiException(
                    $"Failed {request.Method} to '{request.Resource}' - received null response",
                    request.Resource ?? string.Empty,
                    request.Method.ToString());
            }

            // Check for specific error conditions
            if (response.StatusCode == HttpStatusCode.Unauthorized && !useSecondaryApiKey)
            {
                var responseContent = response.Content;

                if (responseContent is not null && responseContent.Contains("Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription."))
                {
                    return await ExecuteAsync(request, true, cancellationToken);
                }
            }

            // Return successful responses
            if (SuccessStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }
            // Handle validation errors - new code for ApiValidationException
            else if (ValidationStatusCodes.Contains(response.StatusCode) && !string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    var validationErrors = new Dictionary<string, IEnumerable<string>>();

                    // Try to parse the validation errors from the response
                    var jsonObject = JObject.Parse(response.Content);

                    // Handle standard ASP.NET Core validation response format
                    if (jsonObject.ContainsKey("errors") && jsonObject["errors"] is JObject errors)
                    {
                        foreach (var property in errors.Properties())
                        {
                            var fieldName = property.Name;
                            var errorMessages = property.Value?.ToObject<string[]>() ?? Array.Empty<string>();
                            validationErrors[fieldName] = errorMessages;
                        }
                    }
                    // If we couldn't parse it in the expected format, add the whole content as a general error
                    else
                    {
                        validationErrors["General"] = new[] { response.Content };
                    }

                    if (validationErrors.Count > 0)
                    {
                        var validationException = new ApiValidationException(
                            $"Validation failed for {request.Method} to '{request.Resource}' with code '{response.StatusCode}'",
                            request.Resource ?? string.Empty,
                            request.Method.ToString(),
                            validationErrors,
                            response.StatusCode,
                            response.Content);

                        logger.LogWarning(validationException, "Validation failed for {Method} to '{Resource}' - Content: {Content}",
                            request.Method, request.Resource, response.Content);
                        throw validationException;
                    }
                }
                catch (JsonException)
                {
                    // If we can't parse the content as JSON, fall back to regular ApiException
                }
            }

            // Handle exceptions in the response
            if (response.ErrorException is not null)
            {
                logger.LogError(response.ErrorException, "Failed {Method} to '{Resource}' with code '{StatusCode}'",
                    request.Method, request.Resource, response.StatusCode);
                throw new ApiException(
                    $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'",
                    request.Resource ?? string.Empty,
                    request.Method.ToString(),
                    response.StatusCode,
                    response.Content,
                    response.ErrorException);
            }
            // Handle other error status codes
            else
            {
                var ex = new ApiException(
                    $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'",
                    request.Resource ?? string.Empty,
                    request.Method.ToString(),
                    response.StatusCode,
                    response.Content);

                logger.LogError(ex, "Failed {Method} to '{Resource}' with response status '{ResponseStatus}' and code '{StatusCode}' - Content: {Content}",
                    request.Method, request.Resource, response.ResponseStatus, response.StatusCode, response.Content);
                throw ex;
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request {Method} to '{Resource}' was canceled", request.Method, request.Resource);
            throw;
        }
        catch (ApiAuthenticationException authEx)
        {
            logger.LogError(authEx, "Authentication failed during {Method} to '{Resource}'",
                request.Method, request.Resource);
            throw;
        }
        catch (ObjectDisposedException ex)
        {
            logger.LogError(ex, "RestClientSingleton was disposed during request {Method} to '{Resource}'",
                request.Method, request.Resource);
            throw;
        }
        catch (Exception ex) when (ex is not ApiException and not ApiValidationException and not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error during {Method} to '{Resource}'",
                request.Method, request.Resource);
            throw;
        }

        // This code should never be reached due to the throw statements above, but we need to add a return
        // to satisfy the compiler requirement that all code paths return a value
        throw new InvalidOperationException("Unexpected execution path in ExecuteAsync method");
    }
}