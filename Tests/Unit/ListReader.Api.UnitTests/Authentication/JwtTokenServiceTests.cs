using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.Authentication.Configuration;
using ListReader.Api.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace ListReader.Api.UnitTests.Authentication;

/// <summary>
/// Contains unit tests for <see cref="JwtTokenService"/>.
/// </summary>
[TestFixture]
public sealed class JwtTokenServiceTests
    {
    /// <summary>
    /// Verifies that GenerateToken returns a successful login response
    /// with a non-empty JWT access token and expected token type.
    /// </summary>
    [Test]
    public void GenerateToken_WithValidUsername_ShouldReturnLoginResponseWithBearerToken ()
        {
        // Arrange
        JwtOptions jwtOptions = new()
            {
            Issuer = "ListReader.Api",
            Audience = "ListReader.Api.Clients",
            SecretKey = "@ListReader.Api.SuperSecretKey.For.Jwt.Signing.123456@6789_10",
            ExpirationMinutes = 60
            };

        JwtTokenService sut = new(Options.Create(jwtOptions));

        // Act
        LoginResponse result = sut.GenerateToken("reader@admin");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresInSeconds.Should().Be(3600);
        result.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        }

    /// <summary>
    /// Verifies that the generated JWT contains the expected username claims.
    /// </summary>
    [Test]
    public void GenerateToken_WithValidUsername_ShouldContainExpectedSubjectAndUniqueNameClaims ()
        {
        // Arrange
        JwtOptions jwtOptions = new()
            {
            Issuer = "ListReader.Api",
            Audience = "ListReader.Api.Clients",
            SecretKey = "@ListReader.Api.SuperSecretKey.For.Jwt.Signing.123456@6789_10",
            ExpirationMinutes = 60
            };

        JwtTokenService sut = new(Options.Create(jwtOptions));
        JwtSecurityTokenHandler tokenHandler = new();

        // Act
        LoginResponse result = sut.GenerateToken("reader@admin");
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        jwtToken.Claims.Should().Contain(
            claim => claim.Type == JwtRegisteredClaimNames.Sub &&
                     claim.Value == "reader@admin");

        jwtToken.Claims.Should().Contain(
            claim => claim.Type == JwtRegisteredClaimNames.UniqueName &&
                     claim.Value == "reader@admin");

        jwtToken.Claims.Should().Contain(
            claim => claim.Type == JwtRegisteredClaimNames.Jti &&
                     !string.IsNullOrWhiteSpace(claim.Value));
        }

    /// <summary>
    /// Verifies that the generated JWT contains the expected issuer and audience.
    /// </summary>
    [Test]
    public void GenerateToken_WithValidUsername_ShouldContainExpectedIssuerAndAudience ()
        {
        // Arrange
        JwtOptions jwtOptions = new()
            {
            Issuer = "ListReader.Api",
            Audience = "ListReader.Api.Clients",
            SecretKey = "@ListReader.Api.SuperSecretKey.For.Jwt.Signing.123456@6789_10",
            ExpirationMinutes = 60
            };

        JwtTokenService sut = new(Options.Create(jwtOptions));
        JwtSecurityTokenHandler tokenHandler = new();

        // Act
        LoginResponse result = sut.GenerateToken("reader@admin");
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        jwtToken.Issuer.Should().Be("ListReader.Api");
        jwtToken.Audiences.Should().ContainSingle()
            .Which.Should().Be("ListReader.Api.Clients");
        }

    /// <summary>
    /// Verifies that the generated JWT expiration aligns with configured lifetime.
    /// </summary>
    [Test]
    public void GenerateToken_WithValidUsername_ShouldSetExpirationCloseToConfiguredLifetime ()
        {
        // Arrange
        JwtOptions jwtOptions = new()
            {
            Issuer = "ListReader.Api",
            Audience = "ListReader.Api.Clients",
            SecretKey = "@ListReader.Api.SuperSecretKey.For.Jwt.Signing.123456@6789_10",
            ExpirationMinutes = 60
            };

        JwtTokenService sut = new(Options.Create(jwtOptions));
        DateTimeOffset beforeGenerationUtc = DateTimeOffset.UtcNow;

        // Act
        LoginResponse result = sut.GenerateToken("reader@admin");

        // Assert
        result.ExpiresAtUtc.Should().BeOnOrAfter(beforeGenerationUtc.AddMinutes(59));
        result.ExpiresAtUtc.Should().BeOnOrBefore(beforeGenerationUtc.AddMinutes(61));
        }
    }
