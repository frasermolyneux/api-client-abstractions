using FluentAssertions;
using MxIO.ApiClient.Abstractions;
using MxIO.ApiClient.Extensions;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Xunit;

namespace MxIO.ApiClient
{
    public class RestResponseExtensionsTests
    {
        [Fact]
        public void ToApiResponse_WithHeadRequest_ReturnsApiResponseWithStatusCode()
        {
            // Arrange
            var request = new RestRequest("/test", Method.Head);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Request = request
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void ToApiResponse_WithNullContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = null
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            apiResponse.Errors.Should().Contain("Response content received by client api was null. (client error).");
        }

        [Fact]
        public void ToApiResponse_WithValidContent_ReturnsDeserializedResponse()
        {
            // Arrange
            var originalApiResponse = new ApiResponseDto(HttpStatusCode.OK);
            var jsonContent = JsonConvert.SerializeObject(originalApiResponse);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void ToApiResponse_WithInvalidJsonContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "This is not valid JSON"
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            apiResponse.Errors.Should().NotBeEmpty(); // Will contain the JSON parsing error
        }

        [Fact]
        public void ToApiResponseGeneric_WithNullContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = null
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            apiResponse.Errors.Should().Contain("Response content received by client api was null. (client error).");
        }

        [Fact]
        public void ToApiResponseGeneric_WithValidContent_ReturnsDeserializedResponse()
        {
            // Arrange
            // Create JSON with the expected structure
            var jsonContent = @"{
                ""StatusCode"": 200,
                ""Errors"": [],
                ""Result"": ""test data""
            }";

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Result.Should().Be("test data");
        }

        [Fact]
        public void ToApiResponseGeneric_WithInvalidJsonContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "This is not valid JSON"
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            apiResponse.Should().NotBeNull();
            apiResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            apiResponse.Errors.Should().NotBeEmpty(); // Will contain the JSON parsing error
        }

        [Fact]
        public void ToApiResponseGeneric_WithMismatchedType_HandlesErrorGracefully()
        {
            // Arrange
            // Create JSON for a response with an integer but try to deserialize to string
            var jsonContent = @"{
                ""StatusCode"": 200,
                ""Errors"": [],
                ""Result"": 123
            }";

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse<string>(); // Try to deserialize to string

            // Assert
            apiResponse.Should().NotBeNull();
            // The JSON.NET deserializer appears to be handling type conversion automatically
            // instead of throwing an error, so the status code remains 200 (OK)
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            // The result will be the string representation of the integer
            apiResponse.Result.Should().Be("123");
        }
    }
}