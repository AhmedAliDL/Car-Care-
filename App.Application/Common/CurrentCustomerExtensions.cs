using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Common.Exceptions;
using App.Domain.Constants;
using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Common;

public static class CurrentCustomerExtensions
{
    public static async Task<Customer> GetRequiredCustomerAsync(
        this IGenericRepository<Customer> customers,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var customer = await customers
            .Query()
            .FirstOrDefaultAsync(c => c.UserId == currentUser.UserId, cancellationToken);

        return customer ?? throw new ForbiddenException("The authenticated user is not linked to a customer profile.");
    }

    public static bool IsStaffOrManager(this ICurrentUser user)
        => user.IsInRole(Roles.Staff) || user.IsInRole(Roles.Manager);
}
