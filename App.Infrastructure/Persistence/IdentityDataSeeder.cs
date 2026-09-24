using App.Domain.Constants;
using App.Domain.Entities;
using App.Infrastructure.Identity;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";
    public string ManagerEmail { get; set; } = "manager@carcare.local";
    public string ManagerPassword { get; set; } = string.Empty;
    public string StaffEmail { get; set; } = "staff@carcare.local";
    public string StaffPassword { get; set; } = string.Empty;
}

public sealed class IdentityDataSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CarCareDbContext _db;
    private readonly SeedOptions _options;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        CarCareDbContext db,
        IOptions<SeedOptions> options,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var role in new[] { Roles.Customer, Roles.Staff, Roles.Manager })
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
        }

        await EnsureUserAsync(_options.ManagerEmail, _options.ManagerPassword, Roles.Manager, cancellationToken);
        await EnsureUserAsync(_options.StaffEmail, _options.StaffPassword, Roles.Staff, cancellationToken);

        if (!await _db.Services.AnyAsync(cancellationToken))
        {
            _db.Services.AddRange(
                new ServiceOffering
                {
                    Id = Guid.NewGuid(),
                    Name = "Oil Change",
                    Description = "Standard oil and filter replacement.",
                    DurationMinutes = 30,
                    BasePrice = 50.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ServiceOffering
                {
                    Id = Guid.NewGuid(),
                    Name = "Brake Inspection",
                    Description = "Full brake system inspection.",
                    DurationMinutes = 45,
                    BasePrice = 100.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ServiceOffering
                {
                    Id = Guid.NewGuid(),
                    Name = "Engine Diagnostics",
                    Description = "Computerized engine diagnostics.",
                    DurationMinutes = 60,
                    BasePrice = 120.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureUserAsync(string email, string password, string role, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Skipping seed for role {Role} because email or password is not configured.", role);
            return;
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = "0000000000",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to seed {Role} user: {Errors}", role, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(user, role);
        _logger.LogInformation("Seeded {Role} account {Email}.", role, email);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
