using CreditSystem.Domain.Aggregates.LoanContract.Events;
using CreditSystem.Domain.Aggregates.LoanContract.Events.Base;
using CreditSystem.Domain.Enums;
using CreditSystem.Domain.Exceptions;
using CreditSystem.Domain.ValueObjects;

namespace CreditSystem.Domain.Aggregates.LoanContract;

public class LoanContractAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    
    public Guid Id { get; private set; }
    public LoanContractState State { get; private set; }
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    // Para rehidratar desde eventos
    public LoanContractAggregate(IEnumerable<IDomainEvent> events): this(null, events)
    {
        State = LoanContractState.Initial;
        foreach (var @event in events)
        {
            Apply(@event, isNew: false);
        }
    }

    // Para crear nuevo
    private LoanContractAggregate()
    {
        State = LoanContractState.Initial;
    }

    #region Factory Methods

    public static LoanContractAggregate Create(
        Guid customerId,
        Money principal,
        InterestRate rate,
        int termMonths,
        Dictionary<string, object> evaluationMetadata)
    {
        var aggregate = new LoanContractAggregate();
        var id = Guid.NewGuid();
        var schedule = PaymentSchedule.Calculate(principal, rate, termMonths, DateTime.UtcNow);

        aggregate.Apply(new ContractCreated
        {
            AggregateId = id,
            CustomerId = customerId,
            Principal = principal,
            InterestRate = rate,
            TermMonths = termMonths,
            Schedule = schedule,
            EvaluationMetadata = evaluationMetadata
        }, isNew: true);

        return aggregate;
    }

    #endregion

    #region Commands

    public void Disburse(string method, string destinationAccount)
    {
        EnsureStatus(ContractStatus.Approved, "Cannot disburse");

        Apply(new LoanDisbursed
        {
            AggregateId = Id,
            Amount = State.Principal,
            DisbursementMethod = method,
            DestinationAccount = destinationAccount,
            DisbursedAt = DateTime.UtcNow
        }, isNew: true);
    }

    public void AccrueInterest(DateTime periodStart, DateTime periodEnd)
    {
        EnsureStatus(ContractStatus.Active, "Cannot accrue interest");

        var interest = State.InterestRate.CalculateDailyInterest(State.CurrentBalance);
        var days = (periodEnd - periodStart).Days;
        var totalInterest = new Money(interest.Amount * days);

        Apply(new InterestAccrued
        {
            AggregateId = Id,
            Amount = totalInterest,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PrincipalBalance = State.CurrentBalance,
            RateApplied = State.InterestRate
        }, isNew: true);
    }

    public void ApplyPayment(Guid paymentId, Money amount, PaymentMethod method)
    {
        if (State.Status != ContractStatus.Active && State.Status != ContractStatus.Delinquent)
            throw new DomainException("Contract not in payable status");
        
        if (amount.Currency != State.Principal.Currency)
            throw new DomainException($"Currency mismatch: expected {State.Principal.Currency}, got {amount.Currency}");

        // Aplicar en orden: fees -> interest -> principal
        var remainingAmount = amount;
        var feePaid = Money.Zero();
        var interestPaid = Money.Zero();
        var principalPaid = Money.Zero();

        // 1. Pagar fees pendientes
        if (State.TotalFees.Amount > 0)
        {
            feePaid = remainingAmount > State.TotalFees ? State.TotalFees : remainingAmount;
            remainingAmount = remainingAmount - feePaid;
        }

        // 2. Pagar interés acumulado
        if (remainingAmount.Amount > 0 && State.AccruedInterest.Amount > 0)
        {
            interestPaid = remainingAmount > State.AccruedInterest ? State.AccruedInterest : remainingAmount;
            remainingAmount = remainingAmount - interestPaid;
        }

        // 3. Pagar principal
        if (remainingAmount.Amount > 0)
        {
            principalPaid = remainingAmount > State.CurrentBalance ? State.CurrentBalance : remainingAmount;
        }

        var newBalance = State.CurrentBalance - principalPaid;

        Apply(new PaymentApplied
        {
            AggregateId = Id,
            PaymentId = paymentId,
            TotalAmount = amount,
            PrincipalPaid = principalPaid,
            InterestPaid = interestPaid,
            FeePaid = feePaid,
            NewBalance = newBalance,
            PaymentNumber = State.PaymentsMade + 1,
            Method = method
        }, isNew: true);

        // Verificar si se pagó completamente
        if (newBalance.Amount == 0)
        {
            Apply(new ContractPaidOff
            {
                AggregateId = Id,
                FinalPayment = amount,
                TotalPrincipalPaid = State.Principal,
                TotalInterestPaid = State.AccruedInterest + interestPaid,
                TotalFeesPaid = State.TotalFees,
                PaidOffAt = DateTime.UtcNow,
                EarlyPayoff = State.PaymentsMade < State.TermMonths
            }, isNew: true);
        }
    }

    public void RecordMissedPayment(int paymentNumber, DateTime dueDate, Money lateFee)
    {
        EnsureStatus(ContractStatus.Active, ContractStatus.Delinquent);

        var scheduledPayment = State.Schedule.Entries
            .FirstOrDefault(e => e.PaymentNumber == paymentNumber)
            ?? throw new DomainException($"Payment {paymentNumber} not found in schedule");

        var daysOverdue = (DateTime.UtcNow - dueDate).Days;

        Apply(new PaymentMissed
        {
            AggregateId = Id,
            PaymentNumber = paymentNumber,
            DueDate = dueDate,
            AmountDue = scheduledPayment.TotalPayment,
            DaysOverdue = daysOverdue,
            LateFeeApplied = lateFee
        }, isNew: true);

        // Auto-default después de X días
        if (daysOverdue >= 90)
        {
            MarkAsDefault($"Payment {daysOverdue} days overdue");
        }
    }

    public void MarkAsDefault(string reason)
    {
        if (State.Status == ContractStatus.Default)
            return;

        Apply(new ContractDefaulted
        {
            AggregateId = Id,
            Reason = reason,
            DaysDelinquent = State.PaymentsMissed * 30, // Aproximado
            OutstandingBalance = State.CurrentBalance,
            AccruedInterest = State.AccruedInterest,
            TotalOwed = State.TotalOwed
        }, isNew: true);
    }

    public void Restructure(InterestRate newRate, int newTermMonths, Money forgiveAmount, string reason)
    {
        if (State.Status != ContractStatus.Delinquent && State.Status != ContractStatus.Default)
            throw new DomainException("Can only restructure delinquent or defaulted contracts");

        var newPrincipal = State.CurrentBalance - forgiveAmount;
        var newSchedule = PaymentSchedule.Calculate(newPrincipal, newRate, newTermMonths, DateTime.UtcNow);

        Apply(new ContractRestructured
        {
            AggregateId = Id,
            NewRate = newRate,
            NewTermMonths = newTermMonths,
            NewSchedule = newSchedule,
            ForgiveAmount = forgiveAmount,
            RestructureReason = reason
        }, isNew: true);
    }

    #endregion

    #region Event Application

    private void Apply(IDomainEvent @event, bool isNew)
    {
        State = ApplyEvent(State, @event);
        
        if (isNew)
        {
            if (@event is DomainEvent domainEvent)
            {
                _uncommittedEvents.Add(domainEvent with { Version = State.Version });
            }
            else
            {
                throw new InvalidOperationException(
                    $"Event {@event.GetType().Name} must inherit from DomainEvent");
            }
        }
    }

    private static LoanContractState ApplyEvent(LoanContractState state, IDomainEvent @event)
    {
        return @event switch
        {
            ContractCreated e => state with
            {
                Id = e.AggregateId,
                CustomerId = e.CustomerId,
                Principal = e.Principal,
                CurrentBalance = e.Principal,
                InterestRate = e.InterestRate,
                TermMonths = e.TermMonths,
                Schedule = e.Schedule,
                Status = ContractStatus.Approved,
                NextPaymentDue = e.Schedule.Entries.First().DueDate,
                Version = state.Version + 1
            },

            LoanDisbursed e => state with
            {
                Status = ContractStatus.Active,
                DisbursedAt = e.DisbursedAt,
                Version = state.Version + 1
            },

            InterestAccrued e => state with
            {
                AccruedInterest = state.AccruedInterest + e.Amount,
                LastInterestAccrualDate = e.PeriodEnd,
                Version = state.Version + 1
            },

            PaymentApplied e => state with
            {
                CurrentBalance = e.NewBalance,
                AccruedInterest = state.AccruedInterest - e.InterestPaid,
                TotalFees = state.TotalFees - e.FeePaid,
                PaymentsMade = state.PaymentsMade + 1,
                LastPaymentDate = DateTime.UtcNow,
                NextPaymentDue = state.Schedule.Entries
                    .FirstOrDefault(x => x.PaymentNumber == e.PaymentNumber + 1)?.DueDate,
                Status = state.PaymentsMissed > 0 && e.NewBalance.Amount > 0 
                    ? ContractStatus.Delinquent 
                    : ContractStatus.Active,
                Version = state.Version + 1
            },

            PaymentMissed e => state with
            {
                PaymentsMissed = state.PaymentsMissed + 1,
                TotalFees = state.TotalFees + e.LateFeeApplied,
                Status = ContractStatus.Delinquent,
                Version = state.Version + 1
            },

            ContractDefaulted e => state with
            {
                Status = ContractStatus.Default,
                DefaultedAt = DateTime.UtcNow,
                Version = state.Version + 1
            },

            ContractRestructured e => state with
            {
                InterestRate = e.NewRate,
                TermMonths = e.NewTermMonths,
                Schedule = e.NewSchedule,
                CurrentBalance = state.CurrentBalance - e.ForgiveAmount,
                Status = ContractStatus.Restructured,
                PaymentsMissed = 0,
                Version = state.Version + 1
            },

            ContractPaidOff e => state with
            {
                Status = ContractStatus.PaidOff,
                CurrentBalance = Money.Zero(),
                AccruedInterest = Money.Zero(),
                PaidOffAt = e.PaidOffAt,
                Version = state.Version + 1
            },

            _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
        };
    }

    #endregion

    #region Helpers

    private void EnsureStatus(ContractStatus expected, string message)
    {
        if (State.Status != expected)
            throw new DomainException($"{message}: expected {expected}, got {State.Status}");
    }

    private void EnsureStatus(params ContractStatus[] allowed)
    {
        if (!allowed.Contains(State.Status))
            throw new DomainException($"Invalid status: {State.Status}");
    }

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    #endregion
    
    public LoanContractAggregate(LoanContractState? snapshot, IEnumerable<IDomainEvent> events)
    {
        State = snapshot ?? LoanContractState.Initial;
        Id = State.Id;
        
        foreach (var @event in events)
        {
            Apply(@event, isNew: false);
        }
    }
}