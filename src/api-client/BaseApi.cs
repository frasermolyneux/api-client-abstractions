using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

namespace MxIO.ApiClient
{
    public class BaseApi
    {
        private readonly ILogger logger;
        private readonly IApiTokenProvider apiTokenProvider;
        private readonly IRestClientSingleton restClientSingleton;

        private readonly string baseUrl;
        private readonly string primaryApiKey;
        private readonly string secondaryApiKey;
        private readonly string apiAudience;

        public BaseApi(ILogger logger, IApiTokenProvider apiTokenProvider, IRestClientSingleton restClientSingleton, IOptions<ApiClientOptions> options)
        {
            this.logger = logger;
            this.apiTokenProvider = apiTokenProvider;
            this.restClientSingleton = restClientSingleton;

            if (string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix))
            {
                baseUrl = options.Value.BaseUrl;
            }
            else
            {
                baseUrl = $"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}";
            }

            primaryApiKey = options.Value.PrimaryApiKey;
            secondaryApiKey = options.Value.SecondaryApiKey;
            apiAudience = options.Value.ApiAudience;
        }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await apiTokenProvider.GetAccessToken(apiAudience);

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", primaryApiKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request, bool useSecondaryApiKey = false)
        {
            if (useSecondaryApiKey && !string.IsNullOrWhiteSpace(secondaryApiKey))
            {
                logger.LogInformation($"Retrying with secondary API key for '{request.Resource}'");
                request.AddOrUpdateHeader("Ocp-Apim-Subscription-Key", secondaryApiKey);
            }

            var response = await restClientSingleton.ExecuteAsync(baseUrl, request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !useSecondaryApiKey)
            {
                var responseContent = response.Content;

                if (responseContent is not null && responseContent.Contains("Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription."))
                {
                    return await ExecuteAsync(request, true);
                }
            }

            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            else if (response.ErrorException is not null)
            {
                logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }
            else
            {
                var ex = new ApplicationException($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                logger.LogError(ex, $"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw ex;
            }
        }
    }
}