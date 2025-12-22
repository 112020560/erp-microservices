using CreditSystem.Domain.Abstractions.Services;
using CreditSystem.Domain.Models.ReadModels;
using Dapper;
using Npgsql;

namespace CreditSystem.Infrastructure.Services;

public class LoanQueryService: ILoanQueryService
{
    private readonly string _connectionString;

    public LoanQueryService(string connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<bool> HasActiveLoansAsync(Guid customerId, CancellationToken ct = default)
    {
        const string sql = @"
        SELECT EXISTS(
            SELECT 1 FROM rm_loan_summaries 
            WHERE customer_id = @CustomerId 
            AND status IN ('Active', 'Delinquent')
        )";
    
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<bool>(sql, new { CustomerId = customerId });
    }
    
    public async Task<LoanSummaryReadModel?> GetLoanSummaryAsync(Guid loanId, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM rm_loan_summaries WHERE loan_id = @LoanId";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<LoanSummaryReadModel>(sql, new { LoanId = loanId });
    }

    public async Task<IReadOnlyList<LoanSummaryReadModel>> GetCustomerLoansAsync(Guid customerId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT * FROM rm_loan_summaries 
            WHERE customer_id = @CustomerId 
            ORDER BY created_at DESC";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<LoanSummaryReadModel>(sql, new { CustomerId = customerId });
        return results.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<DelinquentLoanReadModel>> GetDelinquentLoansAsync(
        int? minDaysOverdue = null, 
        CancellationToken ct = default)
    {
        var sql = "SELECT * FROM rm_delinquent_loans WHERE 1=1";
        var parameters = new DynamicParameters();

        if (minDaysOverdue.HasValue)
        {
            sql += " AND days_overdue >= @MinDays";
            parameters.Add("MinDays", minDaysOverdue.Value);
        }

        sql += " ORDER BY days_overdue DESC";

        await using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<DelinquentLoanReadModel>(sql, parameters);
        return results.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<PaymentHistoryReadModel>> GetPaymentHistoryAsync(
        Guid loanId, 
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT * FROM rm_payment_history 
            WHERE loan_id = @LoanId 
            ORDER BY payment_date DESC";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<PaymentHistoryReadModel>(sql, new { LoanId = loanId });
        return results.ToList().AsReadOnly();
    }

    public async Task<LoanPortfolioReadModel?> GetPortfolioSummaryAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM rm_loan_portfolio WHERE id = 'global'";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<LoanPortfolioReadModel>(sql);
    }

    public async Task<IReadOnlyList<UpcomingPaymentReadModel>> GetUpcomingPaymentsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT * FROM rm_upcoming_payments 
            WHERE due_date BETWEEN @FromDate AND @ToDate
            ORDER BY due_date ASC";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<UpcomingPaymentReadModel>(sql, new { FromDate = fromDate, ToDate = toDate });
        return results.ToList().AsReadOnly();
    }
    
    public async Task<IReadOnlyList<ActiveLoanForAccrual>> GetActiveLoansForAccrualAsync(CancellationToken ct = default)
    {
        const string sql = @"
        SELECT 
            loan_id as LoanId,
            customer_id as CustomerId,
            current_balance as CurrentBalance,
            interest_rate as InterestRate
        FROM rm_loan_summaries 
        WHERE status IN ('Active', 'Delinquent')
        AND current_balance > 0";

        await using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<ActiveLoanForAccrual>(sql);
    
        return results.ToList().AsReadOnly();
    }
}