namespace Retail.Application.Abstractions.Services;

public interface ICreditServiceClient
{
    Task<CustomerCreditStatusDto?> GetCustomerCreditStatusAsync(
        Guid externalCustomerId, CancellationToken ct = default);
}

public sealed record CustomerCreditStatusDto(
    bool CustomerExists,
    Guid? CreditCustomerId,
    bool HasActiveCreditLine,
    IReadOnlyList<ActiveCreditLineDto> ActiveLines);

public sealed record ActiveCreditLineDto(
    Guid LineId,
    string? ProductName,
    string? ProductCode,
    decimal Principal,
    decimal Outstanding,
    string Currency,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    int TermMonths);
