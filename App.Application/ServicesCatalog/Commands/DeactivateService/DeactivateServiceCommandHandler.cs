using App.Application.Abstractions.Persistence;
using App.Application.Common.Exceptions;
using App.Application.ServicesCatalog.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.ServicesCatalog.Commands.DeactivateService;

public sealed class DeactivateServiceCommandHandler : IRequestHandler<DeactivateServiceCommand, ServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateServiceCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> Handle(DeactivateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ServiceOfferings
                         .Query()
                         .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Service ID not found.");

        entity.IsActive = false;

        _unitOfWork.ServiceOfferings.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceMapper.Map(entity);
    }
}
