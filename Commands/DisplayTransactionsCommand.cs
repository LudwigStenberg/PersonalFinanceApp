namespace PersonalFinanceApp;

public class DisplayTransactionsCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly int _userId;

    public DisplayTransactionsCommand(TransactionService transactionService, int userId)
    {
        _transactionService = transactionService;
        _userId = userId;
    }

    public async Task Execute()
    {
        bool viewByTransaction = true; // Default to viewing transactions individually.
        string timeUnit = "Month";     // Default time unit.

        while (true)
        {
            try
            {
                ConsoleUI.ClearAndWriteLine("     [TRANSACTIONS]\n");

                // Fetch and prepare transaction data.
                var summary = await _transactionService.GetGroupedTransactionsAsync(timeUnit, _userId);

                if (summary.GroupedTransactions.Count == 0)
                {
                    ConsoleUI.DisplayError("No transactions to display.");
                    return;
                }

                // Display based on current view mode.
                if (viewByTransaction)
                {
                    ConsoleUI.DisplayTransactionsByIndividual(summary, showIndices: false);
                }
                else
                {
                    ConsoleUI.DisplayTransactionsByCategory(summary);
                }

                ConsoleKey userChoice = ConsoleUI.DisplayTextAndGetChoice(new[]
                {
                "    View By: [1] Day [2] Week [3] Month [4] Year",
                "    [5]   -  Toggle by Transaction/Category",
                "    [6]   -  Delete single transaction",
                "    [7]   -  Delete multiple transactions",
                "    [Esc] -  Go Back",
            }, false);

                if (InputHandler.CheckForReturn(userChoice))
                {
                    return;
                }

                // Handle time unit changes.
                timeUnit = userChoice switch
                {
                    ConsoleKey.D1 => "Day",
                    ConsoleKey.D2 => "Week",
                    ConsoleKey.D3 => "Month",
                    ConsoleKey.D4 => "Year",
                    _ => timeUnit
                };

                // Handle view toggle.
                if (userChoice == ConsoleKey.D5)
                {
                    viewByTransaction = !viewByTransaction;
                    ConsoleUI.DisplayToggleViewMessage(viewByTransaction);
                }

                var _commandManager = new CommandManager();

                if (userChoice == ConsoleKey.D6)
                {

                    _commandManager.TryExecuteCommand(userChoice);
                }
                else if (userChoice == ConsoleKey.D7)
                {
                    _commandManager.TryExecuteCommand(userChoice);
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"Error during transaction display: {ex.Message}");
                return;
            }
        }
    }

}
