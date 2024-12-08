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
