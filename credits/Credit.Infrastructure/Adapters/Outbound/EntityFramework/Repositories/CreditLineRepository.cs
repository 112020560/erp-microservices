using System;
using System.Text.Json;
using System.Threading.Tasks;
using Credit.Domain.Entities;
using Credit.Domain.Models;
using Credit.Infrastructure.Adapters.Outbound.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Credit.Infrastructure.Adapters.Outbound.EntityFramework.Repositories;

public class CreditLineRepository
{
    private readonly CreditDbContext _context;
    public CreditLineRepository(CreditDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(CreditLine creditLine, CancellationToken cancellationToken)
    {
        await _context.CreditLines.AddAsync(creditLine, cancellationToken);
    }

    //Update AmotizationSchedule
    public async Task UpdateAmotizationScheduleAsync(Guid creditLineId,PaymentScheduleModel[] paymentScheduleModels, CancellationToken cancellationToken)
    {
        var amortizationScheduleJson = JsonSerializer.Serialize(paymentScheduleModels);
        await _context.CreditLines.Where(cl => cl.Id == creditLineId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(cl => cl.AmortizationSchedule, amortizationScheduleJson)
                .SetProperty(cl => cl.UpdatedAt, DateTime.UtcNow), cancellationToken);
    }

    //Get all creditline by customer id
    public async Task<IEnumerable<CreditLine>> GetCreditLinesByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        return await _context.CreditLines.Where(creditLine => creditLine.CustomerId == customerId).ToListAsync(cancellationToken);
    }

}
