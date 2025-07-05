using System.Runtime.InteropServices;
using System.Security;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for Entra ID using client credentials (client ID and secret).
/// </summary>
public class ClientCredentialAuthenticationOptions : EntraIdAuthenticationOptions, IDisposable
{
    private SecureString? _clientSecret;
    private bool _disposed = false;

    /// <summary>
    /// Gets or sets the tenant (directory) ID for authentication.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client (application) ID for authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for authentication (stored securely).
    /// </summary>
    public SecureString? ClientSecret
    {
        get => _clientSecret;
        set
        {
            _clientSecret?.Dispose();
            _clientSecret = value;
        }
    }

    /// <summary>
    /// Sets the client secret from a plain string. The plain string is immediately cleared from memory.
    /// </summary>
    /// <param name="clientSecret">The client secret as a plain string.</param>
    public void SetClientSecret(string clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret))
        {
            _clientSecret?.Dispose();
            _clientSecret = null;
            return;
        }

        _clientSecret?.Dispose();
        _clientSecret = new SecureString();

        foreach (char c in clientSecret)
        {
            _clientSecret.AppendChar(c);
        }

        _clientSecret.MakeReadOnly();
    }

    /// <summary>
    /// Gets the client secret as a plain string. The returned string should be used immediately and cleared from memory.
    /// </summary>
    /// <returns>The client secret as a plain string, or empty string if not set.</returns>
    public string GetClientSecretAsString()
    {
        if (_clientSecret == null || _clientSecret.Length == 0)
            return string.Empty;

        IntPtr ptr = Marshal.SecureStringToBSTR(_clientSecret);
        try
        {
            return Marshal.PtrToStringBSTR(ptr) ?? string.Empty;
        }
        finally
        {
            Marshal.ZeroFreeBSTR(ptr);
        }
    }

    /// <summary>
    /// Releases all resources used by the ClientCredentialAuthenticationOptions.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ClientCredentialAuthenticationOptions and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _clientSecret?.Dispose();
                _clientSecret = null;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure SecureString is disposed even if Dispose is not called.
    /// </summary>
    ~ClientCredentialAuthenticationOptions()
    {
        Dispose(false);
    }
}
