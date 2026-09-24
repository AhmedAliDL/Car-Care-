using App.Application.Appointments.Dtos;
using App.Domain.Enums;
using MediatR;

namespace App.Application.Appointments.Commands.UpdateAppointmentStatus;

/// <summary>
/// Request to transition an appointment to a new status.
/// </summary>
public sealed record UpdateAppointmentStatusCommand : IRequest<AppointmentResponse>
{
    /// <summary>The appointment id; set from the route and overrides any value supplied in the body.</summary>
    public Guid Id { get; set; }

    /// <summary>The target status; the transition must be allowed by the appointment state machine.</summary>
    public AppointmentStatus NewStatus { get; set; }
}
