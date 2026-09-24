namespace App.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
