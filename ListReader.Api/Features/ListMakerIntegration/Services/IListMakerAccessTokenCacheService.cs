namespace ListReader.Api.Features.ListMakerIntegration.Services;

/// <summary>
/// Provides cached access token management for authenticating
/// ListReader.Api against ListMaker.Api.
/// </summary>
public interface IListMakerAccessTokenCacheService
{
    /// <summary>
    /// Gets a valid downstream access token for ListMaker.Api.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that allows the operation to be cancelled.
    /// </param>
    /// <returns>
    /// A valid raw JWT access token string, without the "Bearer " prefix.
    /// </returns>
    Task<string> GetAccessTokenAsync ( CancellationToken cancellationToken );
}