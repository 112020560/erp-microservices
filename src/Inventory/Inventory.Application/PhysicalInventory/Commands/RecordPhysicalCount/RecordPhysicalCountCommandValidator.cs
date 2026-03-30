using FluentValidation;

namespace Inventory.Application.PhysicalInventory.Commands.RecordPhysicalCount;

internal sealed class RecordPhysicalCountCommandValidator : AbstractValidator<RecordPhysicalCountCommand>
{
    public RecordPhysicalCountCommandValidator()
    {
        RuleFor(x => x.CountId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.CountedQuantity).GreaterThanOrEqualTo(0);
    }
}
