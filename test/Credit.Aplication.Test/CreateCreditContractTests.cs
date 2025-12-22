using Credit.Application.CommandHandlers;
using Credit.Application.Commands;
using Credit.Application.Tests;
using Credit.Domain.Events;
using Credit.Domain.ValueObjects;
using Xunit;

namespace Credit.Aplication.Test;

public class CreateCreditContractTests
{
    [Fact]
    public async Task Given_command_When_handled_Then_event_is_persisted()
    {
        // Arrange
        var store = new FakeEventStore();
        var handler = new CreateCreditContractHandler(store);

        var command = new CreateCreditContract(
            CreditId.New(),
            "CRC",
            DateTime.UtcNow
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var @event = Assert.Single(store.Events);
        Assert.IsType<CreditContractCreated>(@event);
    }
}
