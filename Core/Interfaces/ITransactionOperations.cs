namespace PersonalFinanceApp;

public interface ITransactionOperations
{
    Transaction CreateTransaction(TransactionInputDTO dto, TransactionType type, int userId);
    void AddTransaction(Transaction transaction);
    bool RemoveTransaction(Transaction transactionToRemove, UserManager userManager);
}