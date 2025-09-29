using System;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for API Key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationOptions, IDisposable
{
    private char[]? _apiKeyBuffer;
    private bool _disposed = false;

    /// <summary>
    /// Gets or sets the API key used for authentication. The value is stored internally using a mutable buffer
    /// that is cleared when the options are disposed.
    /// </summary>
    public string? ApiKey
    {
        get => GetApiKeyInternalOrNull();
        set => SetApiKey(value);
    }

    /// <summary>
    /// Gets a value indicating whether an API key has been configured.
    /// </summary>
    public bool HasApiKey => _apiKeyBuffer != null && _apiKeyBuffer.Length > 0;

    /// <summary>
    /// Sets the API key from a plain string. The value is copied into an internal buffer and any previously stored value
    /// is cleared.
    /// </summary>
    /// <param name="apiKey">The API key as a plain string, or <c>null</c> to clear the stored value.</param>
    public void SetApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            ClearApiKeyBuffer();
            return;
        }

        ClearApiKeyBuffer();
        _apiKeyBuffer = apiKey.ToCharArray();
    }

    /// <summary>
    /// Gets the API key as a plain string. The returned string should be used immediately and cleared from memory.
    /// </summary>
    /// <returns>The API key as a plain string, or empty string if not set.</returns>
    public string GetApiKeyAsString()
    {
        if (_apiKeyBuffer == null || _apiKeyBuffer.Length == 0)
        {
            return string.Empty;
        }

        return new string(_apiKeyBuffer);
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
                ClearApiKeyBuffer();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure the API key buffer is cleared even if Dispose is not called.
    /// </summary>
    ~ApiKeyAuthenticationOptions()
    {
        Dispose(false);
    }

    private string? GetApiKeyInternalOrNull()
    {
        if (_apiKeyBuffer == null || _apiKeyBuffer.Length == 0)
        {
            return null;
        }

        return new string(_apiKeyBuffer);
    }

    private void ClearApiKeyBuffer()
    {
        if (_apiKeyBuffer == null)
        {
            return;
        }

        Array.Clear(_apiKeyBuffer, 0, _apiKeyBuffer.Length);
        _apiKeyBuffer = null;
    }
}
