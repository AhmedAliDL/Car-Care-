using App.Application.Appointments.Dtos;
using MediatR;

namespace App.Application.Appointments.Commands.CreateAppointment;

/// <summary>
/// Request to book an appointment for a vehicle.
/// </summary>
public sealed record CreateAppointmentCommand : IRequest<AppointmentResponse>
{
    /// <summary>The id of the vehicle to book; must belong to the authenticated customer.</summary>
    public Guid VehicleId { get; set; }

    /// <summary>The catalog ids of the services to include; must be active and non-duplicate.</summary>
    public List<Guid> ServiceIds { get; set; } = [];

    /// <summary>The desired start of the appointment (UTC); must be in the future and free of overlaps.</summary>
    public DateTime AppointmentDate { get; set; }

    /// <summary>Optional notes for the garage.</summary>
    public string? Notes { get; set; }
}
