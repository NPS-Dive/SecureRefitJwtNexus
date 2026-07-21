namespace ListMaker.Api.Features.Authentication.Models;

/// <summary>
/// Represents a standardized authentication error response returned by the API.
/// </summary>
public sealed class AuthenticationErrorResponse
{
    /// <summary>
    /// Gets or initializes a short machine-readable error code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets or initializes a human-readable error message.
    /// </summary>
    public required string Message { get; init; }
}