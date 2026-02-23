using System.Net;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using Polly;
using Polly.Retry;
using RestSharp;

namespace MX.Api.Client;

/// <summary>
/// Base class for API clients providing common functionality for REST API calls.
/// This class handles authentication, retries, and error management for REST API operations.
/// </summary>
/// <typeparam name="TOptions">The type of options for this client</typeparam>
public class BaseApi<TOptions>
    where TOptions : ApiClientOptionsBase
{
    private const string AuthorizationHeaderName = "Authorization";
    private const string BearerTokenPrefix = "Bearer ";

    private readonly ILogger<BaseApi<TOptions>> logger;
    private readonly IApiTokenProvider? apiTokenProvider;
    private readonly IRestClientService restClientService;
    private readonly TOptions options;
    private readonly AsyncRetryPolicy<RestResponse> retryPolicy;

    private static readonly HttpStatusCode[] SuccessStatusCodes = [HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent, HttpStatusCode.NotFound];
    private static readonly HttpStatusCode[] NoRetryStatusCodes = [HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity];

    /// <summary>
    /// The strongly-typed options for this client
    /// </summary>
    protected TOptions Options => options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApi{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="apiTokenProvider">The optional API token provider (required for Entra ID authentication).</param>
    /// <param name="restClientService">The REST client service.</param>
    /// <param name="options">The strongly-typed API client options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public BaseApi(
        ILogger<BaseApi<TOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        TOptions options)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.apiTokenProvider = apiTokenProvider;
        this.restClientService = restClientService ?? throw new ArgumentNullException(nameof(restClientService));

        ArgumentNullException.ThrowIfNull(options);

        // Validate the options
        options.Validate();

        if (options.AuthenticationOptions.OfType<EntraIdAuthenticationOptions>().Any() && apiTokenProvider == null)
        {
            throw new ArgumentException("IApiTokenProvider must be provided when using Entra ID authentication", nameof(apiTokenProvider));
        }

        this.options = options;

        // Configure retry policy
        retryPolicy = CreateRetryPolicy(this.options.MaxRetryCount > 0 ? this.options.MaxRetryCount : 3);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApi{TOptions}"/> class using IOptions.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="apiTokenProvider">The optional API token provider (required for Entra ID authentication).</param>
    /// <param name="restClientService">The REST client service.</param>
    /// <param name="options">The strongly-typed API client options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public BaseApi(
        ILogger<BaseApi<TOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        IOptions<TOptions> options)
        : this(logger, apiTokenProvider, restClientService, options?.Value ??
            throw new ArgumentNullException(nameof(options)))
    {
    }



    /// <summary>
    /// Creates a retry policy with exponential backoff for handling transient failures.
    /// </summary>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <returns>A configured retry policy.</returns>
    private AsyncRetryPolicy<RestResponse> CreateRetryPolicy(int retryCount)
    {
        return Policy
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

                    return Task.CompletedTask;
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
        await ApplyAuthenticationAsync(request, cancellationToken).ConfigureAwait(false);

        return request;
    }

    /// <summary>
    /// Applies all configured authentication methods to the request.
    /// </summary>
    /// <param name="request">The request to authenticate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when authentication is not properly configured.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication token acquisition fails.</exception>
    private async Task ApplyAuthenticationAsync(RestRequest request, CancellationToken cancellationToken)
    {
        if (options.AuthenticationOptions == null || !options.AuthenticationOptions.Any())
        {
            logger.LogDebug("No authentication options configured");
            return;
        }

        foreach (var authOption in options.AuthenticationOptions)
        {
            switch (authOption)
            {
                case ApiKeyAuthenticationOptions apiKeyOptions:
                    ApplyApiKeyAuthentication(request, apiKeyOptions);
                    break;

                case EntraIdAuthenticationOptions entraIdOptions:
                    await ApplyEntraIdAuthenticationAsync(request, entraIdOptions, cancellationToken).ConfigureAwait(false);
                    break;

                default:
                    logger.LogWarning("Unsupported authentication type: {AuthenticationType}", authOption.GetType().Name);
                    break;
            }
        }
    }

    /// <summary>
    /// Applies API key authentication to the request.
    /// </summary>
    /// <param name="request">The request to authenticate.</param>
    /// <param name="options">The API key authentication options.</param>
    private void ApplyApiKeyAuthentication(RestRequest request, ApiKeyAuthenticationOptions options)
    {
        var apiKey = options.GetApiKeyAsString();
        if (!string.IsNullOrEmpty(apiKey))
        {
            if (options.Location == Configuration.ApiKeyLocation.QueryParameter)
            {
                request.AddQueryParameter(options.HeaderName, apiKey);
                logger.LogDebug("Added API key authentication as query parameter");
            }
            else
            {
                request.AddHeader(options.HeaderName, apiKey);
                logger.LogDebug("Added API key authentication as header");
            }
        }
        else
        {
            logger.LogWarning("API key authentication requested but no API key is configured");
        }
    }

    /// <summary>
    /// Applies Entra ID token authentication to the request.
    /// </summary>
    /// <param name="request">The request to authenticate.</param>
    /// <param name="options">The Entra ID authentication options.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when apiTokenProvider is not available.</exception>
    /// <exception cref="AuthenticationException">Thrown when token acquisition fails.</exception>
    private async Task ApplyEntraIdAuthenticationAsync(
        RestRequest request,
        EntraIdAuthenticationOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (apiTokenProvider == null)
            {
                throw new InvalidOperationException("IApiTokenProvider not available for Entra ID authentication");
            }

            string accessToken = await apiTokenProvider.GetAccessTokenAsync(options.ApiAudience, cancellationToken).ConfigureAwait(false);
            request.AddHeader(AuthorizationHeaderName, $"{BearerTokenPrefix}{accessToken}");
            logger.LogDebug("Added Entra ID token authentication for audience '{Audience}'", options.ApiAudience);
        }
        catch (AuthenticationException ex)
        {
            logger.LogError(ex, "Failed to get authentication token for audience '{Audience}'", options.ApiAudience);
            throw;
        }
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
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Execute the request with retry policy
            var response = await retryPolicy.ExecuteAsync(
                async (token) => await restClientService.ExecuteAsync(options.BaseUrl, request, token).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            // Ensure response is not null to prevent NullReferenceException
            if (response is null)
            {
                logger.LogError("Received null response for {Method} to '{Resource}'", request.Method, request.Resource);
                throw new HttpRequestException($"Failed {request.Method} to '{request.Resource}' - received null response");
            }

            return HandleResponse(response, request);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request to '{Resource}' was canceled", request.Resource);
            throw;
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException - already logged
            throw;
        }
        catch (InvalidOperationException)
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

    /// <summary>
    /// Handles the REST response and processes according to status code.
    /// </summary>
    /// <param name="response">The REST response to handle.</param>
    /// <param name="request">The original request for context in error messages.</param>
    /// <returns>The response if successful.</returns>
    /// <exception cref="HttpRequestException">Thrown when a non-successful status code is returned.</exception>
    private RestResponse HandleResponse(RestResponse response, RestRequest request)
    {
        if (SuccessStatusCodes.Contains(response.StatusCode))
        {
            return response;
        }
        else
        {
            logger.LogError("HTTP error {StatusCode} for {Method} to '{Resource}': {Content}",
                response.StatusCode, request.Method, request.Resource, response.Content);
            throw new HttpRequestException(
                $"Failed {request.Method} to '{request.Resource}' with status code {response.StatusCode}: {response.Content}");
        }
    }


}
