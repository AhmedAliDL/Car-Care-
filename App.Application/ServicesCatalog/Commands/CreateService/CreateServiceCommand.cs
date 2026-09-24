using App.Application.ServicesCatalog.Dtos;
using MediatR;

namespace App.Application.ServicesCatalog.Commands.CreateService;

/// <summary>
/// Request to create a new catalog service (Manager only).
/// </summary>
public sealed record CreateServiceCommand : IRequest<ServiceResponse>
{
    /// <summary>The service name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>An optional description of what the service includes.</summary>
    public string? Description { get; set; }

    /// <summary>How long the service takes, in minutes; must be positive.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>The base price charged for the service; must be non-negative.</summary>
    public decimal BasePrice { get; set; }
}
