using Microsoft.AspNetCore.Mvc;

using MxIO.ApiClient.Abstractions;

namespace MX.GeoLocation.LookupWebApi.Extensions
{
    public static class ApiResponseDtoExtensions
    {
        public static IActionResult ToHttpResult<T>(this ApiResponseDto<T> apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }

        public static IActionResult ToHttpResult(this ApiResponseDto apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }
    }
}
