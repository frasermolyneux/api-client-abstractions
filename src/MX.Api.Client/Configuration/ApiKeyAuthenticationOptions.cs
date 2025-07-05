using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for API Key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationOptions, IDisposable
{
    private SecureString? _apiKey;
    private bool _disposed = false;

    /// <summary>
    /// Gets or sets the API key used for authentication (stored securely).
    /// </summary>
    public SecureString? ApiKey
    {
        get => _apiKey;
        set
        {
            _apiKey?.Dispose();
            _apiKey = value;
        }
    }

    /// <summary>
    /// Sets the API key from a plain string. The plain string is immediately cleared from memory.
    /// </summary>
    /// <param name="apiKey">The API key as a plain string.</param>
    public void SetApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            _apiKey?.Dispose();
            _apiKey = null;
            return;
        }

        _apiKey?.Dispose();
        _apiKey = new SecureString();

        foreach (char c in apiKey)
        {
            _apiKey.AppendChar(c);
        }

        _apiKey.MakeReadOnly();
    }

    /// <summary>
    /// Gets the API key as a plain string. The returned string should be used immediately and cleared from memory.
    /// </summary>
    /// <returns>The API key as a plain string, or empty string if not set.</returns>
    public string GetApiKeyAsString()
    {
        if (_apiKey == null || _apiKey.Length == 0)
            return string.Empty;

        IntPtr ptr = Marshal.SecureStringToBSTR(_apiKey);
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
    /// Gets or sets the header name for the API key. Defaults to "Ocp-Apim-Subscription-Key" for Azure API Management.
    /// </summary>
    public string HeaderName { get; set; } = "Ocp-Apim-Subscription-Key";

    /// <summary>
    /// Gets the type of authentication.
    /// </summary>
    public override AuthenticationType AuthenticationType => AuthenticationType.ApiKey;

    /// <summary>
    /// Releases all resources used by the ApiKeyAuthenticationOptions.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ApiKeyAuthenticationOptions and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _apiKey?.Dispose();
                _apiKey = null;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure SecureString is disposed even if Dispose is not called.
    /// </summary>
    ~ApiKeyAuthenticationOptions()
    {
        Dispose(false);
    }
}
