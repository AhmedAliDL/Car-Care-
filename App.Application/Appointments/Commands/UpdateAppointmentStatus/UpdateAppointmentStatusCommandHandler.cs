using App.Application.Abstractions.Jobs;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Time;
using App.Application.Appointments.Dtos;
using App.Application.Common.Exceptions;
using App.Domain.Entities;
using App.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, AppointmentResponse>
{
    private readonly IGenericRepository<Appointment> _appointments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IBackgroundJobQueue _jobs;
    private readonly ILogger<UpdateAppointmentStatusCommandHandler> _logger;

    public UpdateAppointmentStatusCommandHandler(
        IGenericRepository<Appointment> appointments,
        IUnitOfWork unitOfWork,
        IClock clock,
        IBackgroundJobQueue jobs,
        ILogger<UpdateAppointmentStatusCommandHandler> logger)
    {
        _appointments = appointments;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<AppointmentResponse> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointments
                              .Query()
                              .Include(a => a.Services)
                              .ThenInclude(s => s.Service)
                              .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
                          ?? throw new NotFoundException("Appointment was not found.");

        var previous = appointment.Status;

        try
        {
            appointment.TransitionTo(request.NewStatus, _clock.UtcNow);
        }
        catch (DomainException ex)
        {
            throw new BadRequestException(ex.Title, ex.Message);
        }

        _appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        EnqueueSafely(() => _jobs.EnqueueCustomerStatusChangedNotification(appointment.Id, previous, appointment.Status));
        return AppointmentMapper.Map(appointment);
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
}
