using App.Application.Abstractions.Jobs;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Abstractions.Time;
using App.Application.Appointments.Dtos;
using App.Application.Common;
using App.Application.Common.Exceptions;
using App.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Application.Appointments.Commands.CancelAppoint;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, AppointmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IBackgroundJobQueue _jobs;
    private readonly ILogger<CancelAppointmentCommandHandler> _logger;

    public CancelAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IClock clock,
        IBackgroundJobQueue jobs,
        ILogger<CancelAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<AppointmentResponse> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetRequiredCustomerAsync(_currentUser, cancellationToken);

        var appointment = await _unitOfWork.Appointments
                              .Query()
                              .Include(a => a.Services)
                              .ThenInclude(s => s.Service)
                              .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
                          ?? throw new NotFoundException("Appointment was not found.");

        if (appointment.CustomerId != customer.Id)
            throw new ForbiddenException("You can only cancel appointments associated with your own account.");

        var previous = appointment.Status;
        try
        {
            appointment.CancelByCustomer(_clock.UtcNow);
        }
        catch (DomainException ex)
        {
            throw new BadRequestException(ex.Title, ex.Message);
        }

        _unitOfWork.Appointments.Update(appointment);
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
