using App.Application.Abstractions.Jobs;
using App.Domain.Enums;

namespace App.Infrastructure.Jobs;

/// <summary>
/// Keeps appointment workflows available when background processing is disabled.
/// </summary>
public sealed class NoOpBackgroundJobQueue : IBackgroundJobQueue
{
    public void EnqueueStaffAppointmentCreatedNotification(Guid appointmentId)
    {
    }

    public void EnqueueCustomerStatusChangedNotification(
        Guid appointmentId,
        AppointmentStatus previousStatus,
        AppointmentStatus newStatus)
    {
    }
}
