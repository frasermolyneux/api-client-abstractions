using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MxIO.ApiClient.Abstractions;
using MxIO.ApiClient.Extensions;

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
    private const string AuthorizationHeaderName = "Authorization";
    private const string BearerTokenPrefix = "Bearer ";

    private readonly ILogger<BaseApi> logger;
    private readonly IApiTokenProvider? apiTokenProvider;
    private readonly IRestClientSingleton restClientSingleton;
    private readonly ApiClientOptions options;

    private readonly AsyncRetryPolicy<RestResponse> retryPolicy;

    private static readonly HttpStatusCode[] SuccessStatusCodes = { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent, HttpStatusCode.NotFound };
    private static readonly HttpStatusCode[] NoRetryStatusCodes = { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity };
    private static readonly HttpStatusCode[] ValidationStatusCodes = { HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity };

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApi"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="apiTokenProvider">The optional API token provider (required for Entra ID authentication).</param>
    /// <param name="restClientSingleton">The REST client singleton.</param>
    /// <param name="options">The API client options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public BaseApi(
        ILogger<BaseApi> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientSingleton restClientSingleton,
        IOptions<ApiClientOptions> options)
    {
        this.logger = logger ?? new NullLogger<BaseApi>();
        this.apiTokenProvider = apiTokenProvider;
        this.restClientSingleton = restClientSingleton ?? throw new ArgumentNullException(nameof(restClientSingleton));

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        if (string.IsNullOrEmpty(options.Value.BaseUrl))
        {
            throw new ArgumentException("BaseUrl must be specified in ApiClientOptions", nameof(options));
        }

        if (options.Value.AuthenticationOptions is EntraIdAuthenticationOptions && apiTokenProvider == null)
        {
            throw new ArgumentException("IApiTokenProvider must be provided when using Entra ID authentication", nameof(apiTokenProvider));
        }

        this.options = options.Value;

        // Configure retry policy - safely handle null responses
        int retryCount = this.options.MaxRetryCount > 0 ? this.options.MaxRetryCount : 3;

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
    /// <exception cref="AuthenticationException">Thrown when authentication token acquisition fails.</exception>
    public async Task<RestRequest> CreateRequestAsync(string resource, Method method, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
        }

        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        var request = new RestRequest(resource, method);

        // Apply authentication based on the configured authentication type
        if (options.AuthenticationOptions is ApiKeyAuthenticationOptions apiKeyOptions)
        {
            // Add API key header
            request.AddHeader(apiKeyOptions.HeaderName, apiKeyOptions.ApiKey);
            logger.LogDebug("Added API key authentication with header '{HeaderName}'", apiKeyOptions.HeaderName);
        }
        else if (options.AuthenticationOptions is EntraIdAuthenticationOptions entraIdOptions)
        {
            // Apply Entra ID token authentication
            try
            {
                if (apiTokenProvider == null)
                {
                    throw new InvalidOperationException("IApiTokenProvider not available for Entra ID authentication");
                }

                string accessToken = await apiTokenProvider.GetAccessTokenAsync(entraIdOptions.ApiAudience, cancellationToken);
                request.AddHeader(AuthorizationHeaderName, $"{BearerTokenPrefix}{accessToken}");
                logger.LogDebug("Added Entra ID token authentication for audience '{Audience}'", entraIdOptions.ApiAudience);
            }
            catch (AuthenticationException ex)
            {
                logger.LogError(ex, "Failed to get authentication token for resource '{Resource}' with audience '{Audience}'",
                    resource, entraIdOptions.ApiAudience);
                throw;
            }
        }

        return request;
    }

    /// <summary>
    /// Executes the REST request and handles errors and retries.
    /// </summary>
    /// <param name="request">The REST request to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the REST response.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails with an unexpected status code.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request fails due to validation errors.</exception>
    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Build the base URL with optional path prefix
            string baseUrl = string.IsNullOrWhiteSpace(options.ApiPathPrefix)
                ? options.BaseUrl
                : $"{options.BaseUrl.TrimEnd('/')}/{options.ApiPathPrefix.TrimStart('/')}";

            // Execute the request with retry policy
            var response = await retryPolicy.ExecuteAsync(
                async (token) => await restClientSingleton.ExecuteAsync(baseUrl, request, token),
                cancellationToken);

            // Ensure response is not null to prevent NullReferenceException
            if (response is null)
            {
                logger.LogError("Received null response for {Method} to '{Resource}'", request.Method, request.Resource);
                throw new HttpRequestException(
                    $"Failed {request.Method} to '{request.Resource}' - received null response");
            }

            // Handle based on status code
            if (SuccessStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }
            else if (ValidationStatusCodes.Contains(response.StatusCode))
            {
                logger.LogWarning("Validation error for {Method} to '{Resource}': {Content}",
                    request.Method, request.Resource, response.Content);
                throw new InvalidOperationException($"Validation failed: {response.Content}");
            }
            else
            {
                logger.LogError("HTTP error {StatusCode} for {Method} to '{Resource}': {Content}",
                    response.StatusCode, request.Method, request.Resource, response.Content);
                throw new HttpRequestException(
                    $"Failed {request.Method} to '{request.Resource}' with status code {response.StatusCode}: {response.Content}");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request to '{Resource}' was canceled", request.Resource);
            throw;
        }
        catch (HttpRequestException ex)
        {
            // Re-throw HttpRequestException - already logged
            throw;
        }
        catch (InvalidOperationException ex)
        {
            // Re-throw InvalidOperationException - already logged
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error executing {Method} request to '{Resource}'",
                request.Method, request.Resource);
            throw;
        }
    }
}
