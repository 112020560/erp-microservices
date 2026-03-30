using FluentValidation;

namespace Inventory.Application.Catalog.Commands.RegisterProduct;

internal sealed class RegisterProductCommandValidator : AbstractValidator<RegisterProductCommand>
{
    public RegisterProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderPoint).GreaterThanOrEqualTo(0);
    }
}
