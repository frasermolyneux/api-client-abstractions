using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;
using MX.Api.IntegrationTests.Constants;
using MX.Api.IntegrationTests.DummyApis.UserApi.Models;
using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;
using MX.Api.Web.Extensions;

namespace MX.Api.IntegrationTests.DummyApis.UserApi.Controllers;

/// <summary>
/// User API controller for testing purposes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase, IUserApiV1
{
    private readonly ILogger<UsersController> _logger;
    private static readonly List<User> _users = new();
    private static int _nextId = 1;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;

        // Initialize with some test data if empty
        if (!_users.Any())
        {
            _users.AddRange(
            [
                new User { Id = _nextId++, Username = "john.doe", Email = "john.doe@example.com", FullName = "John Doe", Role = "Admin" },
                new User { Id = _nextId++, Username = "jane.smith", Email = "jane.smith@example.com", FullName = "Jane Smith", Role = "User" },
                new User { Id = _nextId++, Username = "bob.wilson", Email = "bob.wilson@example.com", FullName = "Bob Wilson", Role = "User" }
            ]);
        }
    }

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated users</returns>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await ((IUserApiV1)this).GetUsersAsync(page, pageSize);
        return response.ToHttpResult();
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var response = await ((IUserApiV1)this).GetUserByIdAsync(id);
        return response.ToHttpResult();
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>Created user</returns>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Convert the request to a User object for the interface
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role
        };

        var response = await ((IUserApiV1)this).CreateUserAsync(user);
        return response.ToHttpResult();
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("~/api/users/health")]
    public ActionResult<ApiResponse<string>> HealthCheck()
    {
        return Ok(new ApiResponse<string>("User API is healthy"));
    }

    // Interface implementation methods

    /// <summary>
    /// Gets all users with pagination implementation
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated users</returns>
    Task<ApiResult<CollectionModel<User>>> IUserApiV1.GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        // Bearer token validation
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(new ApiResponse<CollectionModel<User>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ") || authHeader != "Bearer user-test-token")
        {
            return Task.FromResult(new ApiResponse<CollectionModel<User>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Getting users page {Page} with size {PageSize}", page, pageSize);

        var totalCount = _users.Count;
        var users = _users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var collection = new CollectionModel<User>
        {
            Items = users
        };

        var response = new ApiResponse<CollectionModel<User>>(collection)
        {
            Pagination = new ApiPagination
            {
                TotalCount = totalCount,
                FilteredCount = totalCount, // For simplicity, no filtering in this demo
                Skip = (page - 1) * pageSize,
                Top = users.Count,
                HasMore = totalCount > page * pageSize
            }
        };

        return Task.FromResult(response.ToApiResult());
    }

    /// <summary>
    /// Gets a user by ID implementation
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User</returns>
    Task<ApiResult<User>> IUserApiV1.GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        // Bearer token validation
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ") || authHeader != "Bearer user-test-token")
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Getting user {UserId}", userId);

        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.NotFound,
                    string.Format(ApiErrorConstants.ErrorMessages.ResourceNotFound, "User", userId),
                    string.Format(ApiErrorConstants.ErrorDetails.ResourceDoesNotExist, "user"))
            ).ToApiResult(System.Net.HttpStatusCode.NotFound));
        }

        return Task.FromResult(new ApiResponse<User>(user).ToApiResult());
    }

    /// <summary>
    /// Creates a new user implementation
    /// </summary>
    /// <param name="user">User to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    Task<ApiResult<User>> IUserApiV1.CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        // Bearer token validation
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ") || authHeader != "Bearer user-test-token")
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Creating user {Username}", user.Username);

        // Check if username already exists
        if (_users.Any(u => u.Username == user.Username))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.ResourceExists,
                    string.Format(ApiErrorConstants.ErrorMessages.ResourceAlreadyExists, "User", "username", user.Username),
                    string.Format(ApiErrorConstants.ErrorDetails.ResourceAlreadyExistsDetail, "user", "username"))
            ).ToApiResult(System.Net.HttpStatusCode.BadRequest));
        }

        // Set the ID and creation timestamp
        user.Id = _nextId++;
        user.CreatedAt = DateTime.UtcNow;

        _users.Add(user);

        return Task.FromResult(new ApiResponse<User>(user).ToApiResult(System.Net.HttpStatusCode.Created));
    }

    /// <summary>
    /// Deletes a user implementation
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    Task<ApiResult<string>> IUserApiV1.DeleteUserAsync(int userId, CancellationToken cancellationToken)
    {
        // Bearer token validation
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ") || authHeader != "Bearer user-test-token")
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Deleting user {UserId}", userId);

        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.NotFound,
                    string.Format(ApiErrorConstants.ErrorMessages.ResourceNotFound, "User", userId),
                    string.Format(ApiErrorConstants.ErrorDetails.ResourceDoesNotExist, "user"))
            ).ToApiResult(System.Net.HttpStatusCode.NotFound));
        }

        _users.Remove(user);

        return Task.FromResult(new ApiResponse<string>($"User {userId} deleted successfully").ToApiResult());
    }
}
