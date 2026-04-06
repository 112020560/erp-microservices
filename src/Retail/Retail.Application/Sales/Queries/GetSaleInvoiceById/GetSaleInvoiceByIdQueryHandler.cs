using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;

namespace Retail.Application.Sales.Queries.GetSaleInvoiceById;

internal sealed class GetSaleInvoiceByIdQueryHandler(ISaleInvoiceRepository invoiceRepository)
    : IQueryHandler<GetSaleInvoiceByIdQuery, SaleInvoiceDetailResponse>
{
    public async Task<Result<SaleInvoiceDetailResponse>> Handle(
        GetSaleInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (invoice is null) return Result.Failure<SaleInvoiceDetailResponse>(SaleErrors.InvoiceNotFound);

        var payments = invoice.Payments
            .Select(p => new PaymentLineResponse(p.Id, p.Method, p.Amount, p.Reference))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PaymentLineResponse>;

        var response = new SaleInvoiceDetailResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.QuoteId,
            invoice.CashierId,
            invoice.RequiresElectronicInvoice,
            invoice.ElectronicDocumentId,
            invoice.Total,
            payments!,
            invoice.CreatedAt);

        return Result.Success(response);
    }
}
