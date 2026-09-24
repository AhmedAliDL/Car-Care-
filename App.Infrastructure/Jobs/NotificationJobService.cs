using App.Domain.Enums;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Jobs;

public interface INotificationJobService
{
    Task NotifyStaffOnAppointmentCreatedAsync(Guid appointmentId);
    Task NotifyCustomerOnStatusChangedAsync(Guid appointmentId, AppointmentStatus previousStatus, AppointmentStatus newStatus);
}

public sealed class NotificationJobService : INotificationJobService
{
    private readonly CarCareDbContext _db;
    private readonly ILogger<NotificationJobService> _logger;

    public NotificationJobService(CarCareDbContext db, ILogger<NotificationJobService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task NotifyStaffOnAppointmentCreatedAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment is null)
            {
                _logger.LogWarning("Staff notification skipped; appointment {AppointmentId} was not found.", appointmentId);
                return;
            }

            _logger.LogInformation(
                "Staff notification: appointment {AppointmentId} created for customer {CustomerId} vehicle {VehicleId} at {AppointmentDate:u}.",
                appointment.Id,
                appointment.CustomerId,
                appointment.VehicleId,
                appointment.AppointmentDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Staff notification job failed for appointment {AppointmentId}.", appointmentId);
            throw;
        }
    }

    public async Task NotifyCustomerOnStatusChangedAsync(
        Guid appointmentId,
        AppointmentStatus previousStatus,
        AppointmentStatus newStatus)
    {
        try
        {
            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment is null)
            {
                _logger.LogWarning("Customer notification skipped; appointment {AppointmentId} was not found.", appointmentId);
                return;
            }

            _logger.LogInformation(
                "Customer notification: appointment {AppointmentId} for customer {CustomerId} changed from {PreviousStatus} to {NewStatus}.",
                appointment.Id,
                appointment.CustomerId,
                previousStatus,
                newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer notification job failed for appointment {AppointmentId}.", appointmentId);
            throw;
        }
    }
}
