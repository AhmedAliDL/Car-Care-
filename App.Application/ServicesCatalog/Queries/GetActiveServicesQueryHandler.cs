using App.Application.Abstractions.Persistence;
using App.Application.ServicesCatalog.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.ServicesCatalog.Queries;

public sealed class GetActiveServicesQueryHandler : IRequestHandler<GetActiveServicesQuery, IReadOnlyList<ServiceResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveServicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ServiceResponse>> Handle(GetActiveServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _unitOfWork.ServiceOfferings
            .QueryNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return services.Select(ServiceMapper.Map).ToList();
    }
}
