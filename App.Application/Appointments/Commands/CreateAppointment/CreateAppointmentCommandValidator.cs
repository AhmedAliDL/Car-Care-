using App.Application.Abstractions.Time;
using FluentValidation;

namespace App.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator(IClock clock)
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.ServiceIds).NotEmpty().WithMessage("At least one service is required.");
        RuleFor(x => x.ServiceIds).Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate service IDs are not allowed.");
        RuleFor(x => x.AppointmentDate)
            .Must(date => date > clock.UtcNow)
            .WithMessage("Appointment date must be strictly in the future (UTC).");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
