using Microsoft.AspNetCore.Mvc;

using MxIO.ApiClient.Abstractions;

namespace MxIO.ApiClient.WebExtensions
{
    /// <summary>
    /// Extension methods for transforming API response DTOs into ASP.NET Core action results.
    /// </summary>
    public static class ApiResponseDtoExtensions
    {
        /// <summary>
        /// Converts a generic API response DTO to an HTTP action result with the appropriate status code.
        /// </summary>
        /// <typeparam name="T">The type of the result in the API response.</typeparam>
        /// <param name="apiResponseDto">The API response to convert.</param>
        /// <returns>An IActionResult with the status code from the API response.</returns>
        public static IActionResult ToHttpResult<T>(this ApiResponseDto<T> apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }

        /// <summary>
        /// Converts a base API response DTO to an HTTP action result with the appropriate status code.
        /// </summary>
        /// <param name="apiResponseDto">The API response to convert.</param>
        /// <returns>An IActionResult with the status code from the API response.</returns>
        public static IActionResult ToHttpResult(this ApiResponseDto apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }
    }
}
