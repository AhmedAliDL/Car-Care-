using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Abstractions.Time;
using App.Application.Common;
using App.Application.Common.Exceptions;
using App.Application.Vehicles.Dtos;
using App.Domain.Entities;
using MediatR;

namespace App.Application.Vehicles.Commands.CreateVehicle;

public sealed class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, VehicleResponse>
{
    private readonly IGenericRepository<Vehicle> _vehicles;
    private readonly IGenericRepository<Customer> _customers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateVehicleCommandHandler(
        IGenericRepository<Vehicle> vehicles,
        IGenericRepository<Customer> customers,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IClock clock)
    {
        _vehicles = vehicles;
        _customers = customers;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<VehicleResponse> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetRequiredCustomerAsync(_currentUser, cancellationToken);
        var vin = request.Vin.Trim().ToUpperInvariant();

        if (await _vehicles.AnyAsync(v => v.Vin == vin, cancellationToken))
        {
            throw new ConflictException("VIN Already Registered", "A vehicle with this VIN already exists.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            PlateNumber = request.PlateNumber.Trim(),
            Vin = vin,
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            CreatedAt = _clock.UtcNow
        };

        await _vehicles.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return VehicleHandlerHelpers.Map(vehicle);
    }
}
