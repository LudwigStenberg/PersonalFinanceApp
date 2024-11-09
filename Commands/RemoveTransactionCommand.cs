using System.Runtime.CompilerServices;

namespace PersonalFinanceApp;

public class RemoveTransactionCommand : ICommand
{
    private readonly TransactionManager _transactionManager;
    private readonly UserManager _userManager;

    public RemoveTransactionCommand(TransactionManager transactionManager, UserManager userManager)
    {
        _transactionManager = transactionManager;
        _userManager = userManager;
    }

    public void Execute()
    {
        // Get current transactions
        var summary = _transactionManager.PrepareTransactionData("Day", _userManager);

        // Display transactions with indices
        ConsoleUI.DisplayTransactionsByIndividual(summary, true);

        // Get valid index from user
        int index = InputHandler.GetTransactionIndex(summary.Transactions.Count);
        if (index == -1) return; // User cancelled

        // Remove the transaction
        Transaction transactionToRemove = summary.Transactions[index - 1];
        if (_transactionManager.RemoveTransaction(transactionToRemove, _userManager))
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