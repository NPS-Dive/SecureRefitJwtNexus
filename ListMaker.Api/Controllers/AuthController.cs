using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Features.Authentication.Models;
using ListMaker.Api.Features.Authentication.Services;
using ListMaker.Contracts.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace ListMaker.Api.Controllers;

/// <summary>
/// Provides authentication endpoints for obtaining JWT access tokens.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
    {
    private readonly StaticUserOptions _staticUserOptions;
    private readonly JwtOptions _jwtOptions;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="staticUserOptions">The configured static user credentials.</param>
    /// <param name="jwtOptions">The configured JWT options.</param>
    /// <param name="jwtTokenService">The JWT token creation service.</param>
    public AuthController (
        IOptions<StaticUserOptions> staticUserOptions,
        IOptions<JwtOptions> jwtOptions,
        IJwtTokenService jwtTokenService )
        {
        _staticUserOptions = staticUserOptions.Value;
        _jwtOptions = jwtOptions.Value;
        _jwtTokenService = jwtTokenService;
        }

    /// <summary>
    /// Authenticates a static demo user and returns a JWT access token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>A JWT access token when the supplied credentials are valid.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Login and receive a JWT token",
        Description = "Authenticates the configured static user and returns a signed JWT bearer token.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Login succeeded.", typeof(LoginResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Request body is invalid.", typeof(AuthenticationErrorResponse))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Username or password is invalid.", typeof(AuthenticationErrorResponse))]
    public ActionResult<LoginResponse> Login ( [FromBody] LoginRequest request )
        {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            {
            return BadRequest(
                new AuthenticationErrorResponse
                    {
                    Code = "invalid_login_request",
                    Message = "Username and password are required."
                    });
            }

        bool isValidUsername = string.Equals(
            request.Username,
            _staticUserOptions.Username,
            StringComparison.Ordinal);

        bool isValidPassword = string.Equals(
            request.Password,
            _staticUserOptions.Password,
            StringComparison.Ordinal);

        if (!isValidUsername || !isValidPassword)
            {
            return Unauthorized(
                new AuthenticationErrorResponse
                    {
                    Code = "invalid_credentials",
                    Message = "The supplied username or password is invalid."
                    });
            }

        string accessToken = _jwtTokenService.CreateAccessToken(request.Username);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresInSeconds = _jwtOptions.ExpirationMinutes * 60,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            };

        return Ok(response);
        }
    }
