using CreditSystem.Application.Job;
using CreditSystem.Domain.Abstractions;
using CreditSystem.Domain.Exceptions;
using CreditSystem.Infrastructure.Projections;
using Microsoft.AspNetCore.Mvc;

namespace CreditSystem.Api.EndPoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .WithOpenApi();

        // POST /api/admin/jobs/interest-accrual - Ejecutar manualmente
        group.MapPost("/jobs/interest-accrual", RunInterestAccrualJob)
            .WithName("RunInterestAccrualJob")
            .WithSummary("Manually trigger interest accrual job")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/admin/loans/{id}/accrue-interest - Acumular interés a un préstamo específico
        group.MapPost("/loans/{id:guid}/accrue-interest", AccrueInterestForLoan)
            .WithName("AccrueInterestForLoan")
            .WithSummary("Manually accrue interest for a specific loan")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        group.MapPost("/jobs/payment-missed", RunPaymentMissedJob)
            .WithName("RunPaymentMissedJob")
            .WithSummary("Manually trigger payment missed detection job")
            .Produces(StatusCodes.Status200OK);
    }
    
    private static async Task<IResult> RunPaymentMissedJob(
        [FromServices] IPaymentMissedJob job,
        CancellationToken cancellationToken)
    {
        await job.ExecuteAsync(cancellationToken);
        return Results.Ok(new { Message = "Payment missed job completed" });
    }

    private static async Task<IResult> RunInterestAccrualJob(
        [FromServices] IInterestAccrualJob job,
        CancellationToken cancellationToken)
    {
        await job.ExecuteAsync(cancellationToken);
        return Results.Ok(new { Message = "Interest accrual job completed" });
    }

    private static async Task<IResult> AccrueInterestForLoan(
        Guid id,
        [FromServices] ILoanContractRepository repository,
        [FromServices] ProjectionEngine projectionEngine,
        CancellationToken cancellationToken)
    {
        var aggregate = await repository.GetByIdAsync(id, cancellationToken);

        if (aggregate == null)
            return Results.NotFound();

        var periodEnd = DateTime.UtcNow.Date;
        var periodStart = aggregate.State.LastInterestAccrualDate?.Date 
            ?? aggregate.State.DisbursedAt?.Date 
            ?? periodEnd.AddDays(-1);

        try
        {
            aggregate.AccrueInterest(periodStart, periodEnd);
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }

        await repository.SaveAsync(aggregate, cancellationToken);

        foreach (var @event in aggregate.UncommittedEvents)
        {
            await projectionEngine.ProjectEventAsync(@event, cancellationToken);
        }

        return Results.Ok(new
        {
            LoanId = id,
            AccruedInterest = aggregate.State.AccruedInterest.Amount,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        });
    }
}