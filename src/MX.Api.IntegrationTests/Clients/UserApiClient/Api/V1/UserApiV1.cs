using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;
using MX.Api.IntegrationTests.DummyApis.UserApi.Models;
using RestSharp;

namespace MX.Api.IntegrationTests.Clients.UserApiClient.Api.V1;

/// <summary>
/// Implementation of User API version 1
/// </summary>
public class UserApiV1 : BaseApi<UserApiOptions>, IUserApiV1
{
    public UserApiV1(
        ILogger<BaseApi<UserApiOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        UserApiOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    public async Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users?page={page}&pageSize={pageSize}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<CollectionModel<User>>();
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public async Task<ApiResult<User>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<User>();
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public async Task<ApiResult<User>> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("v1/users", Method.Post, cancellationToken);
        request.AddJsonBody(user);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<User>();
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    public async Task<ApiResult<string>> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users/{userId}", Method.Delete, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<string>();
    }
}
