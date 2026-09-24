using App.Application.Abstractions.Jobs;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Abstractions.Time;
using App.Application.Appointments.Dtos;
using App.Application.Common;
using App.Application.Common.Exceptions;
using App.Application.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVehicleScheduleLock _vehicleLock;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IBackgroundJobQueue _jobs;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IVehicleScheduleLock vehicleLock,
        ICurrentUser currentUser,
        IClock clock,
        IBackgroundJobQueue jobs,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _vehicleLock = vehicleLock;
        _currentUser = currentUser;
        _clock = clock;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<AppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetRequiredCustomerAsync(_currentUser, cancellationToken);
        Appointment? created = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _vehicleLock.AcquireAsync(request.VehicleId, ct);

            var vehicle = await _unitOfWork.Vehicles.Query().FirstOrDefaultAsync(v => v.Id == request.VehicleId, ct)
                          ?? throw new NotFoundException("Vehicle ID does not exist.");

            if (vehicle.CustomerId != customer.Id)
                throw new ForbiddenException("The vehicle does not belong to the authenticated customer.");

            var services = await _unitOfWork.ServiceOfferings
                .Query()
                .Where(s => request.ServiceIds.Contains(s.Id))
                .ToListAsync(ct);

            if (services.Count != request.ServiceIds.Distinct().Count())
                throw new BadRequestException("Invalid Service", "One or more selected services were not found.");

            var inactive = services.Where(s => !s.IsActive).Select(s => s.Name).ToList();
            if (inactive.Count > 0)
            {
                throw new BadRequestException(
                    "Inactive Service",
                    $"Appointments can only be booked with active services. Inactive: {string.Join(", ", inactive)}.");
            }

            if (request.AppointmentDate <= _clock.UtcNow)
            {
                throw new BadRequestException(
                    "Invalid Appointment Date",
                    "Appointment date must be strictly in the future relative to server UTC time.");
            }

            var totalDuration = services.Sum(s => s.DurationMinutes);
            var newStart = request.AppointmentDate;
            var newEnd = newStart.AddMinutes(totalDuration);

            var existing = await _unitOfWork.Appointments
                .Query()
                .Where(a => a.VehicleId == request.VehicleId
                            && AppointmentStatusMachine.ActiveConflictStatuses.Contains(a.Status))
                .Select(a => new
                {
                    a.AppointmentDate,
                    Duration = a.Services.Sum(s => s.DurationMinutes)
                })
                .ToListAsync(ct);

            var hasConflict = existing.Any(e =>
                AppointmentStatusMachine.Overlaps(e.AppointmentDate, e.Duration, newStart, totalDuration));

            if (hasConflict)
            {
                throw new ConflictException(
                    "Booking Conflict",
                    "The vehicle already has an active appointment during the requested time window.");
            }

            created = new Appointment
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                AppointmentDate = request.AppointmentDate,
                Status = AppointmentStatus.Requested,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                CreatedAt = _clock.UtcNow
            };

            foreach (var service in services)
            {
                created.Services.Add(new AppointmentService
                {
                    AppointmentId = created.Id,
                    ServiceId = service.Id,
                    UnitPrice = service.BasePrice,
                    DurationMinutes = service.DurationMinutes
                });
            }

            await _unitOfWork.Appointments.AddAsync(created, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        EnqueueSafely(() => _jobs.EnqueueStaffAppointmentCreatedNotification(created!.Id));
        return await LoadResponseAsync(created!.Id, cancellationToken);
    }

    private void EnqueueSafely(Action enqueue)
    {
        try
        {
            enqueue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue a background notification job after a successful appointment transaction.");
        }
    }

    private async Task<AppointmentResponse> LoadResponseAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments
            .QueryNoTracking()
            .Include(a => a.Services)
            .ThenInclude(s => s.Service)
            .FirstAsync(a => a.Id == id, cancellationToken);

        return AppointmentMapper.Map(appointment);
    }
}
