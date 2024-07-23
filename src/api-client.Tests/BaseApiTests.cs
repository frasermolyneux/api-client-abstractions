using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using FluentAssertions;
using System.Net;

namespace MxIO.ApiClient
{
    [TestFixture]
    public class BaseApiTests
    {
        private ILogger logger;
        private IApiTokenProvider apiTokenProvider;
        private IRestClientSingleton restClientSingleton;
        private IOptions<ApiClientOptions> options;
        private BaseApi baseApi;

        [SetUp]
        public void SetUp()
        {
            logger = A.Fake<ILogger>();
            apiTokenProvider = A.Fake<IApiTokenProvider>();
            restClientSingleton = A.Fake<IRestClientSingleton>();
            options = A.Fake<IOptions<ApiClientOptions>>();

            A.CallTo(() => apiTokenProvider.GetAccessToken(A<string>.Ignored)).Returns(Task.FromResult("fake_access_token"));
            A.CallTo(() => options.Value).Returns(new ApiClientOptions
            {
                BaseUrl = "https://api.example.com",
                ApiPathPrefix = "v1",
                PrimaryApiKey = "primary_key",
                SecondaryApiKey = "secondary_key",
                ApiAudience = "api_audience"
            });

            baseApi = new BaseApi(logger, apiTokenProvider, restClientSingleton, options);
        }

        [Test]
        public async Task CreateRequest_ShouldCorrectlyConstructRestRequest()
        {
            // Arrange
            var resource = "test/resource";
            var method = Method.Get;

            // Act
            var request = await baseApi.CreateRequest(resource, method);

            // Assert
            request.Resource.Should().Be(resource);
            request.Method.Should().Be(method);
            request.Parameters.Should().ContainSingle(p => p.Name == "Ocp-Apim-Subscription-Key" && p.Value != null && (string)p.Value == "primary_key");
            request.Parameters.Should().ContainSingle(p => p.Name == "Authorization" && p.Value != null && (string)p.Value == "Bearer fake_access_token");
        }

        [Test]
        public async Task ExecuteAsync_WithValidRequest_ReturnsOkResponse()
        {
            // Arrange
            var request = new RestRequest("/test", Method.Get);
            var expectedResponse = new RestResponse { StatusCode = HttpStatusCode.OK };

            A.CallTo(() => restClientSingleton.ExecuteAsync(A<string>.Ignored, A<RestRequest>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var response = await baseApi.ExecuteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task ExecuteAsync_WithUnauthorizedResponseAndSecondaryKey_RetriesAndReturnsOk()
        {
            // Arrange
            var request = new RestRequest("/test", Method.Get);
            var unauthorizedResponse = new RestResponse { StatusCode = HttpStatusCode.Unauthorized, Content = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription." };
            var okResponse = new RestResponse { StatusCode = HttpStatusCode.OK };

            A.CallTo(() => restClientSingleton.ExecuteAsync(A<string>.Ignored, A<RestRequest>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsNextFromSequence(Task.FromResult(unauthorizedResponse), Task.FromResult(okResponse));

            // Act
            var response = await baseApi.ExecuteAsync(request);

            // Assert
            A.CallTo(() => restClientSingleton.ExecuteAsync(A<string>.Ignored, request, A<CancellationToken>.Ignored))
                .MustHaveHappenedTwiceExactly();

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}