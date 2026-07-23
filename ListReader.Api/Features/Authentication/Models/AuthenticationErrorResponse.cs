namespace ListReader.Api.Features.Authentication.Models;

/// <summary>
/// Represents a simple authentication error response payload.
/// </summary>
public sealed class AuthenticationErrorResponse
{
    /// <summary>
    /// A machine-readable error code.
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    /// A human-readable error description.
    /// </summary>
    public string ErrorDescription { get; init; } = string.Empty;
}