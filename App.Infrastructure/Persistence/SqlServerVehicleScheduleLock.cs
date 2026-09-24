using App.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public sealed class SqlServerVehicleScheduleLock : IVehicleScheduleLock
{
    private readonly CarCareDbContext _db;

    public SqlServerVehicleScheduleLock(CarCareDbContext db)
    {
        _db = db;
    }

    public async Task AcquireAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsSqlServer())
        {
            await _db.Vehicles
                .FromSqlInterpolated($"SELECT * FROM Vehicles WITH (UPDLOCK, ROWLOCK, HOLDLOCK) WHERE Id = {vehicleId}")
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            return;
        }

        await _db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken);
    }
}
