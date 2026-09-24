using App.Application.ServicesCatalog.Dtos;
using MediatR;

namespace App.Application.ServicesCatalog.Commands.DeactivateService;

public sealed record DeactivateServiceCommand : IRequest<ServiceResponse>
{
    public Guid Id { get; set; }
}
