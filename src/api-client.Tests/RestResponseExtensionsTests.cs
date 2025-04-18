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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("Response content received by client api was null. (client error).", apiResponse.Errors);
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.NotEmpty(apiResponse.Errors); // Will contain the JSON parsing error
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("Response content received by client api was null. (client error).", apiResponse.Errors);
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
            Assert.Equal("test data", apiResponse.Result);
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
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.NotEmpty(apiResponse.Errors); // Will contain the JSON parsing error
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
            Assert.NotNull(apiResponse);
            // The JSON.NET deserializer appears to be handling type conversion automatically
            // instead of throwing an error, so the status code remains 200 (OK)
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
            // The result will be the string representation of the integer
            Assert.Equal("123", apiResponse.Result);
        }

        [Fact]
        public void ToApiResponse_WithEmptyContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = ""
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("Response content received by client api was null. (client error).", apiResponse.Errors);
        }

        [Fact]
        public void ToApiResponseGeneric_WithEmptyContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = ""
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("Response content received by client api was null. (client error).", apiResponse.Errors);
        }

        [Fact]
        public void ToApiResponse_WithErrorStatusCode_PreservesStatusCode()
        {
            // Arrange
            var originalApiResponse = new ApiResponseDto(HttpStatusCode.BadRequest, "Error message");
            var jsonContent = JsonConvert.SerializeObject(originalApiResponse);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
            Assert.Contains("Error message", apiResponse.Errors);
        }

        [Fact]
        public void ToApiResponseGeneric_WithComplexResult_DeserializesCorrectly()
        {
            // Arrange
            var testObject = new TestClass { Id = 123, Name = "Test Name" };
            var originalApiResponse = new ApiResponseDto<TestClass>(HttpStatusCode.OK, testObject);
            var jsonContent = JsonConvert.SerializeObject(originalApiResponse);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse<TestClass>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Result);
            Assert.Equal(123, apiResponse.Result.Id);
            Assert.Equal("Test Name", apiResponse.Result.Name);
        }

        [Fact]
        public void ToApiResponseGeneric_WithNullResult_ReturnsResponseWithNullResult()
        {
            // Arrange
            var jsonContent = @"{
                ""StatusCode"": 200,
                ""Errors"": [],
                ""Result"": null
            }";

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
            Assert.Null(apiResponse.Result);
            Assert.Empty(apiResponse.Errors);
        }

        [Fact]
        public void ToApiResponse_WithNullResponse_ThrowsArgumentNullException()
        {
            // Arrange
            RestResponse? response = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => response!.ToApiResponse());
        }

        [Fact]
        public void ToApiResponseGeneric_WithNullResponse_ThrowsArgumentNullException()
        {
            // Arrange
            RestResponse? response = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => response!.ToApiResponse<string>());
        }

        [Fact]
        public void ToApiResponse_WithDefaultStatusCodeInContent_UsesResponseStatusCode()
        {
            // Arrange
            var jsonContent = @"{
                ""StatusCode"": 0,
                ""Errors"": [""Some error""]
            }";

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
            Assert.Contains("Some error", apiResponse.Errors);
        }

        [Fact]
        public void ToApiResponseGeneric_WithDefaultStatusCodeInContent_UsesResponseStatusCode()
        {
            // Arrange
            var jsonContent = @"{
                ""StatusCode"": 0,
                ""Errors"": [""Some error""],
                ""Result"": ""test data""
            }";

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = jsonContent
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
            Assert.Contains("Some error", apiResponse.Errors);
            Assert.Equal("test data", apiResponse.Result);
        }

        [Fact]
        public void ToApiResponse_WithInvalidContentAndException_HandlesGracefully()
        {
            // Arrange - create a response that will cause an exception during processing
            var invalidJsonWithException = "{\"invalidJson\": true"; // Missing closing brace

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = invalidJsonWithException
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("JSON deserialization error", apiResponse.Errors[0]);
        }

        [Fact]
        public void ToApiResponseGeneric_WithInvalidContentAndException_HandlesGracefully()
        {
            // Arrange - create a response that will cause an exception during processing
            var invalidJsonWithException = "{\"invalidJson\": true"; // Missing closing brace

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = invalidJsonWithException
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("JSON deserialization error", apiResponse.Errors[0]);
        }

        [Fact]
        public void ToApiResponse_WithDeserializationReturningNull_ReturnsErrorResponse()
        {
            // Arrange - Create a response with content that won't deserialize to ApiResponseDto
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "[]" // Array, not an object
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("JSON deserialization error", apiResponse.Errors[0]);
        }

        [Fact]
        public void ToApiResponseGeneric_WithDeserializationReturningNull_ReturnsErrorResponse()
        {
            // Arrange - Create a response with content that won't deserialize to ApiResponseDto<T>
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "[]" // Array, not an object
            };

            // Act
            var apiResponse = response.ToApiResponse<string>();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("JSON deserialization error", apiResponse.Errors[0]);
        }

        [Fact]
        public void ToApiResponse_WithWhitespaceContent_ReturnsErrorResponse()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "   " // Whitespace, not empty
            };

            // Act
            var apiResponse = response.ToApiResponse();

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, apiResponse.StatusCode);
            Assert.Contains("Response content received by client api was null. (client error).", apiResponse.Errors);
        }

        // Helper class for testing complex object deserialization
        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}