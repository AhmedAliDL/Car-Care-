using App.Application.Abstractions.Identity;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Auth.Dtos;
using App.Application.Common.Exceptions;
using App.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Auth.Queries;

public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileResponse>
{
    private readonly IIdentityService _identity;
    private readonly IGenericRepository<Customer> _customers;
    private readonly ICurrentUser _currentUser;

    public GetProfileQueryHandler(
        IIdentityService identity,
        IGenericRepository<Customer> customers,
        ICurrentUser currentUser)
    {
        _identity = identity;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _identity.GetByIdAsync(_currentUser.UserId, cancellationToken)
                   ?? throw new NotFoundException("User profile was not found.");

        var customer = await _customers
            .QueryNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

        return new ProfileResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Role = user.Roles.FirstOrDefault() ?? string.Empty,
            CustomerId = customer?.Id,
            Name = customer?.Name
        };
    }
}
