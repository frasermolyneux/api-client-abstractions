using Microsoft.Extensions.Logging;
using MX.Api.Client.Auth;

namespace MX.Api.Client.Tests.TestClients;

/// <summary>
/// First fake sub-API used to mirror the unified-client registration shape.
/// </summary>
public interface ISubApiA
{
    /// <summary>Sample cached-eligible method.</summary>
    Task<string> GetA(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Second fake sub-API used to mirror the unified-client registration shape.
/// </summary>
public interface ISubApiB
{
    /// <summary>Sample cached-eligible method.</summary>
    Task<string> GetB(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Third fake sub-API used to mirror the unified-client registration shape.
/// </summary>
public interface ISubApiC
{
    /// <summary>Sample cached-eligible method.</summary>
    Task<string> GetC(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Fake sub-API that is intentionally NOT registered as a typed client. Used to prove that unmatched
/// operations in a <c>SharedCacheConfiguration</c> are surfaced as a clear error.
/// </summary>
public interface IUnregisteredSubApi
{
    /// <summary>Sample cached-eligible method on an interface that will not be registered.</summary>
    Task<string> GetUnregistered(int id, CancellationToken cancellationToken = default);
}

/// <summary>Implementation of <see cref="ISubApiA"/>.</summary>
public class SubApiA(
    ILogger<BaseApi<TestApiOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    TestApiOptions options)
    : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), ISubApiA
{
    /// <inheritdoc />
    public Task<string> GetA(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"A:{id}");
    }
}

/// <summary>Implementation of <see cref="ISubApiB"/>.</summary>
public class SubApiB(
    ILogger<BaseApi<TestApiOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    TestApiOptions options)
    : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), ISubApiB
{
    /// <inheritdoc />
    public Task<string> GetB(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"B:{id}");
    }
}

/// <summary>Implementation of <see cref="ISubApiC"/>.</summary>
public class SubApiC(
    ILogger<BaseApi<TestApiOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    TestApiOptions options)
    : BaseApi<TestApiOptions>(logger, apiTokenProvider, restClientService, options), ISubApiC
{
    /// <inheritdoc />
    public Task<string> GetC(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"C:{id}");
    }
}
