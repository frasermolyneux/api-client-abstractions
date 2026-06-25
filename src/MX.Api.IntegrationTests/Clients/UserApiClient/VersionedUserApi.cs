using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Implementation of versioned User API client
/// </summary>
public class VersionedUserApi(IUserApiV1 v1Api) : IVersionedUserApi
{

    /// <summary>
    /// Gets the V1 User API
    /// </summary>
    public IUserApiV1 V1 { get; } = v1Api;
}
