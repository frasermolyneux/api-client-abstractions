using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Interface for versioned User API client
/// </summary>
public interface IVersionedUserApi
{
    /// <summary>
    /// Gets the V1 User API
    /// </summary>
    IUserApiV1 V1 { get; }
}
