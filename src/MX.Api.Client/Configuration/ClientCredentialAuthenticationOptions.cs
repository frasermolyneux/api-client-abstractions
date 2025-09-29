using System;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for Entra ID using client credentials (client ID and secret).
/// </summary>
public class ClientCredentialAuthenticationOptions : EntraIdAuthenticationOptions, IDisposable
{
    private char[]? _clientSecretBuffer;
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
    /// Gets or sets the client secret for authentication. The value is stored internally using a mutable buffer that is
    /// cleared when the options are disposed.
    /// </summary>
    public string? ClientSecret
    {
        get => GetClientSecretInternalOrNull();
        set => SetClientSecret(value);
    }

    /// <summary>
    /// Gets a value indicating whether a client secret has been configured.
    /// </summary>
    public bool HasClientSecret => _clientSecretBuffer != null && _clientSecretBuffer.Length > 0;

    /// <summary>
    /// Sets the client secret from a plain string. The value is copied into an internal buffer and any previously stored value
    /// is cleared.
    /// </summary>
    /// <param name="clientSecret">The client secret as a plain string, or <c>null</c> to clear the stored value.</param>
    public void SetClientSecret(string? clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret))
        {
            ClearClientSecretBuffer();
            return;
        }

        ClearClientSecretBuffer();
        _clientSecretBuffer = clientSecret.ToCharArray();
    }

    /// <summary>
    /// Gets the client secret as a plain string. The returned string should be used immediately and cleared from memory.
    /// </summary>
    /// <returns>The client secret as a plain string, or empty string if not set.</returns>
    public string GetClientSecretAsString()
    {
        if (_clientSecretBuffer == null || _clientSecretBuffer.Length == 0)
            return string.Empty;

        return new string(_clientSecretBuffer);
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
                ClearClientSecretBuffer();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure the client secret buffer is cleared even if Dispose is not called.
    /// </summary>
    ~ClientCredentialAuthenticationOptions()
    {
        Dispose(false);
    }

    private string? GetClientSecretInternalOrNull()
    {
        if (_clientSecretBuffer == null || _clientSecretBuffer.Length == 0)
        {
            return null;
        }

        return new string(_clientSecretBuffer);
    }

    private void ClearClientSecretBuffer()
    {
        if (_clientSecretBuffer == null)
        {
            return;
        }

        Array.Clear(_clientSecretBuffer, 0, _clientSecretBuffer.Length);
        _clientSecretBuffer = null;
    }
}
