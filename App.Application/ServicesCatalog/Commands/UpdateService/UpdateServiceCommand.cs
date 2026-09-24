using App.Application.ServicesCatalog.Dtos;
using MediatR;

namespace App.Application.ServicesCatalog.Commands.UpdateService;

/// <summary>
/// Request to update an existing catalog service (Manager only).
/// </summary>
public sealed record UpdateServiceCommand : IRequest<ServiceResponse>
{
    /// <summary>The service id; set from the route and overrides any value supplied in the body.</summary>
    public Guid Id { get; set; }

    /// <summary>The new service name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The new optional description.</summary>
    public string? Description { get; set; }

    /// <summary>The new duration in minutes; must be positive.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>The new base price; must be non-negative. Existing appointments keep their snapshotted price.</summary>
    public decimal BasePrice { get; set; }
}
