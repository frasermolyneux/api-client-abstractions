using System.Globalization;
using System.Text;
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
    private static readonly Action<ILogger, int, int, Exception?> LogGettingUsersPage =
        LoggerMessage.Define<int, int>(
            LogLevel.Information,
            new EventId(1, nameof(LogGettingUsersPage)),
            "Getting users page {Page} with size {PageSize}");

    private static readonly Action<ILogger, int, Exception?> LogGettingUser =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(2, nameof(LogGettingUser)),
            "Getting user {UserId}");

    private static readonly Action<ILogger, string?, Exception?> LogCreatingUser =
        LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(3, nameof(LogCreatingUser)),
            "Creating user {Username}");

    private static readonly Action<ILogger, int, Exception?> LogDeletingUser =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(LogDeletingUser)),
            "Deleting user {UserId}");

    private static readonly CompositeFormat ResourceNotFoundMessageFormat = CompositeFormat.Parse(ApiErrorConstants.ErrorMessages.ResourceNotFound);
    private static readonly CompositeFormat ResourceDoesNotExistDetailFormat = CompositeFormat.Parse(ApiErrorConstants.ErrorDetails.ResourceDoesNotExist);
    private static readonly CompositeFormat ResourceAlreadyExistsMessageFormat = CompositeFormat.Parse(ApiErrorConstants.ErrorMessages.ResourceAlreadyExists);
    private static readonly CompositeFormat ResourceAlreadyExistsDetailFormat = CompositeFormat.Parse(ApiErrorConstants.ErrorDetails.ResourceAlreadyExistsDetail);

    private readonly ILogger<UsersController> _logger;
    private static readonly List<User> _users = [];
    private static int _nextId = 1;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;

        // Initialize with some test data if empty
        if (_users.Count == 0)
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
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.FromResult(new ApiResponse<CollectionModel<User>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        if (!string.Equals(authHeader, "Bearer user-test-token", StringComparison.Ordinal))
        {
            return Task.FromResult(new ApiResponse<CollectionModel<User>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        LogGettingUsersPage(_logger, page, pageSize, null);

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
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        if (!string.Equals(authHeader, "Bearer user-test-token", StringComparison.Ordinal))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        LogGettingUser(_logger, userId, null);

        var user = _users.FirstOrDefault(u => u.Id == userId);
        return user == null
            ? Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.NotFound,
                    string.Format(CultureInfo.InvariantCulture, ResourceNotFoundMessageFormat, "User", userId),
                    string.Format(CultureInfo.InvariantCulture, ResourceDoesNotExistDetailFormat, "user"))
            ).ToApiResult(System.Net.HttpStatusCode.NotFound))
            : Task.FromResult(new ApiResponse<User>(user).ToApiResult());
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
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        if (!string.Equals(authHeader, "Bearer user-test-token", StringComparison.Ordinal))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        LogCreatingUser(_logger, user.Username, null);

        // Check if username already exists
        if (_users.Any(u => u.Username == user.Username))
        {
            return Task.FromResult(new ApiResponse<User>(
                new ApiError(ApiErrorConstants.ErrorCodes.ResourceExists,
                    string.Format(CultureInfo.InvariantCulture, ResourceAlreadyExistsMessageFormat, "User", "username", user.Username),
                    string.Format(CultureInfo.InvariantCulture, ResourceAlreadyExistsDetailFormat, "user", "username"))
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
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.AuthorizationHeaderRequired,
                    ApiErrorConstants.ErrorDetails.BearerTokenMissing)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        if (!string.Equals(authHeader, "Bearer user-test-token", StringComparison.Ordinal))
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.InvalidBearerToken)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        LogDeletingUser(_logger, userId, null);

        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return Task.FromResult(new ApiResponse<string>(
                new ApiError(ApiErrorConstants.ErrorCodes.NotFound,
                    string.Format(CultureInfo.InvariantCulture, ResourceNotFoundMessageFormat, "User", userId),
                    string.Format(CultureInfo.InvariantCulture, ResourceDoesNotExistDetailFormat, "user"))
            ).ToApiResult(System.Net.HttpStatusCode.NotFound));
        }

        _ = _users.Remove(user);

        return Task.FromResult(new ApiResponse<string>($"User {userId} deleted successfully").ToApiResult());
    }
}
