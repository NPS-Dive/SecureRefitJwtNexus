using ListMaker.Client.Authentication;
using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.ListMakerIntegration.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ListReader.Api.Features.ListMakerIntegration.Services;

/// <summary>
/// Caches and refreshes the downstream JWT token used to authenticate
/// against ListMaker.Api.
/// </summary>
/// <remarks>
/// This service stores the raw token in in-memory cache until shortly before
/// its expiration time. When the cached token is missing or near expiry,
/// it performs a new login call to ListMaker.Api using the configured
/// downstream credentials.
/// </remarks>
public sealed class ListMakerAccessTokenCacheService : IListMakerAccessTokenCacheService
    {
    private const string CacheKey = "listmaker-access-token";
    private static readonly SemaphoreSlim TokenRefreshLock = new(1, 1);

    private readonly IMemoryCache _memoryCache;
    private readonly IListMakerAuthApi _listMakerAuthApi;
    private readonly ListMakerCredentialsOptions _credentialsOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListMakerAccessTokenCacheService"/> class.
    /// </summary>
    /// <param name="memoryCache">
    /// The in-memory cache used to store downstream token state.
    /// </param>
    /// <param name="listMakerAuthApi">
    /// The Refit authentication client used to log in to ListMaker.Api.
    /// </param>
    /// <param name="credentialsOptions">
    /// The configured downstream credentials.
    /// </param>
    public ListMakerAccessTokenCacheService (
        IMemoryCache memoryCache,
        IListMakerAuthApi listMakerAuthApi,
        IOptions<ListMakerCredentialsOptions> credentialsOptions )
        {
        _memoryCache = memoryCache;
        _listMakerAuthApi = listMakerAuthApi;
        _credentialsOptions = credentialsOptions.Value;
        }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync ( CancellationToken cancellationToken )
        {
        if (TryGetValidCachedToken(out string? cachedToken))
            {
            return cachedToken;
            }

        await TokenRefreshLock.WaitAsync(cancellationToken);

        try
            {
            if (TryGetValidCachedToken(out cachedToken))
                {
                return cachedToken;
                }

            ValidateCredentials();

            LoginResponse loginResponse = await _listMakerAuthApi.LoginAsync(
                new LoginRequest
                    {
                    Username = _credentialsOptions.Username,
                    Password = _credentialsOptions.Password
                    },
                cancellationToken);

            if (string.IsNullOrWhiteSpace(loginResponse.AccessToken))
                {
                throw new InvalidOperationException(
                    "ListMaker.Api returned an empty access token.");
                }

            DateTimeOffset cacheExpiryUtc = CalculateSafeCacheExpiryUtc(loginResponse.ExpiresAtUtc);

            CachedAccessToken cachedAccessToken = new(
                loginResponse.AccessToken,
                loginResponse.ExpiresAtUtc);

            _memoryCache.Set(
                CacheKey,
                cachedAccessToken,
                new MemoryCacheEntryOptions
                    {
                    AbsoluteExpiration = cacheExpiryUtc
                    });

            return loginResponse.AccessToken;
            }
        finally
            {
            TokenRefreshLock.Release();
            }
        }

    private bool TryGetValidCachedToken ( out string? accessToken )
        {
        accessToken = null;

        if (!_memoryCache.TryGetValue(CacheKey, out CachedAccessToken? cached))
            {
            return false;
            }

        if (cached is null)
            {
            return false;
            }

        if (cached.ExpiresAtUtc <= DateTimeOffset.UtcNow.AddSeconds(30))
            {
            return false;
            }

        accessToken = cached.AccessToken;
        return true;
        }

    private void ValidateCredentials ()
        {
        if (string.IsNullOrWhiteSpace(_credentialsOptions.Username))
            {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Username)}' is required.");
            }

        if (string.IsNullOrWhiteSpace(_credentialsOptions.Password))
            {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Password)}' is required.");
            }
        }

    private static DateTimeOffset CalculateSafeCacheExpiryUtc ( DateTimeOffset tokenExpiresAtUtc )
        {
        DateTimeOffset safeExpiryUtc = tokenExpiresAtUtc.AddSeconds(-30);

        if (safeExpiryUtc <= DateTimeOffset.UtcNow)
            {
            safeExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(5);
            }

        return safeExpiryUtc;
        }

    /// <summary>
    /// Represents a cached downstream access token entry.
    /// </summary>
    /// <param name="AccessToken">
    /// The raw JWT access token.
    /// </param>
    /// <param name="ExpiresAtUtc">
    /// The actual expiration timestamp reported by ListMaker.Api.
    /// </param>
    private sealed record CachedAccessToken ( string AccessToken, DateTimeOffset ExpiresAtUtc );
    }
