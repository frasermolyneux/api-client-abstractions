using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace MX.Api.Client.Auth;

/// <summary>
/// Client credentials implementation of ITokenCredentialProvider that uses ClientSecretCredential.
/// This provider is used to authenticate with Azure services using client ID and client secret.
/// </summary>
public class ClientCredentialProvider : ITokenCredentialProvider
{
    private readonly ILogger<ClientCredentialProvider>? logger;
    private readonly string tenantId;
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly TokenCredentialOptions? tokenCredentialOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialProvider"/> class
    /// with required credential parameters.
    /// </summary>
    /// <param name="tenantId">The tenant (directory) ID of the application registration.</param>
    /// <param name="clientId">The client (application) ID of the application registration.</param>
    /// <param name="clientSecret">The client secret of the application registration.</param>
    /// <exception cref="ArgumentException">Thrown when any required parameters are null or empty.</exception>
    public ClientCredentialProvider(string tenantId, string clientId, string clientSecret)
        : this(null, tenantId, clientId, clientSecret, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialProvider"/> class
    /// with optional logger and custom token credential options.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="tenantId">The tenant (directory) ID of the application registration.</param>
    /// <param name="clientId">The client (application) ID of the application registration.</param>
    /// <param name="clientSecret">The client secret of the application registration.</param>
    /// <param name="tokenCredentialOptions">Optional Azure credential options to configure authentication methods.</param>
    /// <exception cref="ArgumentException">Thrown when any required parameters are null or empty.</exception>
    public ClientCredentialProvider(
        ILogger<ClientCredentialProvider>? logger,
        string tenantId,
        string clientId,
        string clientSecret,
        TokenCredentialOptions? tokenCredentialOptions = null)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or empty", nameof(tenantId));

        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException($"'{nameof(clientId)}' cannot be null or empty", nameof(clientId));

        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or empty", nameof(clientSecret));

        this.logger = logger;
        this.tenantId = tenantId;
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.tokenCredentialOptions = tokenCredentialOptions;
    }

    /// <summary>
    /// Gets a ClientSecretCredential instance asynchronously for authentication.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing a ClientSecretCredential instance for token acquisition.</returns>
    public Task<TokenCredential> GetTokenCredentialAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        logger?.LogDebug("Creating ClientSecretCredential for client ID: {ClientId}, tenant ID: {TenantId}",
            clientId, tenantId);

        // Creating ClientSecretCredential is not an async operation,
        // but we wrap it in a Task to conform to the async interface pattern
        return Task.FromResult<TokenCredential>(
            tokenCredentialOptions != null
                ? new ClientSecretCredential(tenantId, clientId, clientSecret, tokenCredentialOptions)
                : new ClientSecretCredential(tenantId, clientId, clientSecret));
    }
}
