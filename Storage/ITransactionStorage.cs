namespace PersonalFinanceApp;

public interface ITransactionStorage // Handles WHAT to store/retrieve (high-level storage logic)
{
    Task<UserTransactionDataDTO> LoadTransactionsAsync(int userId);
    Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId);

}
