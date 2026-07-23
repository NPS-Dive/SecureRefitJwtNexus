namespace ListReader.Api.Features.ListMakerIntegration.Configuration;

/// <summary>
/// Represents the downstream credentials used by ListReader.Api
/// to authenticate against ListMaker.Api.
/// </summary>
public sealed class ListMakerCredentialsOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "ListMakerCredentials";

    /// <summary>
    /// The username used to log in to ListMaker.Api.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The password used to log in to ListMaker.Api.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}