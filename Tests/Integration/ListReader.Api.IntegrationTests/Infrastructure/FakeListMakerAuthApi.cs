using ListMaker.Client.Authentication;
using ListMaker.Contracts.Authentication;

namespace ListReader.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a fake downstream authentication API for integration testing.
/// </summary>
public sealed class FakeListMakerAuthApi : IListMakerAuthApi
{
    /// <summary>
    /// Gets or sets the login response returned by the fake API.
    /// </summary>
    public LoginResponse Response { get; set; } = new()
    {
        AccessToken = "fake-downstream-access-token",
        TokenType = "Bearer",
        ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
        ExpiresInSeconds = 1800
    };

    /// <inheritdoc />
    public Task<LoginResponse> LoginAsync (
        LoginRequest request,
        CancellationToken cancellationToken = default )
    {
        return Task.FromResult(Response);
    }
}