namespace ListReader.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents JWT configuration settings for ListReader.Api.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// The token issuer.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// The token audience.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// The symmetric secret key used to sign tokens.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// The token lifetime, in minutes.
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;
}