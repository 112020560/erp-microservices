using CreditSystem.Application.Commands.ApplyPayment;
using CreditSystem.Application.Commands.CreateContract;
using CreditSystem.Application.Commands.DisburseLoan;
using CreditSystem.Application.Queries;
using CreditSystem.Application.Queries.GetLoanSummary;
using CreditSystem.Application.Queries.GetPaymentHistory;
using CreditSystem.Domain.Abstractions.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CreditSystem.Api.EndPoints;

public static class LoanContractEndpoints
{
    public static void MapLoanContractEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/loans")
            .WithTags("Loan Contracts")
            .WithOpenApi();

        // POST /api/loans - Crear contrato
        group.MapPost("/", CreateContract)
            .WithName("CreateContract")
            .WithSummary("Create a new loan contract")
            .Produces<CreateContractResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        
        
        // POST /api/loans/{id}/disburse - Desembolsar
        group.MapPost("/{id:guid}/disburse", DisburseLoan)
            .WithName("DisburseLoan")
            .WithSummary("Disburse an approved loan")
            .Produces<DisburseLoanResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/loans/{id} - Obtener resumen
        group.MapGet("/{id:guid}", GetLoanSummary)
            .WithName("GetLoanSummary")
            .WithSummary("Get loan summary by ID")
            .Produces<LoanSummaryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/loans/customer/{externalCustomerId} - Préstamos por cliente
        group.MapGet("/customer/{externalCustomerId:guid}", GetCustomerLoans)
            .WithName("GetCustomerLoans")
            .WithSummary("Get all loans for a customer")
            .Produces<IReadOnlyList<LoanSummaryResponse>>(StatusCodes.Status200OK);
        
        
        // POST /api/loans/{id}/payments - Aplicar pago
        group.MapPost("/{id:guid}/payments", ApplyPayment)
            .WithName("ApplyPayment")
            .WithSummary("Apply a payment to a loan")
            .Produces<ApplyPaymentResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/loans/{id}/payments - Historial de pagos
        group.MapGet("/{id:guid}/payments", GetPaymentHistory)
            .WithName("GetPaymentHistory")
            .WithSummary("Get payment history for a loan")
            .Produces<IReadOnlyList<PaymentHistoryResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateContract(
        [FromBody] CreateContractCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await mediator.Send(command, cancellationToken);

            if (!response.Success)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Contract Creation Failed",
                    Detail = response.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions = { ["errors"] = response.Errors }
                });
            }

            return Results.Created($"/api/loans/{response.ContractId}", response);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest,
                Extensions = { ["errors"] = ex.Errors.Select(e => e.ErrorMessage).ToList() }
            });
        }
    }
    
    private static async Task<IResult> DisburseLoan(
        Guid id,
        [FromBody] DisburseLoanRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DisburseLoanCommand
            {
                LoanId = id,
                DisbursementMethod = request.DisbursementMethod,
                DestinationAccount = request.DestinationAccount
            };

            var response = await mediator.Send(command, cancellationToken);

            if (!response.Success)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Disbursement Failed",
                    Detail = response.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions = { ["errors"] = response.Errors }
                });
            }

            return Results.Ok(response);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest,
                Extensions = { ["errors"] = ex.Errors.Select(e => e.ErrorMessage).ToList() }
            });
        }
    }

    private static async Task<IResult> GetLoanSummary(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetLoanSummaryQuery { LoanId = id };
        var response = await mediator.Send(query, cancellationToken);

        return response == null 
            ? Results.NotFound() 
            : Results.Ok(response);
    }

    private static async Task<IResult> GetCustomerLoans(
        Guid externalCustomerId,
        [FromServices] IMediator mediator,
        [FromServices] ICustomerService customerService,
        [FromServices] ILoanQueryService queryService,
        CancellationToken cancellationToken)
    {
        var customer = await customerService.GetByExternalIdAsync(externalCustomerId, cancellationToken);
        
        if (customer == null)
            return Results.NotFound();

        var loans = await queryService.GetCustomerLoansAsync(customer.Id, cancellationToken);
        var response = loans.Select(LoanSummaryResponse.FromReadModel).ToList();

        return Results.Ok(response);
    }
    
    private static async Task<IResult> ApplyPayment(
        Guid id,
        [FromBody] ApplyPaymentRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ApplyPaymentCommand
            {
                LoanId = id,
                Amount = request.Amount,
                Currency = request.Currency ?? "USD",
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber
            };

            var response = await mediator.Send(command, cancellationToken);

            if (!response.Success)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Payment Failed",
                    Detail = response.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions = { ["errors"] = response.Errors }
                });
            }

            return Results.Ok(response);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest,
                Extensions = { ["errors"] = ex.Errors.Select(e => e.ErrorMessage).ToList() }
            });
        }
    }

    private static async Task<IResult> GetPaymentHistory(
        Guid id,
        [FromServices] IMediator mediator,
        [FromServices] ILoanQueryService queryService,
        CancellationToken cancellationToken)
    {
        // Verificar que el préstamo existe
        var loan = await queryService.GetLoanSummaryAsync(id, cancellationToken);
    
        if (loan == null)
            return Results.NotFound();

        var query = new GetPaymentHistoryQuery { LoanId = id };
        var response = await mediator.Send(query, cancellationToken);

        return Results.Ok(response);
    }
}

public record DisburseLoanRequest
{
    public string DisbursementMethod { get; init; } = null!;
    public string DestinationAccount { get; init; } = null!;
}

public record ApplyPaymentRequest
{
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string PaymentMethod { get; init; } = null!;
    public string? ReferenceNumber { get; init; }
}