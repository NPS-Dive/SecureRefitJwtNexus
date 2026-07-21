namespace ListMaker.Contracts.Authentication;

/// <summary>
/// Represents a login request sent to an authentication endpoint.
/// </summary>
/// <remarks>
/// This contract is shared by both APIs:
///
/// <list type="bullet">
///   <item>
///     <description>
///       <c>ListReader.Api</c> uses it when an external/demo user logs in.
///     </description>
///   </item>
///   <item>
///     <description>
///       <c>ListMaker.Api</c> uses it when <c>ListReader.Api</c> logs in as a service caller.
///     </description>
///   </item>
/// </list>
///
/// This DTO intentionally contains only transport data.
/// It must not contain authentication logic, validation logic, JWT logic,
/// database logic, or ASP.NET Core-specific attributes.
/// </remarks>
public sealed record LoginRequest
{
    /// <summary>
    /// Gets the username supplied by the caller.
    /// </summary>
    /// <example>reader-user</example>
    public required string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password supplied by the caller.
    /// </summary>
    /// <remarks>
    /// This value is only used for the demo login flow.
    /// In a production system, passwords must never be logged or returned.
    /// </remarks>
    /// <example>reader-password</example>
    public required string Password { get; init; } = string.Empty;
    }