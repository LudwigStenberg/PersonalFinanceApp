namespace PersonalFinanceApp;

public class GroupedTransactions
{
    public DateTime GroupKey { get; set; }
    public string TimeUnit { get; set; }
    public List<Transaction> Transactions { get; set; }
    public Dictionary<object, decimal> IncomesByCategory { get; set; }
    public Dictionary<object, decimal> ExpensesByCategory { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetResult { get; set; }


    public GroupedTransactions(DateTime groupKey, string timeUnit)
    {
        GroupKey = groupKey;
        TimeUnit = timeUnit;
        Transactions = new List<Transaction>();
        IncomesByCategory = new Dictionary<object, decimal>();
        ExpensesByCategory = new Dictionary<object, decimal>();
        TotalIncome = 0;
        TotalExpense = 0;
        NetResult = 0;
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
        NetResult += transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
        TotalIncome += transaction.Type == TransactionType.Income ? transaction.Amount : 0;
        TotalExpense += transaction.Type == TransactionType.Expense ? transaction.Amount : 0;

        if (transaction.Type == TransactionType.Income)
        {
            object categoryKey = transaction.Category != TransactionCategory.Custom
                                ? transaction.Category             // if true
                                : transaction.CustomCategoryName;  // if false

            if (!IncomesByCategory.ContainsKey(categoryKey))
            {
                IncomesByCategory[categoryKey] = transaction.Amount;
            }
            else
            {
                IncomesByCategory[categoryKey] += transaction.Amount;
            }
        }
        else if (transaction.Type == TransactionType.Expense)
        {
            object categoryKey = transaction.Category != TransactionCategory.Custom
                                 ? transaction.Category
                                 : transaction.CustomCategoryName;
            if (!ExpensesByCategory.ContainsKey(categoryKey))
            {
                ExpensesByCategory[categoryKey] = transaction.Amount;
            }
            else
            {
                ExpensesByCategory[categoryKey] += transaction.Amount;
            }
        }

    }
}





