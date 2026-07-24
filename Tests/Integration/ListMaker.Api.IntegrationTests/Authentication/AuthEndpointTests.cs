using System.Net;
using System.Net.Http.Json;
using ListMaker.Api.Features.Authentication.Models;
using ListMaker.Api.IntegrationTests.Infrastructure;
using ListMaker.Contracts.Authentication;

namespace ListMaker.Api.IntegrationTests.Authentication;

/// <summary>
/// Contains integration tests for the authentication endpoints of <c>ListMaker.Api</c>.
/// </summary>
[TestFixture]
public sealed class AuthEndpointTests
    {
    private ListMakerApiWebApplicationFactory _factory = null!;
    private HttpClient _httpClient = null!;

    /// <summary>
    /// Creates a fresh test host and HTTP client before each test.
    /// </summary>
    [SetUp]
    public void SetUp ()
        {
        _factory = new ListMakerApiWebApplicationFactory();
        _httpClient = _factory.CreateClient();
        }

    /// <summary>
    /// Disposes the HTTP client and test host after each test.
    /// </summary>
    [TearDown]
    public void TearDown ()
        {
        _httpClient?.Dispose();
        _factory?.Dispose();
        }

    /// <summary>
    /// Verifies that valid login credentials return HTTP 200 and a usable login response.
    /// </summary>
    [Test]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken ()
        {
        // Arrange
        var request = new LoginRequest
            {
            Username = ListMakerApiWebApplicationFactory.TestUsername,
            Password = ListMakerApiWebApplicationFactory.TestPassword
            };

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponse? loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResponse.TokenType.Should().Be("Bearer");
        loginResponse.ExpiresInSeconds.Should().Be(
            ListMakerApiWebApplicationFactory.TestExpirationMinutes * 60);

        loginResponse.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        }

    /// <summary>
    /// Verifies that invalid credentials return HTTP 401.
    /// </summary>
    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized ()
        {
        // Arrange
        var request = new LoginRequest
            {
            Username = "wrong-user",
            Password = "wrong-password"
            };

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        AuthenticationErrorResponse? errorResponse =
            await response.Content.ReadFromJsonAsync<AuthenticationErrorResponse>();

        errorResponse.Should().NotBeNull();
        errorResponse!.Code.Should().Be("invalid_credentials");
        errorResponse.Message.Should().Be("The supplied username or password is invalid.");
        }

    /// <summary>
    /// Verifies that a login request with an empty username returns HTTP 400.
    /// </summary>
    [Test]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest ()
        {
        // Arrange
        var request = new LoginRequest
            {
            Username = string.Empty,
            Password = ListMakerApiWebApplicationFactory.TestPassword
            };

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        AuthenticationErrorResponse? errorResponse =
            await response.Content.ReadFromJsonAsync<AuthenticationErrorResponse>();

        errorResponse.Should().NotBeNull();
        errorResponse!.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");
        }

    /// <summary>
    /// Verifies that a login request with an empty password returns HTTP 400.
    /// </summary>
    [Test]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest ()
        {
        // Arrange
        var request = new LoginRequest
            {
            Username = ListMakerApiWebApplicationFactory.TestUsername,
            Password = string.Empty
            };

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        AuthenticationErrorResponse? errorResponse =
            await response.Content.ReadFromJsonAsync<AuthenticationErrorResponse>();

        errorResponse.Should().NotBeNull();
        errorResponse!.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");
        }
    }
