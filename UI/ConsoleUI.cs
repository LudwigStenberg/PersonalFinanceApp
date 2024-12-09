namespace PersonalFinanceApp
{
    public class ConsoleUI
    {
        #region Write Methods

        /// <summary>
        /// Displays a prompt message without a newline.
        /// </summary>
        public static void DisplayPrompt(string prompt)
        {
            Console.Write(prompt);
        }

        /// <summary>
        /// Displays an error message in red text and waits for a specified delay.
        /// </summary>
        public static void DisplayError(string message, int sleep = 1500)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Thread.Sleep(sleep);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays a success message in green text and waits for a specified delay.
        /// </summary>
        public static void DisplaySuccess(string message, int delayMs = 1500)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Thread.Sleep(delayMs);
            Console.ResetColor();
        }

        /// <summary>
        /// Clears the console and writes a message with an optional delay.
        /// </summary>
        public static void ClearAndWriteLine(string message, int delayMs = 0)
        {
            Console.Clear();
            Console.WriteLine(message);
            Thread.Sleep(delayMs);
        }

        #endregion

        #region UI Display Methods

        /// <summary>
        /// Displays the main menu and greets the user.
        /// </summary>
        public static void DisplayMainMenu(UserService userService)
        {

            ClearAndWriteLine(
            $"""
                ┏━━━━━━━━━━━━━━━━━ DASHBOARD ━━━━━━━━━━━━━━━━━━┓
                ┃                                              ┃
                ┃    [1]   -  Show Transactions                ┃  
                ┃    [2]   -  Add Income                       ┃  
                ┃    [3]   -  Add Expense                      ┃   
                ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫
                ┃    [6]   -  Sign Out                         ┃  
                ┃    [Esc] -  Exit Program                     ┃  
                ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛       
                """);
            DisplayCurrentUser(userService);
        }

        /// <summary>
        /// Displays the start menu.
        /// </summary>
        public static void DisplayStartMenu()
        {
            ClearAndWriteLine(
            $"""
            ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
            ┃             PERSONAL FINANCE APP             ┃ 
            ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫
            ┃    [1]    - Sign In                          ┃
            ┃    [2]    - Create Account                   ┃
            ┃    [Esc]  - Exit Application                 ┃
            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
            """);
        }

        /// <summary>
        /// Displays the login header.
        /// </summary>
        public static void DisplayLoginHeader()
        {
            ClearAndWriteLine(
            $"""
            ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
            ┃           SIGN IN AS EXISTING USER           ┃ 
            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
            """);
        }

        /// <summary>
        /// Displays the create account header.
        /// </summary>
        public static void DisplayCreateAccountHeader()
        {
            ClearAndWriteLine(
            $"""
            ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
            ┃               CREATE AN ACCOUNT              ┃ 
            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
            """);
        }

        /// <summary>
        /// Displays the add income header.
        /// </summary>
        public static void DisplayAddIncomeHeader()
        {
            ClearAndWriteLine(
            $"""
            ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
            ┃                  ADD INCOME                  ┃ 
            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
            """);
        }

        /// <summary>
        /// Displays the add expense header.
        /// </summary>
        public static void DisplayAddExpenseHeader()
        {
            ClearAndWriteLine(
            $"""
            ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
            ┃                  ADD EXPENSE                 ┃ 
            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
            """);
        }

        /// <summary>
        /// Displays a greeting for the current user.
        /// </summary>
        public static void DisplayCurrentUser(UserService userService)
        {
            if (userService.CurrentUser != null)
            {
                Console.WriteLine($"Greetings, {userService.CurrentUser.Username}!");
            }
        }
        #endregion

        #region Transaction Display Methods

        /// <summary>
        /// Displays transactions grouped by individual with optional indices.
        /// </summary>
        public static void DisplayTransactionsByIndividual(TransactionSummaryDTO summary, bool showIndices = false)
        {
            const int padding = 5;
            const int dateWidth = 15;
            const int typeWidth = 12;
            const int amountWidth = 15;
            const int categoryWidth = 20;
            const int descriptionWidth = 30;

            Console.WriteLine(
                $"{new string(' ', padding)}{"Date",-dateWidth}{"Type",-typeWidth}" +
                $"{"Amount",-amountWidth}{"Category",-categoryWidth}{"Description",-descriptionWidth}");

            int index = 1;
            foreach (var groupKey in summary.GroupedTransactionsDTO.Keys.OrderBy(k => k))
            {
                string displayKey = TransactionDateHelper.GetGroupKey(groupKey, summary.TimeUnit);

                Console.WriteLine(new string('━', padding + dateWidth + typeWidth + amountWidth + categoryWidth + descriptionWidth));
                Console.WriteLine($"{new string(' ', padding)}[{displayKey}]\n");

                foreach (var transaction in summary.GroupedTransactionsDTO[groupKey])
                {
                    string indexDisplay = showIndices ? $"#{index,-5}" : new string(' ', padding);
                    Console.WriteLine(
                        $"{indexDisplay}{transaction.Date,-dateWidth:yyyy-MM-dd}" +
                        $"{transaction.Type,-typeWidth}" +
                        $"{transaction.Amount.ToString("C"),-amountWidth}" +
                        $"{(transaction.Category == TransactionCategory.Custom ? transaction.CustomCategoryName : transaction.Category.ToString()),-categoryWidth}" +
                        $"{transaction.Description,-descriptionWidth}");
                    index++;
                }

                if (summary.TimeUnit == "Day")
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine(new string('━', 97));
            DisplaySummary(summary);
        }

        /// <summary>
        /// Displays transactions grouped by category.
        /// </summary>
        public static void DisplayTransactionsByCategory(TransactionSummaryDTO summary)
        {
            ClearAndWriteLine("     Date          Amount      Description");
            Console.WriteLine(new string('━', 97));
            int maxDescriptionWidth = summary.Transactions.Max(t => t.Description.Length);

            var categorizedTransactions = summary.Transactions
                .GroupBy(t => t.Category)
                .OrderBy(g => g.Key);

            foreach (var category in categorizedTransactions)
            {
                Console.WriteLine($"     [Category: {category.Key}]\n");
                decimal categoryTotal = 0;

                foreach (var transaction in category)
                {
                    string paddedDescription = transaction.Description.PadRight(maxDescriptionWidth);
                    Console.WriteLine($"     {transaction.Date:yyyy-MM-dd}    {transaction.Amount,-10:C}  {paddedDescription}");
                    categoryTotal += transaction.Amount;
                }

                Console.WriteLine($"\n     Total:     {categoryTotal,4:C}");
                Console.WriteLine(new string('━', 97));
            }

            DisplaySummary(summary);
        }

        /// <summary>
        /// Displays a summary of transactions.
        /// </summary>
        public static void DisplaySummary(TransactionSummaryDTO summary)
        {
            if (summary.IsEmpty)
            {
                Console.WriteLine($"There are no transactions made on this account.\nPress any key to go back...");
                return;
            }

            Console.WriteLine("     [SUMMARY]\n");
            Console.WriteLine($"     Total Income:    {summary.TotalIncome,15:C}\tNumber of transactions: {summary.Transactions.Count}");
            Console.WriteLine($"     Total Expenses:  {summary.TotalExpense,15:C}\tCurrent view: {summary.TimeUnit}");
            Console.WriteLine($"     Account Balance: {summary.NetResult,15:C}\tCurrent date range: {summary.Transactions.Min(t => t.Date):yyyy-MM-dd} to {summary.Transactions.Max(t => t.Date):yyyy-MM-dd}");
            Console.WriteLine(new string('━', 97));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Displays available categories for transactions.
        /// </summary>
        public static void DisplayCategories()
        {
            ClearAndWriteLine("[CATEGORIES]\n");

            List<string> categories = new List<string>();

            foreach (TransactionCategory category in Enum.GetValues<TransactionCategory>())
            {
                if (category != TransactionCategory.Custom)
                {
                    categories.Add(category.ToString());
                }
            }

            categories.Add("Custom");

            byte totalColumns = 3;
            byte totalRows = (byte)Math.Ceiling((double)categories.Count / totalColumns);
            byte columnWidth = 20;

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < totalColumns; col++)
                {
                    int index = row + col * totalRows;
                    if (index < categories.Count)
                    {
                        string formattedCategory = $"{index + 1,2}. {categories[index]}";
                        Console.Write(formattedCategory.PadRight(columnWidth));
                    }
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Displays a set of text options and gets the user's choice.
        /// </summary>
        public static ConsoleKey DisplayTextAndGetChoice(string[] options, bool clearScreen = true)
        {
            if (clearScreen)
            {
                Console.Clear();
            }

            foreach (var option in options)
            {
                Console.WriteLine(option);
            }
            return Console.ReadKey(true).Key;
        }

        /// <summary>
        /// Gets a confirmation response from the user.
        /// </summary>
        public static bool GetConfirmation(string message)
        {
            var userChoice = DisplayTextAndGetChoice(new[]
            {
                message,
                "[1] Yes",
                "[2] No, cancel."
            });

            return userChoice == ConsoleKey.D1;
        }

        #endregion
    }
}
