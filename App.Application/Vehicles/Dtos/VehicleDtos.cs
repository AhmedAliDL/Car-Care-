namespace App.Application.Vehicles.Dtos;

/// <summary>
/// Represents a vehicle as returned by the API.
/// </summary>
public sealed class VehicleResponse
{
    /// <summary>The vehicle id.</summary>
    public Guid Id { get; set; }

    /// <summary>The id of the customer who owns the vehicle.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The vehicle's plate number.</summary>
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>The 17-character Vehicle Identification Number.</summary>
    public string Vin { get; set; } = string.Empty;

    /// <summary>The vehicle make (e.g. Toyota).</summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>The vehicle model (e.g. Corolla).</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>The manufacturing year.</summary>
    public int Year { get; set; }

    /// <summary>When the vehicle was added (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// A single entry in a vehicle's service history.
/// </summary>
public sealed class VehicleServiceHistoryItemResponse
{
    /// <summary>The id of the appointment this history entry refers to.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>The scheduled date of the appointment (UTC).</summary>
    public DateTime AppointmentDate { get; set; }

    /// <summary>The appointment status at the time of reading.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>The total price of the appointment's services.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>The services performed in this appointment, with snapshotted prices.</summary>
    public IReadOnlyList<AppointmentServiceSnapshotResponse> Services { get; set; } = [];
}

/// <summary>
/// A point-in-time snapshot of a service line within an appointment.
/// </summary>
/// <remarks>
/// Name, price and duration are captured when the appointment is booked, so later catalog changes
/// do not alter historical appointments.
/// </remarks>
public sealed class AppointmentServiceSnapshotResponse
{
    /// <summary>The catalog id of the service.</summary>
    public Guid ServiceId { get; set; }

    /// <summary>The service name at the time of booking.</summary>
    public string? ServiceName { get; set; }

    /// <summary>The unit price snapshotted at booking time.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>The service duration in minutes at the time of booking.</summary>
    public int DurationMinutes { get; set; }
}
