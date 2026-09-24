using FluentValidation;

namespace App.Application.ServicesCatalog.Commands.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
    }
}
