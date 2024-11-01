using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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
            string timeUnit = "Month"; // Default time unit

            while (true)
            {
                Console.Clear();
                TransactionSummary summary = _transactionManager.PrepareTransactionData(timeUnit, _userManager);
                ConsoleUI.DisplayTransactions(summary, viewByTransaction);

                Console.WriteLine("     View by: [1. Day] [2. Week] [3. Month] [4. Year]\n     [5. Toggle Category/Transaction View]\n     [Esc. Go Back]");
                ConsoleKey userChoice = Console.ReadKey(true).Key;

                if (userChoice == ConsoleKey.Escape)
                {
                    return; // Go back to the main menu
                }

                timeUnit = userChoice switch
                {
                    ConsoleKey.D1 => "Day",
                    ConsoleKey.D2 => "Week",
                    ConsoleKey.D3 => "Month",
                    ConsoleKey.D4 => "Year",
                    _ => timeUnit // Keep current timeUnit if invalid input
                };

                if (userChoice == ConsoleKey.D5)
                {
                    viewByTransaction = !viewByTransaction;
                    Console.WriteLine(viewByTransaction ? "Switching to Transaction View..." : "Switching to Category View...");
                    Thread.Sleep(1250);
                }
            }
        }
    }
}



















