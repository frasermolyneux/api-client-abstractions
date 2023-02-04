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
        private readonly string apimSubscriptionKey;
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

            apimSubscriptionKey = options.Value.ApiKey;
            apiAudience = options.Value.ApiAudience;
        }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await apiTokenProvider.GetAccessToken(apiAudience);

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", apimSubscriptionKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var response = await restClientSingleton.ExecuteAsync(baseUrl, request);

            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            else if (response.ErrorException != null)
            {
                logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }
            else
            {
                logger.LogError($"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            }
        }
    }
}