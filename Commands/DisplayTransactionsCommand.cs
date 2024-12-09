namespace PersonalFinanceApp
{
    /// <summary>
    /// Command to display transactions for a user.
    /// </summary>
    public class DisplayTransactionsCommand : ICommand
    {
        private readonly TransactionService _transactionService;
        private readonly int _userId;

        public DisplayTransactionsCommand(TransactionService transactionService, int userId)
        {
            _transactionService = transactionService;
            _userId = userId;
        }

        /// <summary>
        /// Executes the process of displaying user transactions.
        /// </summary>
        public async Task Execute()
        {
            var _commandManager = new CommandManager();
            _commandManager.InitializeCommands(_transactionService, _userId);

            bool viewByTransaction = true; // Default view
            string timeUnit = "Month";     // Default time unit

            while (true)
            {
                try
                {
                    Console.Clear();
                    var summary = await _transactionService.GetGroupedTransactionsDTOAsync(timeUnit, _userId);

                    if (summary.GroupedTransactionsDTO.Count == 0)
                    {
                        ConsoleUI.DisplayError("No transactions available.");
                        return; // Exit to Main Menu
                    }

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
                            $"""
                                 View By: [1] Day [2] Week [3] Month [4] Year 
                                 [5]   -  Toggle by Transaction/Category      
                                 [6]   -  Delete Single Transaction           
                                 [7]   -  Delete Multiple Transactions        
                                 [Esc] -  Go Back                             
                            """
                             }, clearScreen: false);

                    switch (userChoice)
                    {
                        case ConsoleKey.D1: timeUnit = "Day"; break;
                        case ConsoleKey.D2: timeUnit = "Week"; break;
                        case ConsoleKey.D3: timeUnit = "Month"; break;
                        case ConsoleKey.D4: timeUnit = "Year"; break;

                        case ConsoleKey.D5: // Toggle View
                            viewByTransaction = !viewByTransaction;
                            break;

                        case ConsoleKey.D6: // Delete Single Transaction
                        case ConsoleKey.D7: // Delete Multiple Transactions
                            await _commandManager.TryExecuteCommandAsync(userChoice);
                            break;

                        case ConsoleKey.Escape:
                            return;

                        default:
                            ConsoleUI.DisplayError("Invalid option, please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUI.DisplayError($"An error occurred: {ex.Message}");
                    return; // Exit to Main Menu on error
                }
            }
        }
    }
}
