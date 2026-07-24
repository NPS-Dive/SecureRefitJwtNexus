using ListMaker.Api.Controllers;
using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Features.Authentication.Models;
using ListMaker.Api.Features.Authentication.Services;
using ListMaker.Contracts.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace ListMaker.Api.UnitTests.Features.Authentication;

/// <summary>
/// Contains unit tests for <see cref="AuthController" />.
/// </summary>
[TestFixture]
public sealed class AuthControllerTests
    {
    /// <summary>
    /// Verifies that valid configured credentials return HTTP 200 with a login response.
    /// </summary>
    [Test]
    public void Login_WithValidCredentials_ShouldReturnOkWithLoginResponse ()
        {
        // Arrange
        const string expectedUsername = "reader-user";
        const string expectedPassword = "reader-password";
        const string expectedToken = "fake-jwt-token";

        var staticUserOptions = new StaticUserOptions
            {
            Username = expectedUsername,
            Password = expectedPassword
            };

        var jwtOptions = new JwtOptions
            {
            Issuer = "ListMaker.Api",
            Audience = "ListReader.Api",
            SigningKey = "0123456789ABCDEF0123456789ABCDEF",
            ExpirationMinutes = 30
            };

        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        jwtTokenServiceMock
            .Setup(service => service.CreateAccessToken(expectedUsername))
            .Returns(expectedToken);

        var controller = new AuthController(
            Options.Create(staticUserOptions),
            Options.Create(jwtOptions),
            jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = expectedUsername,
            Password = expectedPassword
            };

        DateTimeOffset beforeLoginUtc = DateTimeOffset.UtcNow;

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        DateTimeOffset afterLoginUtc = DateTimeOffset.UtcNow;

        // Assert
        OkObjectResult okResult = actionResult.Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        LoginResponse response = okResult.Value
            .Should()
            .BeOfType<LoginResponse>()
            .Subject;

        response.AccessToken.Should().Be(expectedToken);
        response.TokenType.Should().Be("Bearer");
        response.ExpiresInSeconds.Should().Be(1800);

        response.ExpiresAtUtc.Should().BeOnOrAfter(beforeLoginUtc.AddMinutes(30));
        response.ExpiresAtUtc.Should().BeOnOrBefore(afterLoginUtc.AddMinutes(30).AddSeconds(1));

        jwtTokenServiceMock.Verify(
            service => service.CreateAccessToken(expectedUsername),
            Times.Once);

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that an empty username returns HTTP 400.
    /// </summary>
    [Test]
    public void Login_WithEmptyUsername_ShouldReturnBadRequest ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = string.Empty,
            Password = "reader-password"
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        BadRequestObjectResult badRequestResult = actionResult.Result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = badRequestResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that a whitespace username returns HTTP 400.
    /// </summary>
    [Test]
    public void Login_WithWhitespaceUsername_ShouldReturnBadRequest ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "   ",
            Password = "reader-password"
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        BadRequestObjectResult badRequestResult = actionResult.Result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = badRequestResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that an empty password returns HTTP 400.
    /// </summary>
    [Test]
    public void Login_WithEmptyPassword_ShouldReturnBadRequest ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "reader-user",
            Password = string.Empty
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        BadRequestObjectResult badRequestResult = actionResult.Result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = badRequestResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that a whitespace password returns HTTP 400.
    /// </summary>
    [Test]
    public void Login_WithWhitespacePassword_ShouldReturnBadRequest ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "reader-user",
            Password = "   "
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        BadRequestObjectResult badRequestResult = actionResult.Result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = badRequestResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_login_request");
        errorResponse.Message.Should().Be("Username and password are required.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that an invalid username returns HTTP 401.
    /// </summary>
    [Test]
    public void Login_WithInvalidUsername_ShouldReturnUnauthorized ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "wrong-user",
            Password = "reader-password"
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = actionResult.Result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = unauthorizedResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_credentials");
        errorResponse.Message.Should().Be("The supplied username or password is invalid.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that an invalid password returns HTTP 401.
    /// </summary>
    [Test]
    public void Login_WithInvalidPassword_ShouldReturnUnauthorized ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "reader-user",
            Password = "wrong-password"
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = actionResult.Result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = unauthorizedResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_credentials");
        errorResponse.Message.Should().Be("The supplied username or password is invalid.");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that credential comparison is case-sensitive.
    /// </summary>
    [Test]
    public void Login_WithDifferentUsernameCasing_ShouldReturnUnauthorized ()
        {
        // Arrange
        var jwtTokenServiceMock = new Mock<IJwtTokenService>(MockBehavior.Strict);

        var controller = CreateController(jwtTokenServiceMock.Object);

        var request = new LoginRequest
            {
            Username = "READER-USER",
            Password = "reader-password"
            };

        // Act
        ActionResult<LoginResponse> actionResult = controller.Login(request);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = actionResult.Result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Subject;

        AuthenticationErrorResponse errorResponse = unauthorizedResult.Value
            .Should()
            .BeOfType<AuthenticationErrorResponse>()
            .Subject;

        errorResponse.Code.Should().Be("invalid_credentials");

        jwtTokenServiceMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Creates an authentication controller with default valid options.
    /// </summary>
    /// <param name="jwtTokenService">The mocked JWT token service.</param>
    /// <returns>A configured <see cref="AuthController" /> instance.</returns>
    private static AuthController CreateController ( IJwtTokenService jwtTokenService )
        {
        var staticUserOptions = new StaticUserOptions
            {
            Username = "reader-user",
            Password = "reader-password"
            };

        var jwtOptions = new JwtOptions
            {
            Issuer = "ListMaker.Api",
            Audience = "ListReader.Api",
            SigningKey = "0123456789ABCDEF0123456789ABCDEF",
            ExpirationMinutes = 30
            };

        return new AuthController(
            Options.Create(staticUserOptions),
            Options.Create(jwtOptions),
            jwtTokenService);
        }
    }
