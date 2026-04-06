using Microsoft.EntityFrameworkCore;
using Retail.Domain.Sales.Abstractions;

namespace Retail.Infrastructure.Persistence.Services;

internal sealed class SaleNumberGenerator(RetailDbContext context) : ISaleNumberGenerator
{
    public async Task<string> NextQuoteNumberAsync(CancellationToken ct = default)
    {
        var seq = await context.NumberSequences
            .FromSqlRaw("SELECT * FROM number_sequences WHERE sequence_name = 'quote' FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (seq is null)
        {
            seq = new NumberSequence { SequenceName = "quote", CurrentValue = 1 };
            context.NumberSequences.Add(seq);
        }
        else
        {
            seq.CurrentValue++;
        }

        await context.SaveChangesAsync(ct);
        return $"COT-{DateTime.UtcNow.Year}-{seq.CurrentValue:D6}";
    }

    public async Task<string> NextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var seq = await context.NumberSequences
            .FromSqlRaw("SELECT * FROM number_sequences WHERE sequence_name = 'invoice' FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (seq is null)
        {
            seq = new NumberSequence { SequenceName = "invoice", CurrentValue = 1 };
            context.NumberSequences.Add(seq);
        }
        else
        {
            seq.CurrentValue++;
        }

        await context.SaveChangesAsync(ct);
        return $"FAC-{DateTime.UtcNow.Year}-{seq.CurrentValue:D6}";
    }
}
