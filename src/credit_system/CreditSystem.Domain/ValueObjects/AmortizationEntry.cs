namespace CreditSystem.Domain.ValueObjects;

public record AmortizationEntry(
    int PaymentNumber,
    DateTime DueDate,
    Money TotalPayment,
    Money Principal,
    Money Interest,
    Money Balance
);