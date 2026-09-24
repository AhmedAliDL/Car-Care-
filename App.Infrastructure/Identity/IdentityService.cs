using App.Application.Abstractions.Identity;
using App.Application.Common.Exceptions;
using App.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is not null;
    }

    public async Task<IdentityUserSnapshot> CreateUserAsync(
        string email,
        string phoneNumber,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = phoneNumber,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
            if (createResult.Errors.Any(e => e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("Email Already Registered", "Email address is already registered.");

            throw new BadRequestException("Identity Validation Failed", errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
            throw new BadRequestException("Role Assignment Failed", errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new IdentityUserSnapshot(user.Id, user.Email!, user.PhoneNumber, roles.ToList());
    }

    public async Task<IdentityUserSnapshot?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        if (!await _userManager.CheckPasswordAsync(user, password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new IdentityUserSnapshot(user.Id, user.Email!, user.PhoneNumber, roles.ToList());
    }

    public async Task<IdentityUserSnapshot?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new IdentityUserSnapshot(user.Id, user.Email!, user.PhoneNumber, roles.ToList());
    }
}
