using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Retry;

using RestSharp;

namespace MxIO.ApiClient;

/// <summary>
/// Base class for API clients providing common functionality for REST API calls.
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
    private static readonly HttpStatusCode[] NoRetryStatusCodes = { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized };

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
        ILogger logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientSingleton restClientSingleton,
        IOptions<ApiClientOptions> options)
    {
        // Cast generic logger to typed logger as a best practice
        this.logger = logger as ILogger<BaseApi> ?? new NullLogger<BaseApi>();

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
        retryPolicy = Policy
            .HandleResult<RestResponse>(r =>
                r is null || !NoRetryStatusCodes.Contains(r.StatusCode))
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (response, timespan, retryCount, context) =>
                {
                    if (response?.Result is not null)
                    {
                        logger.LogWarning("Request failed with {StatusCode}. Waiting {Timespan} before next retry. Retry attempt {RetryCount}",
                            response.Result.StatusCode, timespan, retryCount);
                    }
                    else
                    {
                        logger.LogWarning("Request failed with null response. Waiting {Timespan} before next retry. Retry attempt {RetryCount}",
                            timespan, retryCount);
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
    public async Task<RestRequest> CreateRequest(string resource, Method method, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
        }

        var accessToken = await apiTokenProvider.GetAccessToken(apiAudience, cancellationToken);

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
    /// <exception cref="ApplicationException">Thrown when the request fails with an unexpected status code.</exception>
    public async Task<RestResponse> ExecuteAsync(RestRequest request, bool useSecondaryApiKey = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Apply secondary API key if requested and available
        if (useSecondaryApiKey && !string.IsNullOrWhiteSpace(secondaryApiKey))
        {
            logger.LogInformation("Retrying with secondary API key for '{Resource}'", request.Resource);
            request.AddOrUpdateHeader(SubscriptionKeyHeaderName, secondaryApiKey);
        }

        // Execute the request with retry policy
        var response = await retryPolicy.ExecuteAsync(
            async (token) => await restClientSingleton.ExecuteAsync(baseUrl, request, token),
            cancellationToken);

        // Ensure response is not null to prevent NullReferenceException
        if (response is null)
        {
            logger.LogError("Received null response for {Method} to '{Resource}'", request.Method, request.Resource);
            throw new ApplicationException($"Failed {request.Method} to '{request.Resource}' - received null response");
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
        // Handle exceptions in the response
        else if (response.ErrorException is not null)
        {
            logger.LogError(response.ErrorException, "Failed {Method} to '{Resource}' with code '{StatusCode}'",
                request.Method, request.Resource, response.StatusCode);
            throw response.ErrorException;
        }
        // Handle other error status codes
        else
        {
            var ex = new ApplicationException($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            logger.LogError(ex, "Failed {Method} to '{Resource}' with response status '{ResponseStatus}' and code '{StatusCode}' - Content: {Content}",
                request.Method, request.Resource, response.ResponseStatus, response.StatusCode, response.Content);
            throw ex;
        }
    }
}