using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

namespace MX.ApiClient
{
    public class BaseApi
    {
        private readonly ILogger logger;
        private readonly IApiTokenProvider apiTokenProvider;

        private readonly RestClient restClient;

        private readonly string apimSubscriptionKey;

        public BaseApi(ILogger logger, IApiTokenProvider apiTokenProvider, IOptions<ApiClientOptions> options)
        {
            this.logger = logger;
            this.apiTokenProvider = apiTokenProvider;

            restClient = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
                ? new RestClient($"{options.Value.BaseUrl}")
                : new RestClient($"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}");

            apimSubscriptionKey = options.Value.ApiKey;
        }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await apiTokenProvider.GetAccessToken();

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", apimSubscriptionKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var response = await restClient.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }

            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            else
            {
                logger.LogError($"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            }
        }
    }
}