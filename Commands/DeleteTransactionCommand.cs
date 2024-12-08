namespace PersonalFinanceApp;
public class DeleteTransactionCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly int _userId;

    public DeleteTransactionCommand(TransactionService transactionService, int userId)
    {
        _transactionService = transactionService;
        _userId = userId;
    }

    public async Task Execute()
    {
        try
        {
            Console.Clear();
            // Fetch and display transactions for the user.
            var summary = await _transactionService.GetGroupedTransactionsAsync("Day", _userId);

            if (summary.GroupedTransactions.Count == 0)
            {
                ConsoleUI.DisplayError($"No transactions to remove.");
                return;
            }

            ConsoleUI.DisplayTransactionsByIndividual(summary, showIndices: true);

            // Get the transaction index from the user.
            int index = InputHandler.GetTransactionIndex(summary.GroupedTransactions.Count);
            if (index == -1) return;

            // Identify and remove the transaction.
            Transaction transactionToRemove = summary.Transactions[index - 1];
            int transactionId = transactionToRemove.TransactionId;

            bool success = await _transactionService.DeleteTransactionAsync(transactionId);

            if (success)
            {
                ConsoleUI.DisplaySuccess($"Transaction removed successfully.");
            }
            else
            {
                ConsoleUI.DisplayError($"Failed to remove transaction.");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Error during transaction removal: {ex.Message}");
        }
    }
}
