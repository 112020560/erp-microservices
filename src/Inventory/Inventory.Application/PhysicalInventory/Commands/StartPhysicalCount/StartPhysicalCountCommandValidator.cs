using FluentValidation;

namespace Inventory.Application.PhysicalInventory.Commands.StartPhysicalCount;

internal sealed class StartPhysicalCountCommandValidator : AbstractValidator<StartPhysicalCountCommand>
{
    public StartPhysicalCountCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
    }
}
