namespace PersonalFinanceApp;

public interface ITransactionStorage // Handles WHAT to store/retrieve (high-level storage logic)
{
    Task<List<Transaction>> LoadTransactionsAsync(int userId);
    Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId);

}
