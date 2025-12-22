using Dapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel.Contracts.Crm.Customers;

namespace CreditSystem.Infrastructure.Messaging.RabbitMq.Consumers;

public class CustomerCreatedConsumer: IConsumer<CustomerCreated>
{
    private readonly string _connectionString;
    private readonly ILogger<CustomerCreatedConsumer> _logger;

    public CustomerCreatedConsumer(string connectionString, ILogger<CustomerCreatedConsumer> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<CustomerCreated> context)
    {
        var message = context.Message;

        await CreateCustomerReferenceAsync(message);
    }

    public async Task CreateCustomerReferenceAsync(CustomerCreated message)
    {
        const string sql = @"
            INSERT INTO customer_references 
                (id, external_id, full_name, email, phone, document_type, document_number, created_at, updated_at)
            VALUES 
                (@Id, @ExternalId, @FullName, @Email, @Phone, @DocumentType, @DocumentNumber, @CreatedAt, @UpdatedAt)
            ON CONFLICT (external_id) DO UPDATE SET
                full_name = @FullName,
                email = @Email,
                phone = @Phone,
                document_type = @DocumentType,
                document_number = @DocumentNumber,
                updated_at = @UpdatedAt";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            ExternalId = message.CustomerId,
            message.FullName,
            message.Email,
            message.Phone,
            message.IdentificationType,
            message.IdentificationNumber,
            message.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Customer reference created/updated for external ID {ExternalId}", 
            message.CustomerId);
    }
}