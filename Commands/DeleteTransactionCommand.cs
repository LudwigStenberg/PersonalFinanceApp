namespace PersonalFinanceApp
{
    /// <summary>
    /// Command to delete a single transaction for a user.
    /// </summary>
    public class DeleteTransactionCommand : ICommand
    {
        private readonly TransactionService _transactionService;
        private readonly int _userId;

        public DeleteTransactionCommand(TransactionService transactionService, int userId)
        {
            _transactionService = transactionService;
            _userId = userId;
        }

        /// <summary>
        /// Executes the process of deleting a single transaction.
        /// </summary>
        public async Task Execute()
        {
            try
            {
                Console.Clear();
                // Fetch and display transactions for the user.
                var summary = await _transactionService.GetGroupedTransactionsDTOAsync("Day", _userId);

                if (summary.Transactions.Count == 0)
                {
                    ConsoleUI.DisplayError($"No transactions to remove.");
                    return;
                }
                // Get the transaction index from the user.
                int index = InputHandler.GetTransactionRemovalIndex(summary);
                if (index == -1) return; // Exit if ESC is pressed.

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
}
