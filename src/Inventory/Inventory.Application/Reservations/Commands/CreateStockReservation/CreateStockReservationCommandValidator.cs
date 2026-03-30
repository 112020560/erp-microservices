using FluentValidation;

namespace Inventory.Application.Reservations.Commands.CreateStockReservation;

internal sealed class CreateStockReservationCommandValidator : AbstractValidator<CreateStockReservationCommand>
{
    public CreateStockReservationCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.SalesOrderId).NotEmpty();
    }
}
