namespace ListMaker.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents JWT configuration values used for issuing and validating access tokens.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// The configuration section name used in appsettings files.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symmetric signing key used to sign JWT tokens.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiration duration in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}