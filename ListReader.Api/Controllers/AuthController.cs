using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.Authentication.Configuration;
using ListReader.Api.Features.Authentication.Models;
using ListReader.Api.Features.Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ListReader.Api.Controllers;

/// <summary>
/// Exposes authentication endpoints for ListReader.Api.
/// </summary>
/// <remarks>
/// This controller authenticates callers against static configured credentials
/// for demo purposes and issues JWT access tokens for ListReader.Api.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
    {
    private readonly StaticUserOptions _staticUserOptions;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="staticUserOptions">
    /// The configured static demo credentials.
    /// </param>
    /// <param name="jwtTokenService">
    /// The JWT token service used to issue ListReader.Api access tokens.
    /// </param>
    public AuthController (
        IOptions<StaticUserOptions> staticUserOptions,
        IJwtTokenService jwtTokenService )
        {
        _staticUserOptions = staticUserOptions.Value;
        _jwtTokenService = jwtTokenService;
        }

    /// <summary>
    /// Authenticates a caller and returns a JWT token for ListReader.Api.
    /// </summary>
    /// <param name="request">
    /// The login request containing username and password.
    /// </param>
    /// <returns>
    /// A JWT login response on success; otherwise 401 Unauthorized.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<LoginResponse> Login ( [FromBody] LoginRequest request )
        {
        if (request is null)
            {
            return BadRequest();
            }

        bool isValidUser =
            string.Equals(request.Username, _staticUserOptions.Username, StringComparison.Ordinal) &&
            string.Equals(request.Password, _staticUserOptions.Password, StringComparison.Ordinal);

        if (!isValidUser)
            {
            return Unauthorized(new AuthenticationErrorResponse
                {
                Error = "invalid_credentials",
                ErrorDescription = "The supplied username or password is incorrect."
                });
            }

        LoginResponse response = _jwtTokenService.GenerateToken(request.Username);

        return Ok(response);
        }
    }
