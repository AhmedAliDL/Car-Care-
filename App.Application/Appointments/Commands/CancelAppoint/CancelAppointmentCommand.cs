using App.Application.Appointments.Dtos;
using MediatR;

namespace App.Application.Appointments.Commands.CancelAppoint;

public sealed record CancelAppointmentCommand : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }
}
