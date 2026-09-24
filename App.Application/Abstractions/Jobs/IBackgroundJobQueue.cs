using App.Domain.Enums;

namespace App.Application.Abstractions.Jobs;

public interface IBackgroundJobQueue
{
    void EnqueueStaffAppointmentCreatedNotification(Guid appointmentId);
    void EnqueueCustomerStatusChangedNotification(Guid appointmentId, AppointmentStatus previousStatus, AppointmentStatus newStatus);
}
