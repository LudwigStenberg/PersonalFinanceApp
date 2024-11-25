namespace PersonalFinanceApp;

public class FileTransactionStorage : ITransactionStorage
{
    private readonly FileManager _fileManager;

    public FileTransactionStorage(FileManager fileManager)
    {
        _fileManager = fileManager;
    }



    public async Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId)
    {
        try
        {
            return await _fileManager.SaveToFileAsync(transactions, userId);

        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"There was an error saving the file: {ex.Message}");
        }

        return false;

    }



    public async Task<List<Transaction>> LoadTransactionsAsync(int userId)
    {
        try
        {
            return await _fileManager.LoadFromFileAsync(userId);
        }
        catch (IOException ex)
        {
            ConsoleUI.DisplayError($"Could not load transactions: {ex.Message}");
            return new List<Transaction>();
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Unexpected error loading transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }
}
