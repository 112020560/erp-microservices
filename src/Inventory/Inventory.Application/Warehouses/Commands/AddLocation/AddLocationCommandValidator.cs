using FluentValidation;

namespace Inventory.Application.Warehouses.Commands.AddLocation;

internal sealed class AddLocationCommandValidator : AbstractValidator<AddLocationCommand>
{
    public AddLocationCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Aisle).NotEmpty();
        RuleFor(x => x.Rack).NotEmpty();
        RuleFor(x => x.Level).NotEmpty();
    }
}
