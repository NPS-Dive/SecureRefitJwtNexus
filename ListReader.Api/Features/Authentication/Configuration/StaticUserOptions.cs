namespace ListReader.Api.Features.Authentication.Configuration;

/// <summary>
/// Represents static demo credentials for authenticating callers of ListReader.Api.
/// </summary>
public sealed class StaticUserOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "StaticUser";

    /// <summary>
    /// The allowed username.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The allowed password.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}