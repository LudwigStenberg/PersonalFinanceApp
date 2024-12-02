
namespace PersonalFinanceApp;

public class TransactionService : ITransactionOperations
{
    private readonly IIdGenerator _idGenerator;
    private readonly ITransactionStorage _transactionStorage;
    private List<Transaction> _transactions = new List<Transaction>();

    public TransactionService(IIdGenerator idGenerator, ITransactionStorage transactionStorage)
    {
        _idGenerator = idGenerator;
        _transactionStorage = transactionStorage;
    }

    public Transaction CreateTransaction(TransactionInputDTO dto, TransactionType type, int userId)
    {
        string transactionId = _idGenerator.GenerateId();
        return new Transaction(transactionId, dto.Date, type, dto.Amount,
                             dto.Category, dto.Description, userId)
        {
            CustomCategoryName = dto.CustomCategoryName
        };
    }

    public void AddTransaction(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public bool RemoveTransaction(Transaction transactionToRemove, UserManager userManager)
    {
        return transactionToRemove.UserId == userManager.CurrentUser.UserId &&
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

    public int GetTransactionCount(UserManager userManager)
    {
        return _transactions.Count(t => t.UserId == userManager.CurrentUser.UserId);
    }

    public List<Transaction> GetOrderedTransactions(UserManager userManager)
    {
        return _transactions
            .Where(t => t.UserId == userManager.CurrentUser.UserId)
            .OrderBy(t => t.Date)
            .ToList();
    }

    public (decimal TotalIncome, decimal TotalExpenses) CalculateTotals(UserManager userManager)
    {
        decimal totalIncome = 0;
        decimal totalExpenses = 0;

        foreach (Transaction transaction in _transactions.Where(t => t.UserId == userManager.CurrentUser.UserId))
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

    public TransactionSummary PrepareTransactionData(string timeUnit, UserManager userManager)
    {
        if (GetTransactionCount(userManager) == 0)
        {
            return new TransactionSummary(timeUnit);
        }

        TransactionSummary summary = new TransactionSummary(timeUnit);
        summary.Transactions = GetOrderedTransactions(userManager);

        foreach (Transaction transaction in summary.Transactions)
        {
            string groupKey = TransactionDateHelper.GetGroupKey(transaction.Date, timeUnit);

            if (!summary.GroupedTransactions.ContainsKey(groupKey))
            {
                summary.GroupedTransactions[groupKey] = new List<Transaction>();
            }

            summary.GroupedTransactions[groupKey].Add(transaction);
        }

        var (totalIncome, totalExpense) = CalculateTotals(userManager);
        summary.TotalIncome = totalIncome;
        summary.TotalExpenses = totalExpense;
        summary.NetResult = totalIncome - totalExpense;

        return summary;
    }
}