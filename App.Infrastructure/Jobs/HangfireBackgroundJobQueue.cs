using App.Application.Abstractions.Jobs;
using App.Domain.Enums;
using Hangfire;

namespace App.Infrastructure.Jobs;

public sealed class HangfireBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly IBackgroundJobClient _client;

    public HangfireBackgroundJobQueue(IBackgroundJobClient client)
    {
        _client = client;
    }

    public void EnqueueStaffAppointmentCreatedNotification(Guid appointmentId)
    {
        _client.Enqueue<INotificationJobService>(job => job.NotifyStaffOnAppointmentCreatedAsync(appointmentId));
    }

    public void EnqueueCustomerStatusChangedNotification(Guid appointmentId, AppointmentStatus previousStatus, AppointmentStatus newStatus)
    {
        _client.Enqueue<INotificationJobService>(job =>
            job.NotifyCustomerOnStatusChangedAsync(appointmentId, previousStatus, newStatus));
    }
}
