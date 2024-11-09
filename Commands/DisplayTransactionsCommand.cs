namespace PersonalFinanceApp
{
    public class DisplayTransactionsCommand : ICommand
    {

        private readonly TransactionManager _transactionManager;
        private readonly UserManager _userManager;

        public DisplayTransactionsCommand(TransactionManager transactionManager, UserManager userManager)
        {
            _transactionManager = transactionManager;
            _userManager = userManager;
        }

        public void Execute()
        {
            bool viewByTransaction = true;
            string timeUnit = "Month";

            while (true)
            {
                Console.Clear();
                TransactionSummary summary = _transactionManager.PrepareTransactionData(timeUnit, _userManager);

                if (viewByTransaction)
                {
                    ConsoleUI.DisplayTransactionsByIndividual(summary, showIndices: false);
                }
                else
                {
                    ConsoleUI.DisplayTransactionsByCategory(summary);
                }

                Console.WriteLine("     View by: [1. Day] [2. Week] [3. Month] [4. Year]");
                Console.WriteLine("     [5. Toggle Category/Transaction View]");
                Console.WriteLine("     [6. Remove Transaction]");
                Console.WriteLine("     [Esc. Go Back]");
                ConsoleKey userChoice = Console.ReadKey(true).Key;

                if (userChoice == ConsoleKey.Escape)
                {
                    return;
                }

                timeUnit = userChoice switch
                {
                    ConsoleKey.D1 => "Day",
                    ConsoleKey.D2 => "Week",
                    ConsoleKey.D3 => "Month",
                    ConsoleKey.D4 => "Year",
                    _ => timeUnit
                };

                if (userChoice == ConsoleKey.D5)
                {
                    viewByTransaction = !viewByTransaction;
                    Console.WriteLine(viewByTransaction ? "Switching to Transaction View..." : "Switching to Category View...");
                    Thread.Sleep(1250);
                }

                if (userChoice == ConsoleKey.D6)
                {
                    Console.Clear();
                    ConsoleUI.DisplayTransactionsByIndividual(summary, showIndices: true);

                    int indexToRemove = InputHandler.GetTransactionIndex(summary.Transactions.Count);
                    if (indexToRemove != -1)
                    {
                        var transactionToRemove = summary.Transactions[indexToRemove - 1];
                        bool success = _transactionManager.RemoveTransaction(transactionToRemove, _userManager);
                        Console.WriteLine(success ? "Transaction removed successfully." : "Failed to remove transaction.");
                        Thread.Sleep(1250);
                    }
                }
            }
        }


    }
}



















