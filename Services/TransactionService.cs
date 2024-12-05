namespace PersonalFinanceApp;

public class TransactionService
{
    private readonly ITransactionStorage _transactionStorage;


    public TransactionService(ITransactionStorage transactionStorage)
    {
        _transactionStorage = transactionStorage;
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
            // Save only the new transaction
            return await _transactionStorage.SaveTransactionsAsync(new List<Transaction> { transaction }, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding transaction for User {userId}: {ex.Message}");
            return false;
        }
    }

    // public async Task<bool> AddTransactionAsync(Transaction transaction, int userId)
    // {
    //     try
    //     {
    //         List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);
    //         transactions.Add(transaction);
    //         return await _transactionStorage.SaveTransactionsAsync(transactions, userId);
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error adding transaction for User {userId}: {ex.Message}");
    //         return false;
    //     }
    // }

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
    /// Prepares a summary of transactions grouped by a specified time unit.
    /// </summary>
    public async Task<TransactionSummary> PrepareTransactionDataAsync(string timeUnit, int userId)
    {
        try
        {
            List<Transaction> transactions = await _transactionStorage.LoadTransactionsAsync(userId);

            if (transactions.Count == 0)
            {
                return new TransactionSummary(timeUnit);
            }

            var summary = new TransactionSummary(timeUnit)
            {
                Transactions = transactions.OrderBy(t => t.Date).ToList(),
            };

            foreach (var transaction in transactions)
            {
                string groupKey = TransactionDateHelper.GetGroupKey(transaction.Date, timeUnit);

                if (!summary.GroupedTransactions.ContainsKey(groupKey))
                {
                    summary.GroupedTransactions[groupKey] = new List<Transaction>();
                }

                summary.GroupedTransactions[groupKey].Add(transaction);
            }

            summary.TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            summary.TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            summary.NetResult = summary.TotalIncome - summary.TotalExpenses;

            return summary;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing transaction data for User {userId}: {ex.Message}");
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
