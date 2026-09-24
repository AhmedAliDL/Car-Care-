using App.Domain.Enums;
using App.Domain.Exceptions;

namespace App.Application.Services;

public static class AppointmentStatusMachine
{
    private static readonly IReadOnlyDictionary<AppointmentStatus, AppointmentStatus[]> Allowed =
        new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [AppointmentStatus.Requested] = [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
            [AppointmentStatus.Confirmed] = [AppointmentStatus.InService, AppointmentStatus.Cancelled],
            [AppointmentStatus.InService] = [AppointmentStatus.Completed, AppointmentStatus.NoShow],
            [AppointmentStatus.Completed] = [],
            [AppointmentStatus.Cancelled] = [],
            [AppointmentStatus.NoShow] = []
        };

    public static readonly AppointmentStatus[] ActiveConflictStatuses =
    [
        AppointmentStatus.Requested,
        AppointmentStatus.Confirmed,
        AppointmentStatus.InService
    ];

    public static bool CanTransition(AppointmentStatus from, AppointmentStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static AppointmentStatus[] AllowedTargets(AppointmentStatus from)
        => Allowed.TryGetValue(from, out var next) ? next : [];

    public static void EnsureCanTransition(AppointmentStatus from, AppointmentStatus to)
    {
        if (CanTransition(from, to))
            return;

        var allowed = AllowedTargets(from);
        var allowedText = allowed.Length == 0
            ? "none (terminal state)"
            : string.Join(", ", allowed);

        throw new DomainException(
            "Invalid Status Transition",
            $"Cannot transition from '{from}' to '{to}'. Allowed next statuses: {allowedText}.");
    }

    public static bool IsCustomerCancellable(AppointmentStatus status)
        => status is AppointmentStatus.Requested or AppointmentStatus.Confirmed;

    public static bool Overlaps(DateTime existingStart, int existingDurationMinutes, DateTime newStart, int newDurationMinutes)
    {
        var existingEnd = existingStart.AddMinutes(existingDurationMinutes);
        var newEnd = newStart.AddMinutes(newDurationMinutes);
        return existingStart < newEnd && existingEnd > newStart;
    }
}
