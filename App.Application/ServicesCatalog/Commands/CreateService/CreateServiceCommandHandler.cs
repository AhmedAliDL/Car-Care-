using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Time;
using App.Application.ServicesCatalog.Dtos;
using App.Domain.Entities;
using MediatR;

namespace App.Application.ServicesCatalog.Commands.CreateService;

public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<ServiceResponse> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = new ServiceOffering
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            BasePrice = request.BasePrice,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        await _unitOfWork.ServiceOfferings.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceMapper.Map(entity);
    }
}
