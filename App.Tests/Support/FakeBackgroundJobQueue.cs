using App.Application.Abstractions.Jobs;
using App.Domain.Enums;

namespace App.Tests.Support;

public sealed class FakeBackgroundJobQueue : IBackgroundJobQueue
{
    public List<Guid> StaffNotifications { get; } = [];
    public List<(Guid AppointmentId, AppointmentStatus Previous, AppointmentStatus Next)> CustomerNotifications { get; } = [];

    public void EnqueueStaffAppointmentCreatedNotification(Guid appointmentId)
        => StaffNotifications.Add(appointmentId);

    public void EnqueueCustomerStatusChangedNotification(Guid appointmentId, AppointmentStatus previousStatus, AppointmentStatus newStatus)
        => CustomerNotifications.Add((appointmentId, previousStatus, newStatus));
}
