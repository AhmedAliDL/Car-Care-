using App.Application.Abstractions.Time;
using App.Domain.Enums;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Jobs;

public interface IStaleAppointmentCleanupJob
{
    Task ExecuteAsync();
}

public sealed class StaleAppointmentCleanupJob : IStaleAppointmentCleanupJob
{
    private readonly CarCareDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<StaleAppointmentCleanupJob> _logger;

    public StaleAppointmentCleanupJob(
        CarCareDbContext db,
        IClock clock,
        ILogger<StaleAppointmentCleanupJob> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var cutoff = _clock.UtcNow.AddHours(-24);
        var stale = await _db.Appointments
            .Where(a => a.Status == AppointmentStatus.Requested && a.CreatedAt < cutoff)
            .ToListAsync();

        if (stale.Count == 0)
        {
            _logger.LogInformation("Stale appointment cleanup found no Requested appointments older than 24 hours.");
            return;
        }

        var now = _clock.UtcNow;
        foreach (var appointment in stale)
        {
            appointment.CancelAsStale(now);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation(
            "Stale appointment cleanup cancelled {Count} unconfirmed appointments at {UtcNow:u}.",
            stale.Count,
            now);
    }
}
