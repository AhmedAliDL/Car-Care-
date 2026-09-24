using App.Application.Vehicles.Dtos;
using MediatR;

namespace App.Application.Vehicles.Queries.GetMyVehicles;

public sealed record GetMyVehiclesQuery : IRequest<IReadOnlyList<VehicleResponse>>;