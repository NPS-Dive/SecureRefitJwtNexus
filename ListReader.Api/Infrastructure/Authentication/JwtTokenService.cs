using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.Authentication.Configuration;
using ListReader.Api.Features.Authentication.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ListReader.Api.Infrastructure.Authentication;

/// <summary>
/// Generates JWT access tokens for ListReader.Api callers.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
    {
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtOptions">
    /// The configured JWT options.
    /// </param>
    public JwtTokenService ( IOptions<JwtOptions> jwtOptions )
        {
        _jwtOptions = jwtOptions.Value;
        }

    /// <inheritdoc />
    public LoginResponse GenerateToken ( string username )
    {
        DateTimeOffset issuedAtUtc = DateTimeOffset.UtcNow;
        DateTimeOffset expiresAtUtc = issuedAtUtc.AddMinutes(_jwtOptions.ExpirationMinutes);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        ];

        SymmetricSecurityKey signingKey =
            new(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        SigningCredentials signingCredentials =
            new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtToken = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            ExpiresInSeconds = (int)TimeSpan.FromMinutes(_jwtOptions.ExpirationMinutes).TotalSeconds
        };
    }

    }
