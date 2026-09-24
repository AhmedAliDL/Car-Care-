namespace App.Application.Abstractions.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsInRole(string role);
}
