namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// User API client implementation using versioned approach
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiClient
/// </remarks>
public class UserApiClient(IVersionedUserApi versionedUserApi) : IUserApiClient
{

    /// <summary>
    /// Gets the versioned User API
    /// </summary>
    public IVersionedUserApi Users { get; } = versionedUserApi;
}
