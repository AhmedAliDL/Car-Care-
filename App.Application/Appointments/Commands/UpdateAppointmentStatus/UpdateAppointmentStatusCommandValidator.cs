using FluentValidation;

namespace App.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
