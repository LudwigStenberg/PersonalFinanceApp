namespace PersonalFinanceApp
{
    /// <summary>
    /// Command to add an income transaction for a user.
    /// </summary>
    public class AddIncomeCommand : ICommand
    {
        private readonly TransactionService _transactionService;
        private readonly int _userId;

        public AddIncomeCommand(TransactionService transactionService, int userId)
        {
            _transactionService = transactionService;
            _userId = userId;
        }

        /// <summary>
        /// Executes the process of adding an income transaction.
        /// </summary>
        public async Task Execute()
        {
            try
            {
                ConsoleUI.DisplayAddIncomeHeader();

                TransactionInputDTO transactionData = InputHandler.GetTransactionInput();

                Transaction transaction = _transactionService.CreateTransaction(
                    transactionData,
                    TransactionType.Income,
                    _userId
                    );

                bool success = await _transactionService.AddTransactionAsync(transaction, _userId);

                if (success)
                {
                    ConsoleUI.DisplaySuccess("Income added successfully");
                }
                else
                {
                    ConsoleUI.DisplayError("Failed to add income.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"Error adding income: {ex.Message}");
            }
        }
    }
}
