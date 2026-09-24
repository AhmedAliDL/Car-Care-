namespace App.Application.Abstractions.Persistence;

public interface IVehicleScheduleLock
{
    Task AcquireAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
