using Xunit;

namespace MxIO.ApiClient
{
    public class ApiClientOptionsTests
    {
        [Fact]
        public void ApiClientOptions_ShouldHaveDefaultConstructor()
        {
            // Arrange & Act
            var options = new ApiClientOptions();

            // Assert
            Assert.NotNull(options);
        }

        [Fact]
        public void ApiClientOptions_ShouldSetAndGetProperties()
        {
            // Arrange
            var options = new ApiClientOptions();

            // Act
            options.BaseUrl = "https://api.example.com";
            options.ApiPathPrefix = "v1";
            options.PrimaryApiKey = "primary-key";
            options.SecondaryApiKey = "secondary-key";
            options.ApiAudience = "test-audience";

            // Assert
            Assert.Equal("https://api.example.com", options.BaseUrl);
            Assert.Equal("v1", options.ApiPathPrefix);
            Assert.Equal("primary-key", options.PrimaryApiKey);
            Assert.Equal("secondary-key", options.SecondaryApiKey);
            Assert.Equal("test-audience", options.ApiAudience);
        }
    }
}