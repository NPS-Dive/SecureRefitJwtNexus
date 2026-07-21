using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Features.Authentication.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ListMaker.Api.Infrastructure.Authentication;

/// <summary>
/// Creates signed JWT access tokens for authenticated ListMaker API clients.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
    {
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtOptions">The configured JWT options.</param>
    public JwtTokenService ( IOptions<JwtOptions> jwtOptions )
        {
        _jwtOptions = jwtOptions.Value;
        }

    /// <inheritdoc />
    public string CreateAccessToken ( string username )
        {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        ValidateOptions();

        byte[] signingKeyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);

        var securityKey = new SymmetricSecurityKey(signingKeyBytes);

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        DateTime utcNow = DateTime.UtcNow;
        DateTime expiresAtUtc = utcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
        }

    /// <summary>
    /// Validates the configured JWT options before creating a token.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required JWT configuration values are missing or invalid.
    /// </exception>
    private void ValidateOptions ()
        {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer))
            {
            throw new InvalidOperationException("JWT issuer is not configured.");
            }

        if (string.IsNullOrWhiteSpace(_jwtOptions.Audience))
            {
            throw new InvalidOperationException("JWT audience is not configured.");
            }

        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
            {
            throw new InvalidOperationException("JWT signing key is not configured.");
            }

        if (_jwtOptions.SigningKey.Length < 32)
            {
            throw new InvalidOperationException(
                "JWT signing key must be at least 32 characters long for HMAC SHA-256.");
            }

        if (_jwtOptions.ExpirationMinutes <= 0)
            {
            throw new InvalidOperationException(
                "JWT expiration minutes must be greater than zero.");
            }
        }
    }
