using FluentValidation;

namespace Catalogs.Application.Products.Commands.ChangePrice;

internal sealed class ChangePriceCommandValidator : AbstractValidator<ChangePriceCommand>
{
    public ChangePriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewPrice).GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}
