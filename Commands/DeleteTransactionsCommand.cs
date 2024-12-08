namespace PersonalFinanceApp;

public class DeleteTransactionsCommand : ICommand
{
    private readonly TransactionService _transactionService;

    public DeleteTransactionsCommand(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }


    public async Task Execute()
    {
        while (true)
        {
            ConsoleUI.ClearAndWriteLine(
            $"""    
            Choose the criteria for deleting transactions:
            [1]   - By Category
            [2]   - By Date Range
            [Esc] - Cancel and return to the main menu
            """);

            var userChoice = Console.ReadKey(intercept: true).Key;

            if (InputHandler.CheckForReturn(userChoice))
            {
                Console.WriteLine("\nReturning to the main menu...");
                return;
            }

            switch (userChoice)
            {
                case ConsoleKey.D1:
                    await DeleteByCategory();
                    break;
                case ConsoleKey.D2:
                    await DeleteByDateRange();
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please try again.");
                    continue;
            }
        }
    }

    private async Task DeleteByCategory()
    {
        // Collect and validate category input
        Console.Write("\nEnter category name:");
        string categoryName = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            Console.WriteLine("Invalid category. Please try again.");
            return;
        }

        // Confirm with the user and call TransactionService
        if (ConsoleUI.GetConfirmation($"Delete all transactions for category: {categoryName}?"))
        {
            bool success = await _transactionService.DeleteTransactionsByCategoryAsync(categoryName);
            if (success)
            {
                ConsoleUI.DisplaySuccess("Transactions deleted successfully.");
            }
            else
            {
                ConsoleUI.DisplayError("An error occurred.");
            }
        }
    }

    private async Task DeleteByDateRange()
    {
        // Collect and validate date range
        Console.WriteLine("Enter start date (yyyy-MM-dd):");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
        {
            Console.WriteLine("Invalid start date. Try again.");
            return;
        }

        Console.WriteLine("Enter end date (yyyy-MM-dd):");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
        {
            Console.WriteLine("Invalid end date. Try again.");
            return;
        }

        if (startDate > endDate)
        {
            Console.WriteLine("Start date cannot be after end date.");
            return;
        }

        // Confirm with the user and call TransactionService
        if (ConsoleUI.GetConfirmation($"Delete all transactions from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}?"))
        {
            bool success = await _transactionService.DeleteTransactionsByDateRangeAsync(startDate, endDate);
            Console.WriteLine(success ? "Transactions deleted successfully." : "An error occurred.");
        }
    }
}