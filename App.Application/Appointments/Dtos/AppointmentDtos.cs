using App.Application.Vehicles.Dtos;

namespace App.Application.Appointments.Dtos;

/// <summary>
/// Represents an appointment as returned by the API, including its snapshotted service lines.
/// </summary>
public sealed class AppointmentResponse
{
    /// <summary>The appointment id.</summary>
    public Guid Id { get; set; }

    /// <summary>The id of the customer who owns the appointment.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The id of the vehicle the appointment is for.</summary>
    public Guid VehicleId { get; set; }

    /// <summary>The scheduled start of the appointment (UTC).</summary>
    public DateTime AppointmentDate { get; set; }

    /// <summary>The scheduled end of the appointment (UTC), derived from the total service duration.</summary>
    public DateTime AppointmentEnd { get; set; }

    /// <summary>The current lifecycle status (Requested, Confirmed, InService, Completed, Cancelled, NoShow).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Optional customer notes captured at booking time.</summary>
    public string? Notes { get; set; }

    /// <summary>When the appointment was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the appointment was last modified (UTC), or null if never modified.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>When the appointment was cancelled (UTC), or null if not cancelled.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>The total price of all services, snapshotted at booking time.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>The combined duration in minutes of all booked services.</summary>
    public int TotalDurationMinutes { get; set; }

    /// <summary>The individual services included in this appointment, with their snapshotted prices.</summary>
    public IReadOnlyList<AppointmentServiceSnapshotResponse> Services { get; set; } = [];
}
