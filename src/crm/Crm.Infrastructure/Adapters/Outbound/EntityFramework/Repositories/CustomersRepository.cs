using Crm.Domain.Abstractions.Persistence;
using Crm.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Adapters.Outbound.EntityFramework.Repositories;

public class CustomersRepository : ICustomersRepository
{
    private readonly CrmDbContext _context;
    public CustomersRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    public async Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken)
    {
        return await _context.Customers.ToListAsync(cancellationToken);
    }
}