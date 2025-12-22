using System;
using Crm.Domain.Customers;

namespace Crm.Domain.Abstractions.Persistence;

public interface ICustomersRepository
{
    Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken);
    Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken);
}
