using App.Application.Auth.Dtos;
using MediatR;

namespace App.Application.Auth.Commands.Register;

/// <summary>
/// Request to register a new customer account.
/// </summary>
public sealed record RegisterCustomerCommand : IRequest<RegisterResponse>
{
    /// <summary>The customer's full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>A unique email address used as the login identifier.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The customer's contact phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>The account password (minimum 8 chars, upper/lower/digit/symbol).</summary>
    public string Password { get; set; } = string.Empty;
}
