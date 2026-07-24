using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ListMaker.Api.UnitTests.Features.Authentication;

/// <summary>
/// Contains unit tests for <see cref="JwtTokenService" />.
/// </summary>
[TestFixture]
public sealed class JwtTokenServiceTests
    {
    private const string ValidIssuer = "ApiIntegrationDemo.ListMaker.Api";
    private const string ValidAudience = "ApiIntegrationDemo.ListMaker.Clients";
    private const string ValidSigningKey = "ApiIntegrationDemo-ListMaker-Unit-Test-Signing-Key-2026!@0123456789";
    private const int ValidExpirationMinutes = 60;
    private const string ValidUsername = "@maker-service-user";

    /// <summary>
    /// Verifies that a syntactically valid JWT access token is created.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithValidUsernameAndOptions_ShouldReturnReadableJwtToken ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        string token = service.CreateAccessToken(ValidUsername);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
        }

    /// <summary>
    /// Verifies that the created token contains the expected issuer and audience.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithValidOptions_ShouldContainExpectedIssuerAndAudience ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        string token = service.CreateAccessToken(ValidUsername);
        JwtSecurityToken jwtToken = ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be(ValidIssuer);
        jwtToken.Audiences.Should().ContainSingle().Which.Should().Be(ValidAudience);
        }

    /// <summary>
    /// Verifies that the created token contains identity claims for the authenticated username.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithValidUsername_ShouldContainExpectedUsernameClaims ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        string token = service.CreateAccessToken(ValidUsername);
        JwtSecurityToken jwtToken = ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Sub &&
            claim.Value == ValidUsername);

        jwtToken.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.UniqueName &&
            claim.Value == ValidUsername);

        jwtToken.Claims.Should().Contain(claim =>
            claim.Type == ClaimTypes.Name &&
            claim.Value == ValidUsername);
        }

    /// <summary>
    /// Verifies that each token receives a unique JWT ID claim.
    /// </summary>
    [Test]
    public void CreateAccessToken_WhenCalledMultipleTimes_ShouldCreateDifferentJtiValues ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        string firstToken = service.CreateAccessToken(ValidUsername);
        string secondToken = service.CreateAccessToken(ValidUsername);

        JwtSecurityToken firstJwtToken = ReadJwtToken(firstToken);
        JwtSecurityToken secondJwtToken = ReadJwtToken(secondToken);

        string firstJti = firstJwtToken.Claims
            .Single(claim => claim.Type == JwtRegisteredClaimNames.Jti)
            .Value;

        string secondJti = secondJwtToken.Claims
            .Single(claim => claim.Type == JwtRegisteredClaimNames.Jti)
            .Value;

        // Assert
        firstJti.Should().NotBeNullOrWhiteSpace();
        secondJti.Should().NotBeNullOrWhiteSpace();
        secondJti.Should().NotBe(firstJti);
        }

    /// <summary>
    /// Verifies that the created token can be validated using the configured signing key.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithValidOptions_ShouldCreateTokenThatPassesSignatureValidation ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        JwtTokenService service = CreateService(options);
        string token = service.CreateAccessToken(ValidUsername);

        var validationParameters = new TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,

            ValidateAudience = true,
            ValidAudience = options.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            };

        var handler = new JwtSecurityTokenHandler();

        // Act
        ClaimsPrincipal principal = handler.ValidateToken(
            token,
            validationParameters,
            out SecurityToken validatedToken);

        // Assert
        principal.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        validatedToken.Should().BeOfType<JwtSecurityToken>();
        }

    /// <summary>
    /// Verifies that the token expiration roughly matches the configured expiration duration.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithValidOptions_ShouldUseConfiguredExpirationMinutes ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        JwtTokenService service = CreateService(options);
        DateTime beforeCreationUtc = DateTime.UtcNow;

        // Act
        string token = service.CreateAccessToken(ValidUsername);
        DateTime afterCreationUtc = DateTime.UtcNow;
        JwtSecurityToken jwtToken = ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeOnOrAfter(beforeCreationUtc.AddMinutes(options.ExpirationMinutes).AddSeconds(-2));
        jwtToken.ValidTo.Should().BeOnOrBefore(afterCreationUtc.AddMinutes(options.ExpirationMinutes).AddSeconds(2));
        }

    /// <summary>
    /// Verifies that a null username is rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithNullUsername_ShouldThrowArgumentException ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        Action act = () => service.CreateAccessToken(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
        }

    /// <summary>
    /// Verifies that a whitespace username is rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithWhitespaceUsername_ShouldThrowArgumentException ()
        {
        // Arrange
        JwtTokenService service = CreateService(CreateValidOptions());

        // Act
        Action act = () => service.CreateAccessToken("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
        }

    /// <summary>
    /// Verifies that missing issuer configuration is rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithMissingIssuer_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        options.Issuer = string.Empty;

        JwtTokenService service = CreateService(options);

        // Act
        Action act = () => service.CreateAccessToken(ValidUsername);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT issuer is not configured.");
        }

    /// <summary>
    /// Verifies that missing audience configuration is rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithMissingAudience_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        options.Audience = string.Empty;

        JwtTokenService service = CreateService(options);

        // Act
        Action act = () => service.CreateAccessToken(ValidUsername);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT audience is not configured.");
        }

    /// <summary>
    /// Verifies that missing signing key configuration is rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithMissingSigningKey_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        options.SigningKey = string.Empty;

        JwtTokenService service = CreateService(options);

        // Act
        Action act = () => service.CreateAccessToken(ValidUsername);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT signing key is not configured.");
        }

    /// <summary>
    /// Verifies that too-short signing keys are rejected.
    /// </summary>
    [Test]
    public void CreateAccessToken_WithTooShortSigningKey_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        options.SigningKey = "short-signing-key";

        JwtTokenService service = CreateService(options);

        // Act
        Action act = () => service.CreateAccessToken(ValidUsername);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT signing key must be at least 32 characters long for HMAC SHA-256.");
        }

    /// <summary>
    /// Verifies that zero or negative expiration values are rejected.
    /// </summary>
    /// <param name="expirationMinutes">The invalid expiration value.</param>
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-60)]
    public void CreateAccessToken_WithInvalidExpirationMinutes_ShouldThrowInvalidOperationException (
        int expirationMinutes )
        {
        // Arrange
        JwtOptions options = CreateValidOptions();
        options.ExpirationMinutes = expirationMinutes;

        JwtTokenService service = CreateService(options);

        // Act
        Action act = () => service.CreateAccessToken(ValidUsername);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JWT expiration minutes must be greater than zero.");
        }

    /// <summary>
    /// Creates a service instance using the supplied JWT options.
    /// </summary>
    /// <param name="options">The JWT options used by the service.</param>
    /// <returns>A configured token service instance.</returns>
    private static JwtTokenService CreateService ( JwtOptions options )
        {
        return new JwtTokenService(Options.Create(options));
        }

    /// <summary>
    /// Creates valid JWT options for unit tests.
    /// </summary>
    /// <returns>A valid JWT options instance.</returns>
    private static JwtOptions CreateValidOptions ()
        {
        return new JwtOptions
            {
            Issuer = ValidIssuer,
            Audience = ValidAudience,
            SigningKey = ValidSigningKey,
            ExpirationMinutes = ValidExpirationMinutes
            };
        }

    /// <summary>
    /// Reads a JWT string into a <see cref="JwtSecurityToken" /> instance.
    /// </summary>
    /// <param name="token">The serialized JWT access token.</param>
    /// <returns>The parsed JWT token.</returns>
    private static JwtSecurityToken ReadJwtToken ( string token )
        {
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
    }
