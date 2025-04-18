using System.Net;

using MxIO.ApiClient.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RestSharp;

namespace MxIO.ApiClient.Extensions;

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
    /// Converts a RestResponse to an ApiResponseDto.
    /// </summary>
    /// <param name="response">The RestSharp response to convert.</param>
    /// <returns>A strongly-typed API response DTO object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null.</exception>
    public static ApiResponseDto ToApiResponse(this RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        // Special handling for HEAD requests which don't return content
        if (response.Request?.Method == Method.Head)
        {
            return new ApiResponseDto(response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            var nullContentResponse = new ApiResponseDto(HttpStatusCode.InternalServerError);
            nullContentResponse.Errors.Add(NullContentError);
            return nullContentResponse;
        }

        try
        {
            var apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content, DefaultSerializerSettings);

            if (apiResponseDto is null)
            {
                var deserializationErrorResponse = new ApiResponseDto(HttpStatusCode.InternalServerError);
                deserializationErrorResponse.Errors.Add(DeserializationError);
                return deserializationErrorResponse;
            }

            // If the status code in the DTO is the default (0), create a new response with the proper status code
            if (apiResponseDto.StatusCode == default)
            {
                var newResponse = new ApiResponseDto(response.StatusCode);
                foreach (var error in apiResponseDto.Errors)
                {
                    newResponse.Errors.Add(error);
                }
                return newResponse;
            }

            return apiResponseDto;
        }
        catch (JsonException ex)
        {
            return CreateErrorResponse(HttpStatusCode.InternalServerError, $"JSON deserialization error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(HttpStatusCode.InternalServerError, $"Unexpected error during response processing: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts a RestResponse to a generic ApiResponseDto with a strongly-typed result.
    /// </summary>
    /// <typeparam name="T">The type of the result expected in the response.</typeparam>
    /// <param name="response">The RestSharp response to convert.</param>
    /// <returns>A strongly-typed API response DTO object with result of type T.</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null.</exception>
    public static ApiResponseDto<T> ToApiResponse<T>(this RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return CreateErrorResponse<T>(HttpStatusCode.InternalServerError, NullContentError);
        }

        try
        {
            var apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto<T>>(response.Content, DefaultSerializerSettings);

            if (apiResponseDto is null)
            {
                return CreateErrorResponse<T>(HttpStatusCode.InternalServerError, DeserializationError);
            }

            // If the status code in the DTO is the default (0), create a new response with the proper status code
            if (apiResponseDto.StatusCode == default)
            {
                var newResponse = new ApiResponseDto<T>(response.StatusCode, apiResponseDto.Result);
                foreach (var error in apiResponseDto.Errors)
                {
                    newResponse.Errors.Add(error);
                }
                return newResponse;
            }

            return apiResponseDto;
        }
        catch (JsonException ex)
        {
            return CreateErrorResponse<T>(HttpStatusCode.InternalServerError, $"JSON deserialization error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>(HttpStatusCode.InternalServerError, $"Unexpected error during response processing: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates an error response with a specific status code and error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <param name="errorMessage">The error message to include.</param>
    /// <returns>A configured ApiResponseDto with the error.</returns>
    private static ApiResponseDto CreateErrorResponse(HttpStatusCode statusCode, string errorMessage)
    {
        var response = new ApiResponseDto(statusCode);
        response.Errors.Add(errorMessage);
        return response;
    }

    /// <summary>
    /// Creates an error response with a specific status code and error message.
    /// </summary>
    /// <typeparam name="T">The type of the result in the response.</typeparam>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <param name="errorMessage">The error message to include.</param>
    /// <returns>A configured ApiResponseDto with the error.</returns>
    private static ApiResponseDto<T> CreateErrorResponse<T>(HttpStatusCode statusCode, string errorMessage)
    {
        var response = new ApiResponseDto<T>(statusCode);
        response.Errors.Add(errorMessage);
        return response;
    }
}
