using App.Application.Abstractions.Persistence;
using App.Application.Common.Exceptions;
using App.Application.ServicesCatalog.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.ServicesCatalog.Commands.UpdateService;

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ServiceOfferings
                         .Query()
                         .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Service ID not found.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.DurationMinutes = request.DurationMinutes;
        entity.BasePrice = request.BasePrice;

        _unitOfWork.ServiceOfferings.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceMapper.Map(entity);
    }
}
