using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Common;
using App.Application.Common.Exceptions;
using App.Application.Vehicles.Dtos;
using App.Domain.Constants;
using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Vehicles;

internal static class VehicleHandlerHelpers
{
    public static VehicleResponse Map(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        CustomerId = vehicle.CustomerId,
        PlateNumber = vehicle.PlateNumber,
        Vin = vehicle.Vin,
        Make = vehicle.Make,
        Model = vehicle.Model,
        Year = vehicle.Year,
        CreatedAt = vehicle.CreatedAt
    };

    public static async Task EnsureCanAccessAsync(
        ICurrentUser currentUser,
        IGenericRepository<Customer> customers,
        Vehicle vehicle,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsStaffOrManager())
            return;

        if (!currentUser.IsInRole(Roles.Customer))
            throw new ForbiddenException("You are not allowed to access this vehicle.");

        var customerId = await customers
            .QueryNoTracking()
            .Where(c => c.UserId == currentUser.UserId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId is null || vehicle.CustomerId != customerId)
            throw new ForbiddenException("You are not allowed to access another customer's vehicle.");
    }
}
