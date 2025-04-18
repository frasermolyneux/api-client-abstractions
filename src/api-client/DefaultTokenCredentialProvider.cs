using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MxIO.ApiClient;

/// <summary>
/// Default implementation of ITokenCredentialProvider that uses DefaultAzureCredential.
/// This provider is used to authenticate with Azure services using the DefaultAzureCredential,
/// which tries multiple authentication methods in sequence.
/// </summary>
public class DefaultTokenCredentialProvider : ITokenCredentialProvider
{
    private readonly ILogger<DefaultTokenCredentialProvider>? logger;
    private readonly DefaultAzureCredentialOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenCredentialProvider"/> class
    /// with default credential options.
    /// </summary>
    public DefaultTokenCredentialProvider()
        : this(null, new DefaultAzureCredentialOptions
        {
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCodeCredential = false,
            ExcludeAzurePowerShellCredential = false,
            ExcludeEnvironmentCredential = false,
            ExcludeManagedIdentityCredential = false
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenCredentialProvider"/> class
    /// with optional logger and custom credential options.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <param name="options">Azure credential options to configure authentication methods</param>
    public DefaultTokenCredentialProvider(
        ILogger<DefaultTokenCredentialProvider>? logger,
        DefaultAzureCredentialOptions options)
    {
        this.logger = logger;
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenCredentialProvider"/> class
    /// that uses options from dependency injection.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <param name="optionsAccessor">Options accessor for credential options</param>
    public DefaultTokenCredentialProvider(
        ILogger<DefaultTokenCredentialProvider>? logger,
        IOptions<DefaultAzureCredentialOptions> optionsAccessor)
    {
        this.logger = logger;
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor), "DefaultAzureCredentialOptions must be provided");
    }

    /// <summary>
    /// Gets a DefaultAzureCredential instance for authentication.
    /// The DefaultAzureCredential attempts to authenticate via the following mechanisms in order:
    /// - Environment variables
    /// - Managed Identity
    /// - Visual Studio
    /// - Azure CLI
    /// - Azure PowerShell
    /// </summary>
    /// <returns>A DefaultAzureCredential instance for token acquisition.</returns>
    public TokenCredential GetTokenCredential()
    {
        logger?.LogDebug("Creating DefaultAzureCredential with authentication settings: ManagedIdentity={ManagedIdentityEnabled}, " +
                         "Environment={EnvironmentEnabled}, VisualStudioCode={VSCodeEnabled}, AzurePowerShell={PowerShellEnabled}",
            !options.ExcludeManagedIdentityCredential,
            !options.ExcludeEnvironmentCredential,
            !options.ExcludeVisualStudioCodeCredential,
            !options.ExcludeAzurePowerShellCredential);

        return new DefaultAzureCredential(options);
    }
}