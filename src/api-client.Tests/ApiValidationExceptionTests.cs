using System.Net;
using Xunit;

namespace MxIO.ApiClient
{
    public class ApiValidationExceptionTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Validation error occurred";
            var resource = "test/resource";
            var method = "GET";
            var statusCode = HttpStatusCode.BadRequest;
            var responseContent = "{\"errors\": {\"field1\": [\"Error 1\", \"Error 2\"]}}";
            var validationErrors = new Dictionary<string, IEnumerable<string>>
            {
                { "field1", new[] { "Error 1", "Error 2" } }
            };
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new ApiValidationException(
                message,
                resource,
                method,
                validationErrors,
                statusCode,
                responseContent,
                innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(resource, exception.Resource);
            Assert.Equal(method, exception.Method);
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Equal(responseContent, exception.ResponseContent);
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal(validationErrors.Count, exception.ValidationErrors.Count);
            Assert.Equal(validationErrors["field1"], exception.ValidationErrors["field1"]);
        }

        [Fact]
        public void GetAllValidationMessages_ReturnsFormattedMessages()
        {
            // Arrange
            var validationErrors = new Dictionary<string, IEnumerable<string>>
            {
                { "field1", new[] { "Error 1", "Error 2" } },
                { "field2", new[] { "Error 3" } }
            };

            var exception = new ApiValidationException(
                "Validation error occurred",
                "test/resource",
                "GET",
                validationErrors);

            // Act
            var messages = exception.GetAllValidationMessages();

            // Assert
            Assert.Equal(3, messages.Length);
            Assert.Contains("field1: Error 1", messages);
            Assert.Contains("field1: Error 2", messages);
            Assert.Contains("field2: Error 3", messages);
        }

        [Fact]
        public void ApiValidationException_InheritsFromApiException()
        {
            // Arrange & Act
            var exception = new ApiValidationException(
                "Validation error occurred",
                "test/resource",
                "GET",
                new Dictionary<string, IEnumerable<string>>());

            // Assert
            Assert.IsType<ApiValidationException>(exception);
            Assert.IsAssignableFrom<ApiException>(exception);
        }
    }
}