using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Retry;

using RestSharp;

namespace MxIO.ApiClient
{
    /// <summary>
    /// Base class for API clients providing common functionality for REST API calls.
    /// </summary>
    public class BaseApi
    {
        private readonly ILogger logger;
        private readonly IApiTokenProvider apiTokenProvider;
        private readonly IRestClientSingleton restClientSingleton;

        private readonly string baseUrl;
        private readonly string primaryApiKey;
        private readonly string secondaryApiKey;
        private readonly string apiAudience;

        private readonly AsyncRetryPolicy<RestResponse> retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApi"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="apiTokenProvider">The API token provider.</param>
        /// <param name="restClientSingleton">The REST client singleton.</param>
        /// <param name="options">The API client options.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
        public BaseApi(ILogger logger, IApiTokenProvider apiTokenProvider, IRestClientSingleton restClientSingleton, IOptions<ApiClientOptions> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.apiTokenProvider = apiTokenProvider ?? throw new ArgumentNullException(nameof(apiTokenProvider));
            this.restClientSingleton = restClientSingleton ?? throw new ArgumentNullException(nameof(restClientSingleton));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Value.BaseUrl))
                throw new ArgumentException("BaseUrl must be specified in ApiClientOptions", nameof(options));

            if (string.IsNullOrEmpty(options.Value.PrimaryApiKey))
                throw new ArgumentException("PrimaryApiKey must be specified in ApiClientOptions", nameof(options));

            if (string.IsNullOrEmpty(options.Value.ApiAudience))
                throw new ArgumentException("ApiAudience must be specified in ApiClientOptions", nameof(options));

            // Construct the base URL with optional path prefix
            baseUrl = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
                ? options.Value.BaseUrl
                : $"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}";

            primaryApiKey = options.Value.PrimaryApiKey;
            secondaryApiKey = options.Value.SecondaryApiKey;
            apiAudience = options.Value.ApiAudience;

            // Configure retry policy - fix to handle null responses safely
            retryPolicy = Policy
                .HandleResult<RestResponse>(r =>
                {
                    // First check for null to avoid null reference exception
                    if (r == null)
                        return true;

                    return !new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized }.Contains(r.StatusCode);
                })
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (response, timespan, retryCount, context) =>
                    {
                        if (response?.Result != null)
                        {
                            logger.LogWarning($"Request failed with {response.Result.StatusCode}. Waiting {timespan} before next retry. Retry attempt {retryCount}");
                        }
                        else
                        {
                            logger.LogWarning($"Request failed with null response. Waiting {timespan} before next retry. Retry attempt {retryCount}");
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
        public async Task<RestRequest> CreateRequest(string resource, Method method, CancellationToken cancellationToken = default)
        {
            var accessToken = await apiTokenProvider.GetAccessToken(apiAudience, cancellationToken);

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", primaryApiKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        /// <summary>
        /// Executes the REST request and handles errors and retries.
        /// </summary>
        /// <param name="request">The REST request to execute.</param>
        /// <param name="useSecondaryApiKey">Whether to use the secondary API key.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The REST response.</returns>
        /// <exception cref="ApplicationException">Thrown when the request fails with an unexpected status code.</exception>
        public async Task<RestResponse> ExecuteAsync(RestRequest request, bool useSecondaryApiKey = false, CancellationToken cancellationToken = default)
        {
            // Apply secondary API key if requested and available
            if (useSecondaryApiKey && !string.IsNullOrWhiteSpace(secondaryApiKey))
            {
                logger.LogInformation($"Retrying with secondary API key for '{request.Resource}'");
                request.AddOrUpdateHeader("Ocp-Apim-Subscription-Key", secondaryApiKey);
            }

            // Execute the request with retry policy
            var response = await retryPolicy.ExecuteAsync(
                async (token) => await restClientSingleton.ExecuteAsync(baseUrl, request, token),
                cancellationToken);

            // Ensure response is not null to prevent NullReferenceException
            if (response == null)
            {
                logger.LogError($"Received null response for {request.Method} to '{request.Resource}'");
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
            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            // Handle exceptions in the response
            else if (response.ErrorException is not null)
            {
                logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }
            // Handle other error status codes
            else
            {
                var ex = new ApplicationException($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                logger.LogError(ex, $"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw ex;
            }
        }
    }
}