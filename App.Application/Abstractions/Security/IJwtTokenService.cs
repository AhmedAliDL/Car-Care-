namespace App.Application.Abstractions.Security;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(Guid userId, string email, string role);
}
