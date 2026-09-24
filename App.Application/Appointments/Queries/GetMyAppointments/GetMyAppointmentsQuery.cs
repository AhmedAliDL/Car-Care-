using App.Application.Appointments.Dtos;
using MediatR;

namespace App.Application.Appointments.Queries;

public sealed record GetMyAppointmentsQuery : IRequest<IReadOnlyList<AppointmentResponse>>;
