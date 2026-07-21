namespace ListMaker.Contracts.Authentication;

/// <summary>
/// Represents the response returned by a successful login operation.
/// </summary>
/// <remarks>
/// Both <c>ListMaker.Api</c> and <c>ListReader.Api</c> will return this contract
/// from their login endpoints.
///
/// The response contains enough information for callers to use the JWT token
/// and understand when it expires.
/// </remarks>
public sealed record LoginResponse
{
    /// <summary>
    /// Gets the issued JWT access token.
    /// </summary>
    /// <remarks>
    /// The caller should send this value in the HTTP Authorization header:
    ///
    /// <code>
    /// Authorization: Bearer {accessToken}
    /// </code>
    /// </remarks>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Gets the token type.
    /// </summary>
    /// <remarks>
    /// For this demo, this value will be:
    ///
    /// <code>
    /// Bearer
    /// </code>
    /// </remarks>
    public required string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets the exact UTC date and time when the token expires.
    /// </summary>
    /// <remarks>
    /// This is especially important for <c>ListReader.Api</c>, because it will
    /// cache the downstream <c>ListMaker.Api</c> token until shortly before this
    /// expiration time.
    /// </remarks>
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    /// <summary>
    /// Gets the number of seconds from the issue time until expiration.
    /// </summary>
    /// <remarks>
    /// This is useful for clients that prefer duration-based expiration handling.
    /// </remarks>
    public required int ExpiresInSeconds { get; set; }
}