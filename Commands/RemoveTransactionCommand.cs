using System.Runtime.CompilerServices;

namespace PersonalFinanceApp;

public class RemoveTransactionCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly UserService _userService;

    public RemoveTransactionCommand(TransactionService transactionService, UserService userService)
    {
        _transactionService = transactionService;
        _userService = userService;
    }

    public void Execute()
    {
        // Get current transactions
        var summary = _transactionService.PrepareTransactionData("Day", _userService);

        // Display transactions with indices
        ConsoleUI.DisplayTransactionsByIndividual(summary, true);

        // Get valid index from user
        int index = InputHandler.GetTransactionIndex(summary.Transactions.Count);
        if (index == -1) return; // User cancelled

        // Remove the transaction
        Transaction transactionToRemove = summary.Transactions[index - 1];
        if (_transactionService.RemoveTransaction(transactionToRemove, _userService))
        {
            ConsoleUI.DisplaySuccess("Transaction removed successfully.");
        }
        else
        {
            ConsoleUI.DisplayError("Failed to remove transaction.");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}