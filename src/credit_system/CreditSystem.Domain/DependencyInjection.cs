using CreditSystem.Domain.Rules;
using CreditSystem.Domain.Rules.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreditSystem.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Rules Engine
        services.AddScoped<IContractRule, MaxLoanAmountRule>();
        services.AddScoped<IContractRule, CreditScoreRule>();
        services.AddScoped<IContractRule, DebtToIncomeRule>();
        services.AddScoped<IContractRule, CollateralRule>();
        services.AddScoped<IContractRule, ActiveLoansRule>();
        services.AddScoped<ContractEngine>();

        return services;
    }
}