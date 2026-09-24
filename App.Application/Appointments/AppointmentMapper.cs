using App.Application.Appointments.Dtos;
using App.Application.Vehicles.Dtos;
using App.Domain.Entities;

namespace App.Application.Appointments;

internal static class AppointmentMapper
{
    public static AppointmentResponse Map(Appointment appointment)
    {
        var duration = appointment.Services.Sum(s => s.DurationMinutes);
        return new AppointmentResponse
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            VehicleId = appointment.VehicleId,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentEnd = appointment.AppointmentDate.AddMinutes(duration),
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt,
            CancelledAt = appointment.CancelledAt,
            TotalPrice = appointment.Services.Sum(s => s.UnitPrice),
            TotalDurationMinutes = duration,
            Services = appointment.Services.Select(s => new AppointmentServiceSnapshotResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Service?.Name,
                UnitPrice = s.UnitPrice,
                DurationMinutes = s.DurationMinutes
            }).ToList()
        };
    }
}
