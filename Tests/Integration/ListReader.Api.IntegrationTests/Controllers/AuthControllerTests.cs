using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.Authentication.Models;
using ListReader.Api.IntegrationTests.Infrastructure;

namespace ListReader.Api.IntegrationTests.Controllers;

/// <summary>
/// Contains integration tests for authentication endpoints.
/// </summary>
[TestFixture]
public sealed class AuthControllerTests
    {
    private ListReaderApiWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Initializes the test host and HTTP client before each test.
    /// </summary>
    [SetUp]
    public void SetUp ()
        {
        _factory = new ListReaderApiWebApplicationFactory();
        _client = _factory.CreateClient();
        }

    /// <summary>
    /// Disposes test resources after each test.
    /// </summary>
    [TearDown]
    public void TearDown ()
        {
        _client.Dispose();
        _factory.Dispose();
        }

    /// <summary>
    /// Verifies that valid credentials return a JWT login response.
    /// </summary>
    [Test]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken ()
        {
        // Arrange
        LoginRequest request = new()
            {
            Username = "reader@test",
            Password = "Reader@Test123!"
            };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponse? payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.TokenType.Should().Be("Bearer");
        payload.ExpiresInSeconds.Should().BeGreaterThan(0);
        payload.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        }

    /// <summary>
    /// Verifies that invalid credentials return 401 Unauthorized with the expected payload.
    /// </summary>
    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized ()
        {
        // Arrange
        LoginRequest request = new()
            {
            Username = "wrong-user",
            Password = "wrong-pass"
            };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        AuthenticationErrorResponse? payload =
            await response.Content.ReadFromJsonAsync<AuthenticationErrorResponse>();

        payload.Should().NotBeNull();
        payload!.Error.Should().Be("invalid_credentials");
        payload.ErrorDescription.Should().Be("The supplied username or password is incorrect.");
        }
    }
