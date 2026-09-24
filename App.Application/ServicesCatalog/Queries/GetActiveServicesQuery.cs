using App.Application.ServicesCatalog.Dtos;
using MediatR;

namespace App.Application.ServicesCatalog.Queries;

public sealed record GetActiveServicesQuery : IRequest<IReadOnlyList<ServiceResponse>>;
