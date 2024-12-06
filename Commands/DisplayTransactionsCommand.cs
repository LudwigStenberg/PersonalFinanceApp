namespace PersonalFinanceApp;

public class DisplayTransactionsCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly int _userId;

    public DisplayTransactionsCommand(TransactionService transactionService, int userId)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _userId = userId > 0 ? userId : throw new ArgumentException("User ID must be positive.", nameof(userId));
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

                // Show menu and get user choice.
                // ConsoleKey userChoice = ConsoleUI.DisplayMenuAndGetChoice(new[]
                // {
                //     "    - View By -",
                //     "    [1] - Day      [5] - Toggle by Transaction/Category",
                //     "    [2] - Week     [6] - Remove Transaction",
                //     "    [3] - Month    [Esc]  - Go Back",
                //     "    [4] - Year",
                // }, false);

                ConsoleKey userChoice = ConsoleUI.DisplayMenuAndGetChoice(new[]
{
                    "    - View By -",
                    "    [1] - Day",
                    "    [2] - Week",
                    "    [3] - Month",
                    "    [4] - Year",
                    "    -----------",
                    "    [5] - Toggle by Transaction/Category",
                    "    [6] - Remove Transaction",
                    "    [Esc]  - Go Back",
                }, false);

                if (userChoice == ConsoleKey.Escape)
                {
                    return;
                }

                // Handle time unit selection.
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

                // Handle transaction removal.
                if (userChoice == ConsoleKey.D6)
                {
                    int indexToRemove = ConsoleUI.GetTransactionRemovalIndex(summary);
                    if (indexToRemove != -1)
                    {
                        var transactionToRemove = summary.Transactions[indexToRemove - 1];
                        bool success = await _transactionService.RemoveTransactionAsync(transactionToRemove, _userId);
                        ConsoleUI.DisplaySuccess(success
                            ? "Transaction removed successfully."
                            : "Failed to remove transaction.");
                    }
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
