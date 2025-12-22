using CreditSystem.Domain.Abstractions;
using CreditSystem.Domain.Abstractions.Projections;
using CreditSystem.Domain.Abstractions.Services;
using CreditSystem.Domain.Aggregates.LoanContract;
using CreditSystem.Domain.Rules;
using CreditSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CreditSystem.Application.Commands.CreateContract;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, CreateContractResponse>
{
    private readonly ILoanContractRepository _repository;
    private readonly ICustomerService _customerService;
    private readonly ILoanQueryService _queryService;
    private readonly ContractEngine _contractEngine;
    private readonly IProjectionEngine _projectionEngine;
    private readonly ILogger<CreateContractCommandHandler> _logger;

    public CreateContractCommandHandler(
        ILoanContractRepository repository,
        ICustomerService customerService,
        ILoanQueryService queryService,
        ContractEngine contractEngine,
        IProjectionEngine projectionEngine,
        ILogger<CreateContractCommandHandler> logger)
    {
        _repository = repository;
        _customerService = customerService;
        _queryService = queryService;
        _contractEngine = contractEngine;
        _projectionEngine = projectionEngine;
        _logger = logger;
    }

    public async Task<CreateContractResponse> Handle(
        CreateContractCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Buscar cliente por external ID (CRM)
        var customer = await _customerService.GetByExternalIdAsync(
            request.ExternalCustomerId, 
            cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning(
                "Customer not found for external ID {ExternalId}", 
                request.ExternalCustomerId);
            
            return CreateContractResponse.Failed(
                $"Customer with external ID {request.ExternalCustomerId} not found");
        }
        
        // 2. Verificar si tiene préstamos activos
        var hasActiveLoans = await _queryService.HasActiveLoansAsync(customer.Id, cancellationToken);

        // 2. Evaluar reglas del motor de Smart Contract
        var evaluationContext = new ContractEvaluationContext
        {
            Customer = customer,
            RequestedAmount = request.Amount,
            TermMonths = request.TermMonths,
            CollateralValue = request.CollateralValue,
            CreditScore = customer.CreditScore,
            MonthlyIncome = customer.MonthlyIncome,
            MonthlyDebt = customer.MonthlyDebt,
            HasActiveLoans = hasActiveLoans
        };

        var evaluation = await _contractEngine.EvaluateAsync(evaluationContext);

        if (!evaluation.Approved)
        {
            _logger.LogInformation(
                "Contract rejected for customer {CustomerId}. Reasons: {Reasons}",
                customer.Id,
                string.Join(", ", evaluation.Results.Where(r => !r.Passed).Select(r => r.Message)));

            return CreateContractResponse.Rejected(evaluation.Results);
        }

        // 3. Crear el Aggregate
        var principal = new Money(request.Amount, request.Currency);
        var interestRate = new InterestRate(evaluation.InterestRate);

        var aggregate = LoanContractAggregate.Create(
            customerId: customer.Id,  // ID local, no el del CRM
            principal: principal,
            rate: interestRate,
            termMonths: request.TermMonths,
            evaluationMetadata: new Dictionary<string, object>
            {
                ["ExternalCustomerId"] = request.ExternalCustomerId,
                ["CollateralValue"] = request.CollateralValue ?? 0,
                ["EvaluationResults"] = evaluation.Results
            }
        );

        // 4. Persistir eventos
        await _repository.SaveAsync(aggregate, cancellationToken);

        // 5. Proyectar eventos a Read Models
        foreach (var @event in aggregate.UncommittedEvents)
        {
            await _projectionEngine.ProjectEventAsync(@event, cancellationToken);
        }

        _logger.LogInformation(
            "Contract {ContractId} created for customer {CustomerId} with rate {Rate}%",
            aggregate.Id, customer.Id, evaluation.InterestRate);

        return CreateContractResponse.Approved(
            aggregate.Id, 
            evaluation.InterestRate, 
            evaluation.Results);
    }
}