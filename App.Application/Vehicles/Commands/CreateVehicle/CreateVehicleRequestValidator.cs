using App.Application.Abstractions.Time;
using FluentValidation;

namespace App.Application.Vehicles.Commands.CreateVehicle;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator(IClock clock)
    {
        RuleFor(x => x.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Vin)
            .NotEmpty()
            .Length(17)
            .Matches("^[A-HJ-NPR-Z0-9]{17}$")
            .WithMessage("VIN must be 17 characters and exclude I, O, and Q.");
        RuleFor(x => x.Make).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year)
            .InclusiveBetween(1950, clock.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1950 and {clock.UtcNow.Year + 1}.");
    }
}
