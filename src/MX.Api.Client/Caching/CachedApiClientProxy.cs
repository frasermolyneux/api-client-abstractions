using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;

using MX.Api.Abstractions;
using MX.Api.Client.Configuration;
using MX.Caching.Abstractions;

namespace MX.Api.Client.Caching;

internal class CachedApiClientProxy<TClient> : DispatchProxy
    where TClient : class
{
    private static readonly MethodInfo InvokeCachedMethod = typeof(CachedApiClientProxy<TClient>)
        .GetMethod(nameof(InvokeCachedAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private TClient _target = null!;
    private ApiClientOptionsBase _options = null!;
    private IMxCache _cache = null!;
    private ICachePolicyResolver _policyResolver = null!;
    private DefaultCachePolicies<TClient>? _defaultPolicies;
    private string _cachePartition = null!;

    internal static TClient Create(
        TClient target,
        ApiClientOptionsBase options,
        IMxCache cache,
        ICachePolicyResolver policyResolver,
        DefaultCachePolicies<TClient>? defaultPolicies)
    {
        var client = Create<TClient, CachedApiClientProxy<TClient>>();
        var proxy = (CachedApiClientProxy<TClient>)(object)client;
        proxy._target = target;
        proxy._options = options;
        proxy._cache = cache;
        proxy._policyResolver = policyResolver;
        proxy._defaultPolicies = defaultPolicies;
        proxy._cachePartition = CreateCachePartition(options.BaseUrl, options.CachePartition);
        return client;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        var returnType = targetMethod.ReturnType;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            return InvokeTarget(targetMethod, args);
        }

        CachePolicy? libraryDefault = null;
        _ = _defaultPolicies?.Policies.TryGetValue(targetMethod, out libraryDefault);
        var selection = EffectiveCachePolicySelector.Resolve(targetMethod, _options, libraryDefault);
        var methodIdentity = GetMethodIdentity(targetMethod);
        var operation = new CacheOperation(typeof(TClient).FullName ?? typeof(TClient).Name, methodIdentity);
        var policy = _policyResolver.Resolve(operation, selection.LibraryDefault, selection.ConsumerOverride);

        if (!policy.Enabled)
        {
            return InvokeTarget(targetMethod, args);
        }

        var parameters = targetMethod.GetParameters();
        var arguments = args ?? [];
        var keyArguments = arguments
            .Where((_, index) => parameters[index].ParameterType != typeof(CancellationToken))
            .ToArray();
        var cancellationToken = arguments
            .Where((_, index) => parameters[index].ParameterType == typeof(CancellationToken))
            .OfType<CancellationToken>()
            .FirstOrDefault();
        var canonicalArguments = CacheKeyBuilder.Create(
            "v1",
            typeof(TClient).FullName ?? typeof(TClient).Name,
            methodIdentity,
            keyArguments);
        var key = new CacheKey(
            $"mx-api:v2:{_cachePartition}:{Hash(methodIdentity)}:{Hash(canonicalArguments.Value)}");

        return InvokeCachedMethod
            .MakeGenericMethod(returnType.GenericTypeArguments[0])
            .Invoke(this, [targetMethod, arguments, parameters, key, policy, cancellationToken]);
    }

    internal static string GetMethodIdentity(MethodInfo method)
    {
        var identity = new StringBuilder();
        _ = identity.Append(GetTypeIdentity(method.DeclaringType!));
        _ = identity.Append('.');
        _ = identity.Append(method.Name);

        if (method.IsGenericMethod)
        {
            _ = identity.Append('`');
            _ = identity.Append(method.GetGenericArguments().Length);
            _ = identity.Append('[');
            _ = identity.AppendJoin(',', method.GetGenericArguments().Select(GetTypeIdentity));
            _ = identity.Append(']');
        }

        _ = identity.Append('(');
        _ = identity.AppendJoin(',', method.GetParameters().Select(parameter => GetTypeIdentity(parameter.ParameterType)));
        _ = identity.Append(')');
        return identity.ToString();
    }

    private static string GetTypeIdentity(Type type)
    {
        if (type.IsByRef)
        {
            return $"{GetTypeIdentity(type.GetElementType()!)}&";
        }

        if (type.IsArray)
        {
            return $"{GetTypeIdentity(type.GetElementType()!)}[{new string(',', type.GetArrayRank() - 1)}]";
        }

        if (!type.IsGenericType)
        {
            return GetNamedTypeIdentity(type);
        }

        var genericType = type.GetGenericTypeDefinition();
        var genericName = genericType.FullName ?? genericType.Name;
        var arityIndex = genericName.IndexOf('`', StringComparison.Ordinal);
        if (arityIndex >= 0)
        {
            genericName = genericName[..arityIndex];
        }

        return $"[{genericType.Assembly.FullName}]::{genericName}<{string.Join(',', type.GetGenericArguments().Select(GetTypeIdentity))}>";
    }

    private static string GetNamedTypeIdentity(Type type)
    {
        return $"[{type.Assembly.FullName}]::{type.FullName ?? type.Name}";
    }

    private async Task<TResult> InvokeCachedAsync<TResult>(
        MethodInfo targetMethod,
        object?[] arguments,
        ParameterInfo[] parameters,
        CacheKey key,
        CachePolicy policy,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _cache.GetOrCreateAsync(
                key,
                policy,
                async token =>
                {
                    var invocationArguments = (object?[])arguments.Clone();
                    for (var index = 0; index < parameters.Length; index++)
                    {
                        if (parameters[index].ParameterType == typeof(CancellationToken))
                        {
                            invocationArguments[index] = token;
                        }
                    }

                    var task = (Task<TResult>)InvokeTarget(targetMethod, invocationArguments)!;
                    var result = await task.ConfigureAwait(false);
                    return result is IApiResult apiResult
                        && !apiResult.IsSuccess
                        && !apiResult.IsNotFound
                            ? throw new NonCacheableResultException<TResult>(result)
                            : result;
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (NonCacheableResultException<TResult> exception)
        {
            return exception.Result;
        }
    }

    private sealed class NonCacheableResultException<TResult>(TResult result) : Exception
    {
        public TResult Result { get; } = result;
    }

    private object? InvokeTarget(MethodInfo targetMethod, object?[]? arguments)
    {
        try
        {
            return targetMethod.Invoke(_target, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static string CreateCachePartition(string baseUrl, string cachePartition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(cachePartition);

        var normalizedBaseUrl = Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            ? uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped)
            : baseUrl.Trim();

        return Hash($"{typeof(TClient).AssemblyQualifiedName}|{normalizedBaseUrl}|{cachePartition}");
    }

    private static string Hash(string value)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
