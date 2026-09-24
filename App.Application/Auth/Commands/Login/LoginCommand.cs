using App.Application.Auth.Dtos;
using MediatR;

namespace App.Application.Auth.Commands.Login;

/// <summary>
/// Request to authenticate and obtain an access token.
/// </summary>
public sealed record LoginCommand : IRequest<AuthResponse>
{
    /// <summary>The account email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The account password.</summary>
    public string Password { get; set; } = string.Empty;
}
