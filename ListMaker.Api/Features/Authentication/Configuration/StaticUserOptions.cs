namespace ListMaker.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents static login credentials used by the authentication endpoint.
/// </summary>
public sealed class StaticUserOptions
{
    /// <summary>
    /// The configuration section name used for static user settings.
    /// </summary>
    public const string SectionName = "StaticUser";

    /// <summary>
    /// Gets or sets the configured username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configured password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}