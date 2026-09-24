using App.Application.Vehicles.Dtos;
using MediatR;

namespace App.Application.Vehicles.Commands.CreateVehicle;

/// <summary>
/// Request to add a vehicle to the authenticated customer's garage.
/// </summary>
public sealed record CreateVehicleCommand : IRequest<VehicleResponse>
{
    /// <summary>The vehicle's plate number.</summary>
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>The Vehicle Identification Number; 17 characters, excluding I, O and Q.</summary>
    public string Vin { get; set; } = string.Empty;

    /// <summary>The vehicle make (e.g. Toyota).</summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>The vehicle model (e.g. Corolla).</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>The manufacturing year.</summary>
    public int Year { get; set; }
}
