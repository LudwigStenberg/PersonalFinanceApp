namespace PersonalFinanceApp
{
    /// <summary>
    /// Command to add an expense transaction for a user.
    /// </summary>
    public class AddExpenseCommand : ICommand
    {
        private readonly TransactionService _transactionService;
        private readonly int _userId;

        public AddExpenseCommand(TransactionService transactionService, int userId)
        {
            _transactionService = transactionService;
            _userId = userId;
        }

        /// <summary>
        /// Executes the process of adding an expense transaction.
        /// </summary>
        public async Task Execute()
        {
            try
            {
                ConsoleUI.DisplayAddExpenseHeader();

                TransactionInputDTO transactionData = InputHandler.GetTransactionInput();

                Transaction transaction = _transactionService.CreateTransaction(
                     transactionData,
                     TransactionType.Expense,
                    _userId
                    );

                bool success = await _transactionService.AddTransactionAsync(transaction, _userId);

                if (success)
                {
                    ConsoleUI.DisplaySuccess("Expense added successfully");
                }
                else
                {
                    ConsoleUI.DisplayError("Failed to add expense.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"Error adding expense: {ex.Message}");
            }
        }
    }
}
