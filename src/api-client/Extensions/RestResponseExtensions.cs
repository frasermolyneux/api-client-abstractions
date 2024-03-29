﻿using System.Net;

using MxIO.ApiClient.Abstractions;

using Newtonsoft.Json;

using RestSharp;

namespace MxIO.ApiClient.Extensions
{
    public static class RestResponseExtensions
    {
        public static ApiResponseDto ToApiResponse(this RestResponse response)
        {
            ApiResponseDto? apiResponseDto;

            if (response.Request?.Method == Method.Head)
                return new ApiResponseDto(response.StatusCode);

            if (response.Content == null)
            {
                apiResponseDto = new ApiResponseDto(HttpStatusCode.InternalServerError);
                apiResponseDto.Errors.Add("Response content received by client api was null. (client error).");
                return apiResponseDto;
            }
            else
            {
                try
                {
                    apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);
                }
                catch (Exception ex)
                {
                    apiResponseDto = new ApiResponseDto(HttpStatusCode.InternalServerError);
                    apiResponseDto.Errors.Add(ex.Message);
                }
            }

            if (apiResponseDto == null)
            {
                apiResponseDto = new ApiResponseDto(HttpStatusCode.InternalServerError);
                apiResponseDto.Errors.Add("Response received by client api could not be transformed into API response. (client error).");
            }

            return apiResponseDto;
        }

        public static ApiResponseDto<T> ToApiResponse<T>(this RestResponse response)
        {
            ApiResponseDto<T>? apiResponseDto;

            if (response.Content == null)
            {
                apiResponseDto = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                apiResponseDto.Errors.Add("Response content received by client api was null. (client error).");
                return apiResponseDto;
            }
            else
            {
                try
                {
                    apiResponseDto = JsonConvert.DeserializeObject<ApiResponseDto<T>>(response.Content);
                }
                catch (Exception ex)
                {
                    apiResponseDto = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                    apiResponseDto.Errors.Add(ex.Message);
                }
            }

            if (apiResponseDto == null)
            {
                apiResponseDto = new ApiResponseDto<T>(HttpStatusCode.InternalServerError);
                apiResponseDto.Errors.Add("Response received by client api could not be transformed into API response. (client error).");
            }

            return apiResponseDto;
        }
    }
}
