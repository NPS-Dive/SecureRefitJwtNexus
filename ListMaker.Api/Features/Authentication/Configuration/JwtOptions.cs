namespace ListMaker.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents JWT authentication configuration values.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// The configuration section name used for JWT settings.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// The minimum accepted signing key length for HMAC SHA algorithms.
    /// </summary>
    public const int MinimumSigningKeyLength = 32;

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT signing key.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT expiration duration in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; }
}