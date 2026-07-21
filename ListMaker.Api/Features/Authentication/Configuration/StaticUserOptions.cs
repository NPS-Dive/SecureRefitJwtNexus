namespace ListMaker.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents the static development user credentials accepted by the ListMaker API.
/// </summary>
/// <remarks>
/// This is intentionally simple for the demo integration scenario.
/// Production systems should use a real identity provider or secure credential store.
/// </remarks>
public sealed class StaticUserOptions
{
    /// <summary>
    /// The configuration section name used in appsettings files.
    /// </summary>
    public const string SectionName = "StaticUser";

    /// <summary>
    /// Gets or sets the static username allowed to authenticate.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the static password allowed to authenticate.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}