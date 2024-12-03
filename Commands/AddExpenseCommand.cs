namespace PersonalFinanceApp;

public class AddExpenseCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly int _userId;

    public AddExpenseCommand(TransactionService transactionService, int userId)
    {
        _transactionService = transactionService;
        _userId = userId;
    }

    public async void Execute()
    {
        try
        {
            ConsoleUI.ClearAndWriteLine("== Add Expense ==\n");

            // Gather input for the transaction.
            TransactionInputDTO transactionData = InputHandler.GetTransactionInput();

            // Create a new Transaction object.
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
