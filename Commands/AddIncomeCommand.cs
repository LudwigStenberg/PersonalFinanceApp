namespace PersonalFinanceApp;

public class AddIncomeCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly int _userId;

    public AddIncomeCommand(TransactionService transactionService, int userId)
    {
        _transactionService = transactionService;
        _userId = userId;
    }


    public async void Execute()
    {
        try
        {
            ConsoleUI.ClearAndWriteLine("== Add Income ==\n");

            // Gather input from user.
            TransactionInputDTO transactionData = InputHandler.GetTransactionInput();

            // Create transaction object.
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
