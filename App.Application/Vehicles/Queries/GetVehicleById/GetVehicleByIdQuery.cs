using App.Application.Vehicles.Dtos;
using MediatR;

namespace App.Application.Vehicles.Queries.GetVehicleById;

public sealed record GetVehicleByIdQuery : IRequest<VehicleResponse>
{
    public Guid Id { get; set; }
}
