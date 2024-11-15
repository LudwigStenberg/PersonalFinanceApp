﻿namespace PersonalFinanceApp;

public class ConsoleUI
{

    public static void DisplayPrompt(string prompt) // Wrapper for Console.Write but it allows for future enhancements. E.g. color, formatting etc.
    {
        Console.Write(prompt);
    }



    public static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }



    public static void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }



    public static void WelcomeUser(UserManager userManager)
    {
        if (userManager.CurrentUser != null)
        {
            Console.WriteLine($"\nGood to see you, {userManager.CurrentUser.Username}!");
            Thread.Sleep(1500);
        }
    }


    public static void DisplayDashboard(TransactionManager transactionManager)
    {
        Console.Clear();
        Console.WriteLine("== Dashboard ==\n");
        decimal accountBalance = transactionManager.GetAccountBalance();
        Console.WriteLine($"Account balance: {accountBalance:C}");
    }

    public static void DisplayTransactionsByIndividual(TransactionSummary summary, bool showIndices = false)
    {
        // Console.Clear();
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
            string displayKey = TransactionDateHelper.FormatGroupKey(groupKey, summary.TimeUnit);

            Console.WriteLine(new string('=', padding + dateWidth + typeWidth + amountWidth + categoryWidth + descriptionWidth));
            Console.WriteLine($"{new string(' ', padding)}{displayKey}\n");


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

            DisplaySummary(summary);
        }
    }



    public static void DisplaySummary(TransactionSummary summary)
    {
        if (summary.IsEmpty)
        {
            Console.WriteLine($"There are no transactions made on this account.\nPress any key to go back...");
            return;
        }

        Console.WriteLine("\n     == Summary ==\n");
        Console.WriteLine($"     Total Income:    {summary.TotalIncome,15:C}\tNumber of transactions: {summary.Transactions.Count}");
        Console.WriteLine($"     Total Expenses:  {summary.TotalExpenses,15:C}\tCurrent view: {summary.TimeUnit}");
        Console.WriteLine($"     Net Result:      {summary.NetResult,15:C}\tCurrent date range: {summary.Transactions.Min(t => t.Date):yyyy-MM-dd} to {summary.Transactions.Max(t => t.Date):yyyy-MM-dd}");
        Console.WriteLine(new string('=', 99));

    }




    public static void DisplayCategories()
    {
        Console.Clear();
        Console.WriteLine("== Categories ==\n");

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
}







