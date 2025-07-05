using MX.Api.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RestSharp;

namespace MX.Api.Client.Extensions;

/// <summary>
/// Extension methods for working with RestSharp response objects.
/// </summary>
public static class RestResponseExtensions
{
    private const string NullContentError = "Response content received by client api was null. (client error).";
    private const string DeserializationError = "Response received by client api could not be transformed into API response. (client error).";

    private static readonly JsonSerializerSettings DefaultSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>
    /// Converts a RestResponse to an ApiResult containing an ApiResponse.
    /// </summary>
    /// <param name="response">The RestSharp response to convert.</param>
    /// <returns>An API result wrapper object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null.</exception>
    public static ApiResult ToApiResult(this RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        // Create API result wrapper with the status code
        var httpResponse = new ApiResult(response.StatusCode);

        // Special handling for HEAD requests which don't return content
        if (response.Request?.Method == Method.Head)
        {
            return httpResponse;
        }

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            var apiResponse = new ApiResponse();
            apiResponse.Errors = new[] { new ApiError("NullContent", NullContentError) };
            httpResponse.Result = apiResponse;
            return httpResponse;
        }
        try
        {
            // Try to deserialize directly to ApiResponse
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content, DefaultSerializerSettings);

            if (apiResponse is null)
            {
                // Try to deserialize as a dynamic object to see if it's valid JSON but not ApiResponse format
                var dynamicObj = JsonConvert.DeserializeObject(response.Content);
                if (dynamicObj != null)
                {
                    // Valid JSON but not in ApiResponse format
                    var errorResponse = new ApiResponse();
                    errorResponse.Errors = new[] { new ApiError("DeserializationError", DeserializationError) };
                    httpResponse.Result = errorResponse;
                    return httpResponse;
                }

                // If both attempts failed, create a generic error response
                var nullResponse = new ApiResponse();
                nullResponse.Errors = new[] { new ApiError("DeserializationError", DeserializationError) };
                httpResponse.Result = nullResponse;
                return httpResponse;
            }

            httpResponse.Result = apiResponse;
            return httpResponse;
        }
        catch (JsonException ex)
        {
            var errorResponse = new ApiResponse();
            errorResponse.Errors = new[] { new ApiError("JsonError", $"JSON deserialization error: {ex.Message}") };
            httpResponse.Result = errorResponse;
            return httpResponse;
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse();
            errorResponse.Errors = new[] { new ApiError("UnexpectedError", $"Unexpected error during response processing: {ex.Message}") };
            httpResponse.Result = errorResponse;
            return httpResponse;
        }
    }

    /// <summary>
    /// Converts a RestResponse to an ApiResult containing a strongly-typed ApiResponse.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in the response.</typeparam>
    /// <param name="response">The RestSharp response to convert.</param>
    /// <returns>An API result wrapper object with a strongly-typed API response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null.</exception>
    public static ApiResult<T> ToApiResult<T>(this RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        // Create API result wrapper with the status code
        var httpResponse = new ApiResult<T>(response.StatusCode);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            var apiResponse = new ApiResponse<T>();
            apiResponse.Errors = new[] { new ApiError("NullContent", NullContentError) };
            httpResponse.Result = apiResponse;
            return httpResponse;
        }
        try
        {
            // Try to deserialize directly to ApiResponse<T>
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content, DefaultSerializerSettings);

            if (apiResponse is null)
            {
                // Try to deserialize as a dynamic object to see if it's valid JSON but not ApiResponse format
                var dynamicObj = JsonConvert.DeserializeObject(response.Content);
                if (dynamicObj != null)
                {
                    // Valid JSON but not in ApiResponse format
                    var errorResponse = new ApiResponse<T>();
                    errorResponse.Errors = new[] { new ApiError("DeserializationError", DeserializationError) };
                    httpResponse.Result = errorResponse;
                    return httpResponse;
                }

                // If both attempts failed, create a generic error response
                var nullResponse = new ApiResponse<T>();
                nullResponse.Errors = new[] { new ApiError("DeserializationError", DeserializationError) };
                httpResponse.Result = nullResponse;
                return httpResponse;
            }

            httpResponse.Result = apiResponse;
            return httpResponse;
        }
        catch (JsonException ex)
        {
            var errorResponse = new ApiResponse<T>();
            errorResponse.Errors = new[] { new ApiError("JsonError", $"JSON deserialization error: {ex.Message}") };
            httpResponse.Result = errorResponse;
            return httpResponse;
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<T>();
            errorResponse.Errors = new[] { new ApiError("UnexpectedError", $"Unexpected error during response processing: {ex.Message}") };
            httpResponse.Result = errorResponse;
            return httpResponse;
        }
    }
}
