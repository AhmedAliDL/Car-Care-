using App.Application.Abstractions.Identity;
using App.Application.Abstractions.Security;
using App.Application.Auth.Dtos;
using App.Application.Common.Exceptions;
using App.Domain.Constants;
using MediatR;

namespace App.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identity;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(IIdentityService identity, IJwtTokenService jwt)
    {
        _identity = identity;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _identity.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
            throw new UnauthorizedAppException("Invalid credentials.");

        var role = user.Roles.FirstOrDefault() ?? Roles.Customer;
        var token = _jwt.CreateToken(user.Id, user.Email, role);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = token.ExpiresAtUtc,
            UserId = user.Id,
            Role = role
        };
    }
}
