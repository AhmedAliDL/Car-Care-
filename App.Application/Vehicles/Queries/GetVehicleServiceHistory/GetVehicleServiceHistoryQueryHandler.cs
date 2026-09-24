using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Abstractions.Time;
using App.Application.Common.Exceptions;
using App.Application.Vehicles.Dtos;
using App.Domain.Entities;
using App.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Vehicles.Queries.GetVehicleServiceHistory;

public sealed class GetVehicleServiceHistoryQueryHandler
    : IRequestHandler<GetVehicleServiceHistoryQuery, IReadOnlyList<VehicleServiceHistoryItemResponse>>
{
    private readonly IGenericRepository<Vehicle> _vehicles;
    private readonly IGenericRepository<Customer> _customers;
    private readonly IGenericRepository<Appointment> _appointments;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public GetVehicleServiceHistoryQueryHandler(
        IGenericRepository<Vehicle> vehicles,
        IGenericRepository<Customer> customers,
        IGenericRepository<Appointment> appointments,
        ICurrentUser currentUser,
        IClock clock)
    {
        _vehicles = vehicles;
        _customers = customers;
        _appointments = appointments;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<VehicleServiceHistoryItemResponse>> Handle(
        GetVehicleServiceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles
                          .QueryNoTracking()
                          .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
                      ?? throw new NotFoundException("Vehicle ID does not exist.");

        await VehicleHandlerHelpers.EnsureCanAccessAsync(_currentUser, _customers, vehicle, cancellationToken);

        var appointments = await _appointments
            .QueryNoTracking()
            .Where(a => a.VehicleId == request.VehicleId)
            .Where(a => a.Status == AppointmentStatus.Completed
                        || a.Status == AppointmentStatus.Cancelled
                        || a.Status == AppointmentStatus.NoShow
                        || a.AppointmentDate <= _clock.UtcNow)
            .Include(a => a.Services)
            .ThenInclude(s => s.Service)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);

        return appointments.Select(a => new VehicleServiceHistoryItemResponse
        {
            AppointmentId = a.Id,
            AppointmentDate = a.AppointmentDate,
            Status = a.Status.ToString(),
            TotalPrice = a.Services.Sum(s => s.UnitPrice),
            Services = a.Services.Select(s => new AppointmentServiceSnapshotResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Service?.Name,
                UnitPrice = s.UnitPrice,
                DurationMinutes = s.DurationMinutes
            }).ToList()
        }).ToList();
    }
}
