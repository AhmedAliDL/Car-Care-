using App.Application.ServicesCatalog.Dtos;
using App.Domain.Entities;

namespace App.Application.ServicesCatalog;

internal static class ServiceMapper
{
    public static ServiceResponse Map(ServiceOffering entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        DurationMinutes = entity.DurationMinutes,
        BasePrice = entity.BasePrice,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };
}
