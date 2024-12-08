using System.Data;
using Npgsql;

namespace PersonalFinanceApp;

public class TransactionService
{
    private readonly ITransactionStorage _transactionStorage;
    private readonly DatabaseService _dbService;
    private UserTransactionDataDTO _currentUserTransactionData;


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
    public async Task<bool> DeleteTransactionAsync(int transactionId)
    {
        const string deleteSql = @"DELETE FROM transactions
                                   WHERE transaction_id = @TransactionId";
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@TransactionId", transactionId }
            };

            await _dbService.ExecuteNonQueryAsync(deleteSql, parameters);
            ConsoleUI.DisplaySuccess($"Transaction with ID {transactionId} successfully deleted.");
            return true;

        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Error deleting transaction with ID {transactionId}: {ex.Message}");
            return false;
        }
    }


    public async Task<bool> DeleteTransactionsAsync(IEnumerable<int> transactionIds)
    {
        if (transactionIds == null || !transactionIds.Any())
        {
            ConsoleUI.DisplayError("No transaction IDs provided for deletion.");
            return false;
        }

        int count = transactionIds.Count(); // Calculate count once for reuse

        const string deleteSql = @"DELETE FROM transactions
                               WHERE transaction_id = ANY(@TransactionIds)";

        await using var transaction = await _dbService.Connection.BeginTransactionAsync();

        try
        {
            // Create parameters for the query
            var parameters = new Dictionary<string, object>
        {
            { "@TransactionIds", transactionIds.ToArray() } // Ensure parameter is an array
        };

            // Execute the query
            await _dbService.ExecuteNonQueryAsync(deleteSql, parameters);

            await transaction.CommitAsync(); // Commit the transaction
            ConsoleUI.DisplaySuccess($"{count} transactions successfully deleted.");
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Rollback on error
            ConsoleUI.DisplayError($"Error deleting transactions: {ex.Message}");
            return false;
        }
    }





    // ===================================================================================
    //                                  Utility Methods
    // ===================================================================================
    public async Task<UserTransactionDataDTO> GetUserTransactionDataAsync(int userId)
    {
        if (_currentUserTransactionData == null || _currentUserTransactionData.UserId != userId)
        {
            _currentUserTransactionData = await _transactionStorage.LoadTransactionsAsync(userId);
        }

        return _currentUserTransactionData;
    }

    public UserTransactionDataDTO GetCurrentUserTransactionData()
    {
        if (_currentUserTransactionData == null)
        {
            throw new InvalidOperationException("User transaction data has not been loaded.");
        }
        return _currentUserTransactionData;
    }

    public List<Transaction> GetOrderedTransactions()
    {
        try
        {
            UserTransactionDataDTO userData = GetCurrentUserTransactionData();
            return userData.Transactions.OrderBy(t => t.Date).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving ordered transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }

    /// <summary>
    /// Calculates the total income and expenses for the user.
    /// </summary>
    public (decimal TotalIncome, decimal TotalExpenses) CalculateTotals()
    {
        try
        {
            var userData = GetCurrentUserTransactionData();
            decimal totalIncome = userData.Transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            decimal totalExpenses = userData.Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return (totalIncome, totalExpenses);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error calculating totals: {ex.Message}");
            return (0, 0);
        }
    }


    /// <summary>
    /// Calculates the user's account balance (Income - Expenses).
    /// </summary>
    public decimal GetAccountBalance()
    {
        try
        {
            var userData = GetCurrentUserTransactionData();

            decimal accountBalance = 0;
            foreach (var t in userData.Transactions)
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
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error calculating account balance: {ex.Message}");
            return 0;
        }
    }



    // ===================================================================================
    //                           Reporting and Summarization
    // ===================================================================================
    /// <summary>
    /// Handles grouping and summarization of transactions.
    /// </summary>
    public async Task<TransactionSummaryDTO> GetGroupedTransactionsAsync(string timeUnit, int userId)
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
            var summary = new TransactionSummaryDTO(timeUnit);

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

    public async Task<bool> DeleteTransactionsByCategoryAsync(string categoryName)
    {
        var userData = GetCurrentUserTransactionData();

        // Match transactions by either enum Category or CustomCategoryName
        var transactionIds = userData.Transactions
            .Where(t =>
                t.Category.ToString().Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(t.CustomCategoryName) && t.CustomCategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
            .Select(t => t.TransactionId)
            .ToList();

        if (transactionIds.Count == 0)
        {
            ConsoleUI.DisplayError($"No transactions found for category: {categoryName}");
            return false;
        }

        return await DeleteTransactionsAsync(transactionIds);
    }

    public async Task<bool> DeleteTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var userData = GetCurrentUserTransactionData();
        var transactionIds = userData.Transactions
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .Select(t => t.TransactionId)
            .ToList();

        if (transactionIds.Count == 0)
        {
            ConsoleUI.DisplayError($"No transactions found in the specified date range.");
            return false;
        }

        return await DeleteTransactionsAsync(transactionIds);
    }
}