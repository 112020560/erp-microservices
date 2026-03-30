using FluentValidation;

namespace Inventory.Application.Stock.Commands.ReceiveGoods;

internal sealed class ReceiveGoodsCommandValidator : AbstractValidator<ReceiveGoodsCommand>
{
    public ReceiveGoodsCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.LocationId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
