namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// User API client implementation using versioned approach
/// </summary>
public class UserApiClient : IUserApiClient
{
    /// <summary>
    /// Initializes a new instance of the UserApiClient
    /// </summary>
    public UserApiClient(IVersionedUserApi versionedUserApi)
    {
        Users = versionedUserApi;
    }

    /// <summary>
    /// Gets the versioned User API
    /// </summary>
    public IVersionedUserApi Users { get; }
}
