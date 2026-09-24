namespace App.Application.Abstractions.Identity;

public sealed record IdentityUserSnapshot(
    Guid Id,
    string Email,
    string? PhoneNumber,
    IReadOnlyList<string> Roles);

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<IdentityUserSnapshot> CreateUserAsync(
        string email,
        string phoneNumber,
        string password,
        string role,
        CancellationToken cancellationToken = default);

    Task<IdentityUserSnapshot?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<IdentityUserSnapshot?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
