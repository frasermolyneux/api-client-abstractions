using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using MX.Api.Client.Configuration;

namespace MX.Api.Client.Auth;

/// <summary>
/// Client credentials implementation of ITokenCredentialProvider that uses ClientSecretCredential.
/// This provider is used to authenticate with Azure services using client ID and client secret.
/// </summary>
public class ClientCredentialProvider : ITokenCredentialProvider
{
    private readonly ILogger<ClientCredentialProvider>? logger;
    private readonly ClientCredentialAuthenticationOptions options;
    private readonly TokenCredentialOptions? tokenCredentialOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialProvider"/> class
    /// with client credential authentication options.
    /// </summary>
    /// <param name="options">The client credential authentication options containing secure credentials.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required properties are null or empty.</exception>
    public ClientCredentialProvider(ClientCredentialAuthenticationOptions options)
        : this(null, options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialProvider"/> class
    /// with optional logger and custom token credential options.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="options">The client credential authentication options containing secure credentials.</param>
    /// <param name="tokenCredentialOptions">Optional Azure credential options to configure authentication methods.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required properties are null or empty.</exception>
    public ClientCredentialProvider(
        ILogger<ClientCredentialProvider>? logger,
        ClientCredentialAuthenticationOptions options,
        TokenCredentialOptions? tokenCredentialOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.TenantId))
            throw new ArgumentException("TenantId cannot be null or empty", nameof(options));

        if (string.IsNullOrEmpty(options.ClientId))
            throw new ArgumentException("ClientId cannot be null or empty", nameof(options));

        if (options.ClientSecret == null || options.ClientSecret.Length == 0)
            throw new ArgumentException("ClientSecret cannot be null or empty", nameof(options));

        this.logger = logger;
        this.options = options;
        this.tokenCredentialOptions = tokenCredentialOptions;
    }

    /// <summary>
    /// Legacy constructor for backward compatibility. Use the constructor that accepts ClientCredentialAuthenticationOptions instead.
    /// </summary>
    /// <param name="tenantId">The tenant (directory) ID of the application registration.</param>
    /// <param name="clientId">The client (application) ID of the application registration.</param>
    /// <param name="clientSecret">The client secret of the application registration.</param>
    /// <exception cref="ArgumentException">Thrown when any required parameters are null or empty.</exception>
    [Obsolete("Use the constructor that accepts ClientCredentialAuthenticationOptions for enhanced security")]
    public ClientCredentialProvider(string tenantId, string clientId, string clientSecret)
        : this(null, tenantId, clientId, clientSecret, null)
    {
    }

    /// <summary>
    /// Legacy constructor for backward compatibility. Use the constructor that accepts ClientCredentialAuthenticationOptions instead.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="tenantId">The tenant (directory) ID of the application registration.</param>
    /// <param name="clientId">The client (application) ID of the application registration.</param>
    /// <param name="clientSecret">The client secret of the application registration.</param>
    /// <param name="tokenCredentialOptions">Optional Azure credential options to configure authentication methods.</param>
    /// <exception cref="ArgumentException">Thrown when any required parameters are null or empty.</exception>
    [Obsolete("Use the constructor that accepts ClientCredentialAuthenticationOptions for enhanced security")]
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
        this.tokenCredentialOptions = tokenCredentialOptions;

        // Create options object for legacy support
        var clientCredOptions = new ClientCredentialAuthenticationOptions
        {
            TenantId = tenantId,
            ClientId = clientId
        };
        clientCredOptions.SetClientSecret(clientSecret);
        this.options = clientCredOptions;
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
            options.ClientId, options.TenantId);

        // Get the client secret securely - it will be automatically cleared from memory
        var clientSecret = options.GetClientSecretAsString();

        try
        {
            // Creating ClientSecretCredential is not an async operation,
            // but we wrap it in a Task to conform to the async interface pattern
            return Task.FromResult<TokenCredential>(
                tokenCredentialOptions is ClientSecretCredentialOptions clientSecretOptions
                    ? new ClientSecretCredential(options.TenantId, options.ClientId, clientSecret, clientSecretOptions)
                    : new ClientSecretCredential(options.TenantId, options.ClientId, clientSecret));
        }
        finally
        {
            // Clear the client secret from memory immediately after use
            if (!string.IsNullOrEmpty(clientSecret))
            {
                // Zero out the string in memory (best effort)
                unsafe
                {
                    fixed (char* ptr = clientSecret)
                    {
                        for (int i = 0; i < clientSecret.Length; i++)
                        {
                            ptr[i] = '\0';
                        }
                    }
                }
            }
        }
    }
}
