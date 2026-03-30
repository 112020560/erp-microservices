using FluentValidation;

namespace Inventory.Application.Lots.Commands.CreateLot;

internal sealed class CreateLotCommandValidator : AbstractValidator<CreateLotCommand>
{
    public CreateLotCommandValidator()
    {
        RuleFor(x => x.LotNumber).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
