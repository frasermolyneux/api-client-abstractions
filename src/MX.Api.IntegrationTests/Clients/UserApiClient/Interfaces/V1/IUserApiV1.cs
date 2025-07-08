using MX.Api.Abstractions;
using MX.Api.IntegrationTests.DummyApis.UserApi.Models;

namespace MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;

/// <summary>
/// Interface for User API version 1
/// </summary>
public interface IUserApiV1
{
    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated users</returns>
    Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User</returns>
    Task<ApiResult<User>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    Task<ApiResult<User>> CreateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    Task<ApiResult<string>> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
}
