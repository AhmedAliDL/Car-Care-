using App.Application.Vehicles.Dtos;
using MediatR;

namespace App.Application.Vehicles.Queries.GetVehicleServiceHistory;

public sealed record GetVehicleServiceHistoryQuery : IRequest<IReadOnlyList<VehicleServiceHistoryItemResponse>>
{
    public Guid VehicleId { get; set; }
}
