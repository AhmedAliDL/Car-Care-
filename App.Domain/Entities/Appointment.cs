using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;

namespace App.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Customer? Customer { get; set; }
    public Vehicle? Vehicle { get; set; }
    public ICollection<AppointmentService> Services { get; set; } = new List<AppointmentService>();

    public int TotalDurationMinutes => Services.Sum(s => s.DurationMinutes);

    public void TransitionTo(AppointmentStatus newStatus, DateTime utcNow)
    {
        AppointmentStatusMachine.EnsureCanTransition(Status, newStatus);
        ApplyStatus(newStatus, utcNow);
    }

    public void CancelByCustomer(DateTime utcNow)
    {
        if (Status is AppointmentStatus.InService or AppointmentStatus.Completed)
        {
            throw new DomainException(
                "Cancellation Not Allowed",
                "In-service or completed appointments cannot be cancelled.");
        }

        if (!AppointmentStatusMachine.IsCustomerCancellable(Status))
        {
            throw new DomainException(
                "Cancellation Not Allowed",
                $"Appointments with status '{Status}' cannot be cancelled by the customer.");
        }

        TransitionTo(AppointmentStatus.Cancelled, utcNow);
    }

    public void CancelAsStale(DateTime utcNow)
    {
        if (Status != AppointmentStatus.Requested)
            return;

        Status = AppointmentStatus.Cancelled;
        CancelledAt = utcNow;
        UpdatedAt = utcNow;
    }

    private void ApplyStatus(AppointmentStatus newStatus, DateTime utcNow)
    {
        Status = newStatus;
        UpdatedAt = utcNow;
        if (newStatus == AppointmentStatus.Cancelled)
            CancelledAt = utcNow;
    }
}
