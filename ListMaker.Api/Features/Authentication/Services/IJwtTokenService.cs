namespace ListMaker.Api.Features.Authentication.Services;

/// <summary>
/// Defines behavior for generating JWT access tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a signed JWT access token for the specified username.
    /// </summary>
    /// <param name="username">The authenticated username.</param>
    /// <returns>A signed JWT access token.</returns>
    string CreateAccessToken ( string username );
}