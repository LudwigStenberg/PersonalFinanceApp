namespace PersonalFinanceApp;

public class ConsoleUI
{

    public static void DisplayPrompt(string prompt)
    {
        Console.Write(prompt);
    }


    public static void DisplayError(string message, int sleep = 2000)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Thread.Sleep(sleep);
        Console.ResetColor();
    }



    public static void DisplaySuccess(string message, int delayMs = 2000)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Thread.Sleep(delayMs);
        Console.ResetColor();
    }

    public static void ClearAndWriteLine(string message, int delayMs = 0)
    {
        Console.Clear();
        Console.WriteLine(message);
        Thread.Sleep(delayMs);
    }

    public static void WelcomeUser(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            DisplaySuccess($"\nGood to see you, {username}!");
        }
    }


    public static void DisplayDashboard(TransactionService transactionService, int userId)
    {
        ClearAndWriteLine($"== DASHBOARD ==");
        try
        {
            var userTransactionData = transactionService.GetCurrentUserTransactionData();
            Console.WriteLine($"Hello, {userTransactionData.Username}!");

            decimal accountBalance = transactionService.GetAccountBalance();
            Console.WriteLine($"Account balance: {accountBalance:C}");
        }
        catch (Exception ex)
        {
            DisplayError($"Error retrieving account balance: {ex.Message}");
        }
    }

    public static void DisplayTransactionsByIndividual(TransactionSummary summary, bool showIndices = false)
    {
        const int padding = 5;
        const int dateWidth = 15;
        const int typeWidth = 12;
        const int amountWidth = 15;
        const int categoryWidth = 20;
        const int descriptionWidth = 30;

        // Header with padding
        Console.WriteLine(
            $"{new string(' ', padding)}{"Date",-dateWidth}{"Type",-typeWidth}" +
            $"{"Amount",-amountWidth}{"Category",-categoryWidth}{"Description",-descriptionWidth}");

        int index = 1;
        foreach (var groupKey in summary.GroupedTransactions.Keys.OrderBy(k => k))
        {
            string displayKey = TransactionDateHelper.GetGroupKey(groupKey, summary.TimeUnit);

            Console.WriteLine(new string('=', padding + dateWidth + typeWidth + amountWidth + categoryWidth + descriptionWidth));
            Console.WriteLine($"{new string(' ', padding)}[{displayKey}]\n");


            foreach (var transaction in summary.GroupedTransactions[groupKey])
            {
                string indexDisplay = showIndices ? $"{index,-5}" : new string(' ', padding);
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
        Console.WriteLine(new string('=', padding + dateWidth + typeWidth + amountWidth + categoryWidth + descriptionWidth));
        DisplaySummary(summary);
    }



    public static void DisplayTransactionsByCategory(TransactionSummary summary)
    {
        var categorizedTransactions = summary.Transactions
            .GroupBy(t => t.Category)
            .OrderBy(g => g.Key);

        foreach (var category in categorizedTransactions)
        {

            Console.WriteLine($"\nCategory: {category.Key}");
            decimal categoryTotal = 0;

            foreach (var transaction in category)
            {
                Console.WriteLine($"  {transaction.Date:yyyy-MM-dd}  {transaction.Amount,10:C}  {transaction.Description}");
                categoryTotal += transaction.Amount;
            }

            Console.WriteLine($"  Total for {category.Key}: {categoryTotal,10:C}");

        }
        DisplaySummary(summary);
    }



    public static void DisplaySummary(TransactionSummary summary)
    {
        if (summary.IsEmpty)
        {
            Console.WriteLine($"There are no transactions made on this account.\nPress any key to go back...");
            return;
        }

        Console.WriteLine("     [SUMMARY]\n");
        Console.WriteLine($"     Total Income:    {summary.TotalIncome,15:C}\tNumber of transactions: {summary.Transactions.Count}");
        Console.WriteLine($"     Total Expenses:  {summary.TotalExpense,15:C}\tCurrent view: {summary.TimeUnit}");
        Console.WriteLine($"     Net Result:      {summary.NetResult,15:C}\tCurrent date range: {summary.Transactions.Min(t => t.Date):yyyy-MM-dd} to {summary.Transactions.Max(t => t.Date):yyyy-MM-dd}");
        Console.WriteLine(new string('=', 99));

    }



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

        // Add Custom as last option
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

    public static ConsoleKey DisplayMenuAndGetChoice(string[] options, bool clearScreen = true)
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

    public static void DisplayToggleViewMessage(bool viewByTransaction)
    {
        ClearAndWriteLine(viewByTransaction
            ? "Switching to Transaction View..."
            : "Switching to category View...", 2000);
    }

    public static int GetTransactionRemovalIndex(TransactionSummary summary)
    {
        DisplayTransactionsByIndividual(summary, true);
        return InputHandler.GetTransactionIndex(summary.Transactions.Count);
    }
}