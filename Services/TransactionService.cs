
namespace PersonalFinanceApp;

public class TransactionService : ITransactionOperations
{
    private readonly ITransactionStorage _transactionStorage;
    private List<Transaction> _transactions = new List<Transaction>();

    public TransactionService(ITransactionStorage transactionStorage)
    {

        _transactionStorage = transactionStorage;
    }

    public Transaction CreateTransaction(TransactionInputDTO dto, TransactionType type, int userId)
    {
        return new Transaction(dto.Date, type, dto.Amount,
                             dto.Category, dto.Description, userId)
        {
            CustomCategoryName = dto.CustomCategoryName
        };
    }

    public void AddTransaction(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public bool RemoveTransaction(Transaction transactionToRemove, UserService userService)
    {
        return transactionToRemove.UserId == userService.CurrentUser.UserId &&
               _transactions.Remove(transactionToRemove);
    }

    public void InitializeTransactions(List<Transaction> transactions)
    {
        _transactions = transactions ?? new List<Transaction>();
    }

    public List<Transaction> GetCurrentUserTransactions(int userId)
    {
        return _transactions.Where(t => t.UserId == userId).ToList();
    }

    public int GetTransactionCount(UserService userService)
    {
        return _transactions.Count(t => t.UserId == userService.CurrentUser.UserId);
    }

    public List<Transaction> GetOrderedTransactions(UserService userService)
    {
        return _transactions
            .Where(t => t.UserId == userService.CurrentUser.UserId)
            .OrderBy(t => t.Date)
            .ToList();
    }

    public (decimal TotalIncome, decimal TotalExpenses) CalculateTotals(UserService userService)
    {
        decimal totalIncome = 0;
        decimal totalExpenses = 0;

        foreach (Transaction transaction in _transactions.Where(t => t.UserId == userService.CurrentUser.UserId))
        {
            if (transaction.Type == TransactionType.Income)
            {
                totalIncome += transaction.Amount;
            }
            else if (transaction.Type == TransactionType.Expense)
            {
                totalExpenses += transaction.Amount;
            }
        }
        return (totalIncome, totalExpenses);
    }

    public decimal GetAccountBalance()
    {
        decimal accountBalance = 0;
        foreach (Transaction t in _transactions)
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

    public TransactionSummary PrepareTransactionData(string timeUnit, UserService userService)
    {
        if (GetTransactionCount(userService) == 0)
        {
            return new TransactionSummary(timeUnit);
        }

        TransactionSummary summary = new TransactionSummary(timeUnit);
        summary.Transactions = GetOrderedTransactions(userService);

        foreach (Transaction transaction in summary.Transactions)
        {
            string groupKey = TransactionDateHelper.GetGroupKey(transaction.Date, timeUnit);

            if (!summary.GroupedTransactions.ContainsKey(groupKey))
            {
                summary.GroupedTransactions[groupKey] = new List<Transaction>();
            }

            summary.GroupedTransactions[groupKey].Add(transaction);
        }

        var (totalIncome, totalExpense) = CalculateTotals(userService);
        summary.TotalIncome = totalIncome;
        summary.TotalExpenses = totalExpense;
        summary.NetResult = totalIncome - totalExpense;

        return summary;
    }
}