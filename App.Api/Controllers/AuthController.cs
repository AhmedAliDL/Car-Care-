using App.Application.Auth.Commands.Login;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Dtos;
using App.Application.Auth.Queries;
using App.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace App.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new customer account.
    /// </summary>
    /// <remarks>
    /// Creates a user with the fixed <c>Customer</c> role. Manager and Staff accounts are seeded, not self-registered.
    /// Sample request:
    /// ```
    /// POST /api/auth/register
    /// {
    ///   "name": "John Doe",
    ///   "email": "john@example.com",
    ///   "phone": "+201000000000",
    ///   "password": "P@ssw0rd!"
    /// }
    /// ```
    /// </remarks>
    /// <param name="req">Registration details: name, email, phone and password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created customer's identity and profile summary.</returns>
    /// <response code="201">Customer registered successfully.</response>
    /// <response code="400">Validation failed (e.g. weak password, missing fields).</response>
    /// <response code="409">A user with this email already exists.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerCommand req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(req, cancellationToken);
        return Created($"/api/auth/me", result);
    }

    /// <summary>
    /// Authenticates a user and issues a JWT access token.
    /// </summary>
    /// <remarks>
    /// Returns a bearer token used to authorize all other endpoints. When the authenticated user is a
    /// <c>Manager</c>, an HTTP-only cookie is also set so the Hangfire dashboard can be opened in a browser.
    /// Sample request:
    /// ```
    /// POST /api/auth/login
    /// {
    ///   "email": "manager@carcare.local",
    ///   "password": "Manager123!"
    /// }
    /// ```
    /// </remarks>
    /// <param name="req">Login credentials: email and password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The access token, token type, expiry, user id and role.</returns>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="401">Invalid email or password.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(req, cancellationToken);

        if (result.Role == Roles.Manager)
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                    new Claim(ClaimTypes.Email, req.Email),
                    new Claim(ClaimTypes.Role, result.Role)
                },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets the authenticated user's profile.
    /// </summary>
    /// <remarks>Available to Customer, Staff and Manager roles. Identity is taken from the access token, not the request body.</remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The current user's profile.</returns>
    /// <response code="200">Profile returned.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [Authorize(Roles = $"{Roles.Customer},{Roles.Staff},{Roles.Manager}")]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfileQuery(), cancellationToken);
        return Ok(result);
    }
}
