using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Retail.Application.Abstractions.Services;

namespace Retail.Infrastructure.Services;

internal sealed class CreditServiceClient(
    HttpClient httpClient,
    ILogger<CreditServiceClient> logger) : ICreditServiceClient
{
    public async Task<CustomerCreditStatusDto?> GetCustomerCreditStatusAsync(
        Guid externalCustomerId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/v1/credit/customers/{externalCustomerId}/credit-status", ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Credit service returned {StatusCode} for customer {CustomerId}",
                    response.StatusCode, externalCustomerId);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CustomerCreditStatusDto>(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to reach credit service for customer {CustomerId}", externalCustomerId);
            return null;
        }
    }
}
