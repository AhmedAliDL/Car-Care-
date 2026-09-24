using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Common.Exceptions;
using App.Application.Vehicles.Dtos;
using App.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Vehicles.Queries.GetVehicleById;

public sealed class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleResponse>
{
    private readonly IGenericRepository<Vehicle> _vehicles;
    private readonly IGenericRepository<Customer> _customers;
    private readonly ICurrentUser _currentUser;

    public GetVehicleByIdQueryHandler(
        IGenericRepository<Vehicle> vehicles,
        IGenericRepository<Customer> customers,
        ICurrentUser currentUser)
    {
        _vehicles = vehicles;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task<VehicleResponse> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles
                          .QueryNoTracking()
                          .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException("Vehicle ID does not exist.");

        await VehicleHandlerHelpers.EnsureCanAccessAsync(_currentUser, _customers, vehicle, cancellationToken);
        return VehicleHandlerHelpers.Map(vehicle);
    }
}
