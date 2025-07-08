using MX.Api.Abstractions;
using MX.Api.IntegrationTests.DummyApis.UserApi.Models;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Interface for the User API client
/// </summary>
public interface IUserApiClient
{
    /// <summary>
    /// Gets the versioned User API
    /// </summary>
    IVersionedUserApi Users { get; }
}
