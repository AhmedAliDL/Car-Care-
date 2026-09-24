using App.Domain.Entities;

namespace App.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    IGenericRepository<Appointment> Appointments { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<AppointmentService> AppointmentServices { get; }
    IGenericRepository<ServiceOffering> ServiceOfferings { get; }
    IGenericRepository<Vehicle> Vehicles { get; }
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
