using App.Application.Abstractions.Persistence;
using App.Domain.Entities;
using App.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly CarCareDbContext _db;

    public EfUnitOfWork(CarCareDbContext db)
    {
        _db = db;
        Vehicles = new GenericRepository<Vehicle>(_db);
        Appointments = new GenericRepository<Appointment>(_db);
        Customers = new GenericRepository<Customer>(_db);
        AppointmentServices = new GenericRepository<AppointmentService>(_db);
        ServiceOfferings = new GenericRepository<ServiceOffering>(_db);
    }

    public IGenericRepository<Appointment> Appointments { get; private set; }

    public IGenericRepository<Customer> Customers { get; private set; }

    public IGenericRepository<AppointmentService> AppointmentServices { get; private set; }

    public IGenericRepository<ServiceOffering> ServiceOfferings { get; private set; }

    public IGenericRepository<Vehicle> Vehicles { get; private set; }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);
}
