using ListMaker.Contracts.Authentication;

namespace ListReader.Api.Features.Authentication.Services;

/// <summary>
/// Defines JWT creation behavior for ListReader.Api.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified username.
    /// </summary>
    /// <param name="username">
    /// The authenticated username.
    /// </param>
    /// <returns>
    /// A login response containing the access token and expiration metadata.
    /// </returns>
    LoginResponse GenerateToken ( string username );
}