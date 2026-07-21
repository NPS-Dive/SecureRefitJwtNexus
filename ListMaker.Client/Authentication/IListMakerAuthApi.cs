using ListMaker.Contracts.Authentication;
using Refit;

namespace ListMaker.Client.Authentication;

/// <summary>
/// Defines the typed HTTP client operations for the authentication endpoints
/// exposed by ListMaker.Api.
/// </summary>
/// <remarks>
/// Refit generates the runtime HTTP client implementation for this interface.
/// Consumers should obtain this interface through dependency injection instead
/// of constructing an implementation directly.
/// </remarks>
public interface IListMakerAuthApi
{
    /// <summary>
    /// Authenticates a service or user against ListMaker.Api and returns
    /// the JWT access-token information required for protected endpoints.
    /// </summary>
    /// <param name="request">
    /// The username and password expected by the ListMaker.Api login endpoint.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that allows the caller to cancel the outgoing HTTP request.
    /// </param>
    /// <returns>
    /// The authentication response containing the JWT access token, token type,
    /// and UTC expiration timestamp.
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown by Refit when ListMaker.Api returns a non-success HTTP status code,
    /// such as 400 Bad Request, 401 Unauthorized, or 500 Internal Server Error.
    /// </exception>
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync (
        [Body] LoginRequest request,
        CancellationToken cancellationToken = default );
}