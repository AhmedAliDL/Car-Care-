using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Appointments.Dtos;
using App.Application.Common;
using App.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Appointments.Queries;

public sealed class GetMyAppointmentsQueryHandler : IRequestHandler<GetMyAppointmentsQuery, IReadOnlyList<AppointmentResponse>>
{
    private readonly IGenericRepository<Appointment> _appointments;
    private readonly IGenericRepository<Customer> _customers;
    private readonly ICurrentUser _currentUser;

    public GetMyAppointmentsQueryHandler(
        IGenericRepository<Appointment> appointments,
        IGenericRepository<Customer> customers,
        ICurrentUser currentUser)
    {
        _appointments = appointments;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AppointmentResponse>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetRequiredCustomerAsync(_currentUser, cancellationToken);

        var appointments = await _appointments
            .QueryNoTracking()
            .Where(a => a.CustomerId == customer.Id)
            .Include(a => a.Services)
            .ThenInclude(s => s.Service)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);

        return appointments.Select(AppointmentMapper.Map).ToList();
    }
}
