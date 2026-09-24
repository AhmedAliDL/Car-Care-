namespace App.Application.ServicesCatalog.Dtos;

/// <summary>
/// Represents a catalog service as returned by the API.
/// </summary>
public sealed class ServiceResponse
{
    /// <summary>The service id.</summary>
    public Guid Id { get; set; }

    /// <summary>The service name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>An optional description of what the service includes.</summary>
    public string? Description { get; set; }

    /// <summary>How long the service takes, in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>The current base price charged for the service.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Whether the service can be booked. Deactivated services are hidden from booking.</summary>
    public bool IsActive { get; set; }

    /// <summary>When the service was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
