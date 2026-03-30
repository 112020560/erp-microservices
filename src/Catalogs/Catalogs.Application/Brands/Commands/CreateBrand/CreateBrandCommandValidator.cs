using FluentValidation;

namespace Catalogs.Application.Brands.Commands.CreateBrand;

internal sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.");
    }
}
