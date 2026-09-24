namespace App.Domain.Entities;

public class AppointmentService
{
    public Guid AppointmentId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal UnitPrice { get; set; }
    public int DurationMinutes { get; set; }

    public Appointment? Appointment { get; set; }
    public ServiceOffering? Service { get; set; }
}
