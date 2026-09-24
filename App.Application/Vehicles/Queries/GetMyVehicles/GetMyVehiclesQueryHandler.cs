using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Common;
using App.Application.Vehicles.Dtos;
using App.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Vehicles.Queries.GetMyVehicles;

public sealed class GetMyVehiclesQueryHandler : IRequestHandler<GetMyVehiclesQuery, IReadOnlyList<VehicleResponse>>
{
    private readonly IGenericRepository<Vehicle> _vehicles;
    private readonly IGenericRepository<Customer> _customers;
    private readonly ICurrentUser _currentUser;

    public GetMyVehiclesQueryHandler(
        IGenericRepository<Vehicle> vehicles,
        IGenericRepository<Customer> customers,
        ICurrentUser currentUser)
    {
        _vehicles = vehicles;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VehicleResponse>> Handle(GetMyVehiclesQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetRequiredCustomerAsync(_currentUser, cancellationToken);

        var vehicles = await _vehicles
            .QueryNoTracking()
            .Where(v => v.CustomerId == customer.Id)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return vehicles.Select(VehicleHandlerHelpers.Map).ToList();
    }
}
