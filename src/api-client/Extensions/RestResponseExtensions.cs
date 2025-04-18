using System.Net;

using MxIO.ApiClient.Abstractions;

using Newtonsoft.Json;

using RestSharp;

namespace MxIO.ApiClient.Extensions
{
    /// <summary>
    /// Extension methods for working with RestSharp response objects.
    /// </summary>
    public static class RestResponseExtensions
    {
        /// <summary>
        /// Converts a RestResponse to an ApiResponseDto.
        /// </summary>
        /// <param name="response">The RestSharp response to convert.</param>
        /// <returns>A strongly-typed API response DTO object.</returns>
        public static ApiResponseDto ToApiResponse(this RestResponse response)
        {
            // Special handling for HEAD requests which don't return content
            if (response.Request?.Method == Method.Head)
                return new ApiResponseDto(response.StatusCode);

            if (response.Content == null)
            {
                var nullContentResponse = new ApiResponseDto(HttpStatusCode.InternalServerError);
                nullContentResponse.Errors.Add("Response content received by client api was null. (client error).");
                return nullContentResponse;
            }

            try
            {
                var apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);
                if (apiResponseDto == null)
                {
                    var deserializationErrorResponse = new ApiResponseDto(HttpStatusCode.InternalServerError);
                    deserializationErrorResponse.Errors.Add("Response received by client api could not be transformed into API response. (client error).");
                    return deserializationErrorResponse;
                }

                return apiResponseDto;
            }
            catch (Exception ex)
            {
                var exceptionResponse = new ApiResponseDto(HttpStatusCode.InternalServerError);
                exceptionResponse.Errors.Add(ex.Message);
                return exceptionResponse;
            }
        }

        /// <summary>
        /// Converts a RestResponse to a generic ApiResponseDto with a strongly-typed result.
        /// </summary>
        /// <typeparam name="T">The type of the result expected in the response.</typeparam>
        /// <param name="response">The RestSharp response to convert.</param>
        /// <returns>A strongly-typed API response DTO object with result of type T.</returns>
        public static ApiResponseDto<T> ToApiResponse<T>(this RestResponse response)
        {
            if (response.Content == null)
            {
                var nullContentResponse = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                nullContentResponse.Errors.Add("Response content received by client api was null. (client error).");
                return nullContentResponse;
            }

            try
            {
                var apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto<T>>(response.Content);
                if (apiResponseDto == null)
                {
                    var deserializationErrorResponse = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                    deserializationErrorResponse.Errors.Add("Response received by client api could not be transformed into API response. (client error).");
                    return deserializationErrorResponse;
                }

                return apiResponseDto;
            }
            catch (Exception ex)
            {
                var exceptionResponse = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                exceptionResponse.Errors.Add(ex.Message);
                return exceptionResponse;
            }
        }
    }
}
