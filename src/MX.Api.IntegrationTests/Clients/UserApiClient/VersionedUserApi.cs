using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Implementation of versioned User API client
/// </summary>
public class VersionedUserApi : IVersionedUserApi
{
    public VersionedUserApi(IUserApiV1 v1Api)
    {
        V1 = v1Api;
    }

    /// <summary>
    /// Gets the V1 User API
    /// </summary>
    public IUserApiV1 V1 { get; }
}
