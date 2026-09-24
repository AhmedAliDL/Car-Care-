using App.Application.Abstractions.Identity;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Time;
using App.Application.Auth.Dtos;
using App.Application.Common.Exceptions;
using App.Domain.Constants;
using App.Domain.Entities;
using MediatR;

namespace App.Application.Auth.Commands.Register;

public sealed class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, RegisterResponse>
{
    private readonly IIdentityService _identity;
    private readonly IGenericRepository<Customer> _customers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegisterCustomerCommandHandler(
        IIdentityService identity,
        IGenericRepository<Customer> customers,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _identity = identity;
        _customers = customers;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<RegisterResponse> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _identity.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("Email Already Registered", "Email address is already registered.");
        }

        RegisterResponse? response = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var user = await _identity.CreateUserAsync(
                request.Email.Trim(),
                request.Phone.Trim(),
                request.Password,
                Roles.Customer,
                ct);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = request.Name.Trim(),
                CreatedAt = _clock.UtcNow
            };

            await _customers.AddAsync(customer, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            response = new RegisterResponse
            {
                UserId = user.Id,
                CustomerId = customer.Id,
                Email = user.Email,
                Name = customer.Name,
                Role = Roles.Customer
            };
        }, cancellationToken);

        return response!;
    }
}
