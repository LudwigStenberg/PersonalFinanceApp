using System.Data;
using Npgsql;
using Npgsql.Internal;

namespace PersonalFinanceApp;

public class TransactionService
{
    private readonly ITransactionStorage _transactionStorage;
    private readonly DatabaseService _dbService;


    public TransactionService(ITransactionStorage transactionStorage, DatabaseService dbService)
    {
        _transactionStorage = transactionStorage;
        _dbService = dbService;
    }


    // ===================================================================================
    //                            Transaction CRUD Operations
    // ===================================================================================
    /// <summary>
    /// Adds a new transaction for the specified user.
    /// </summary>
    /// 
    public async Task<bool> AddTransactionAsync(Transaction transaction, int userId)
    {
        try
        {
            return await _transactionStorage.SaveTransactionsAsync(new List<Transaction> { transaction }, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding transaction for User {userId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Removes an existing transaction for the specified user.
    /// </summary>
    public async Task<bool> RemoveTransactionAsync(Transaction transactionToRemove, int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);
            bool removed = transactions.Remove(transactionToRemove);

            if (!removed)
            {
                Console.WriteLine($"Transaction not found for removal. User ID: {userId}, Transaction: {transactionToRemove}.");
                return false;
            }

            return await _transactionStorage.SaveTransactionsAsync(transactions, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing transaction for User {userId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Retrieves all transactions for the specified user.
    /// </summary>
    public async Task<List<Transaction>> GetCurrentUserTransactionsAsync(int userId)
    {
        try
        {
            return await _transactionStorage.LoadTransactionsAsync(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting transactions for User {userId}: {ex.Message}");
            return new List<Transaction>();
        }
    }



    // ===================================================================================
    //                                  Utility Methods
    // ===================================================================================
    /// <summary>
    /// Gets the count of all transactions for the specified user.
    /// </summary>
    public async Task<int> GetTransactionCountAsync(int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);
            return transactions.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting transaction count for User {userId}: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Retrieves all transactions for the user, ordered by date (ascending).
    /// </summary>
    public async Task<List<Transaction>> GetOrderedTransactionsAsync(int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);
            return transactions.OrderBy(t => t.Date).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving ordered transactions for User {userId}: {ex.Message}");
            return new List<Transaction>();
        }
    }

    /// <summary>
    /// Calculates the total income and expenses for the user.
    /// </summary>
    public async Task<(decimal TotalIncome, decimal TotalExpenses)> CalculateTotalsAsync(int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);
            decimal totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            return (totalIncome, totalExpenses);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating totals for User {userId}: {ex.Message}");
            return (0, 0);
        }
    }

    /// <summary>
    /// Calculates the user's account balance (Income - Expenses).
    /// </summary>
    public async Task<decimal> GetAccountBalanceAsync(int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);

            decimal accountBalance = 0;
            foreach (var t in transactions)
            {
                if (t.Type == TransactionType.Income)
                {
                    accountBalance += t.Amount;
                }
                else if (t.Type == TransactionType.Expense)
                {
                    accountBalance -= t.Amount;
                }
            }

            return accountBalance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating account balance for User {userId}: {ex.Message}");
            return 0;
        }
    }


    // ===================================================================================
    //                           Reporting and Summarization
    // ===================================================================================
    /// <summary>
    /// Handles grouping and summarization of transactions.
    /// </summary>
    public async Task<TransactionSummary> GetGroupedTransactionsAsync(string timeUnit, int userId)
    {
        try
        {
            string groupByClause = timeUnit switch
            {
                "Day" => "DATE(date)",
                "Week" => "DATE_TRUNC('week', date)",
                "Month" => "DATE_TRUNC('month', date)",
                "Year" => "DATE_TRUNC('year', date)",
                _ => throw new ArgumentException("Invalid time unit", nameof(timeUnit))
            };

            string sql = $@"
                 SELECT {groupByClause} AS GroupKey,
                        transaction_id, date, type, amount, category, custom_category_name, description
                FROM transactions
                WHERE user_id = @UserId
                ORDER BY {groupByClause}";

            using var cmd = new NpgsqlCommand(sql, _dbService.Connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            var summary = new TransactionSummary(timeUnit);

            while (await reader.ReadAsync())
            {
                DateTime groupKey = reader.GetDateTime(0);
                int transactionId = reader.GetInt32(1);
                DateTime date = reader.GetDateTime(2);

                // Parse 'type' (TransactionType)
                var typeString = reader.GetString(3);
                if (!Enum.TryParse(typeString, ignoreCase: true, out TransactionType type))
                {
                    throw new ArgumentException($"Invalid transaction type: {typeString}");
                }

                decimal amount = reader.GetDecimal(4);

                // Parse 'category' (TransactionCategory)
                var categoryString = reader.GetString(5);
                if (!Enum.TryParse(categoryString, ignoreCase: true, out TransactionCategory category))
                {
                    throw new ArgumentException($"Invalid category: {categoryString}");
                }

                string customCategoryName = reader.IsDBNull(6) ? null : reader.GetString(6);
                string description = reader.IsDBNull(7) ? null : reader.GetString(7);

                // Add the transaction to the appropriate group
                if (!summary.GroupedTransactions.ContainsKey(groupKey))
                {
                    summary.GroupedTransactions[groupKey] = new List<Transaction>();
                }

                var transaction = new Transaction(transactionId, date, type, amount, category, customCategoryName, description, userId);
                summary.GroupedTransactions[groupKey].Add(transaction);

                // Add transaction to the summary's flat list of transactions
                summary.Transactions.Add(transaction);

                // Accumulate totals for Income and Expense
                if (type == TransactionType.Income)
                    summary.TotalIncome += amount;
                else if (type == TransactionType.Expense)
                    summary.TotalExpense += amount;
            }

            summary.NetResult = summary.TotalIncome - summary.TotalExpense;

            return summary;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetGroupedTransactionsAsync: {ex.Message}");
            throw;
        }
    }





    // ===================================================================================
    //                                   Helper Methods
    // ===================================================================================

    /// <summary>
    /// Creates a transaction object from the input DTO.
    /// </summary>
    public Transaction CreateTransaction(TransactionInputDTO dto, TransactionType type, int userId)
    {
        return new Transaction(dto.Date, type, dto.Amount, dto.Category, dto.CustomCategoryName, dto.Description, userId)
        {
            CustomCategoryName = dto.CustomCategoryName
        };
    }
}