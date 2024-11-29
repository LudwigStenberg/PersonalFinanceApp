
namespace PersonalFinanceApp;

public class DatabseTransactionStorage : ITransactionStorage
{
    public Task<List<Transaction>> LoadTransactionsAsync(int userId)
    {
        const string sql = @"
        ";
    }

    public Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId)
    {
        throw new NotImplementedException();
    }
}

