namespace PersonalFinanceApp;

public class CommandManager
{
    private readonly Dictionary<ConsoleKey, ICommand> _commands;

    public CommandManager()
    {
        _commands = new Dictionary<ConsoleKey, ICommand>();
    }

    public void RegisterCommand(ConsoleKey key, ICommand command)
    {
        if (_commands.ContainsKey(key))
        {
            throw new ArgumentException($"Command already registered for key: {key}");
        }
        _commands[key] = command;
    }

    // public void InitializeCommands(TransactionService transactionService, int userId)
    // {
    //     RegisterCommand(ConsoleKey.D1, new DisplayTransactionsCommand(transactionService, userId));
    //     RegisterCommand(ConsoleKey.D2, new AddIncomeCommand(transactionService, userId));
    //     RegisterCommand(ConsoleKey.D3, new AddExpenseCommand(transactionService, userId));
    //     RegisterCommand(ConsoleKey.D6, new RemoveTransactionCommand(transactionService, userId));
    // }

    public void InitializeCommands(TransactionService transactionService, int userId)
    {
        try
        {
            RegisterCommand(ConsoleKey.D1, new DisplayTransactionsCommand(transactionService, userId));
            Console.WriteLine("[DEBUG] D1 command registered successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Failed to register D1: {ex.Message}");
        }

        try
        {
            RegisterCommand(ConsoleKey.D2, new AddIncomeCommand(transactionService, userId));
            Console.WriteLine("[DEBUG] D2 command registered successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Failed to register D2: {ex.Message}");
        }

        try
        {
            RegisterCommand(ConsoleKey.D3, new AddExpenseCommand(transactionService, userId));
            Console.WriteLine("[DEBUG] D3 command registered successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Failed to register D3: {ex.Message}");
        }
    }



    public void ExecuteCommand(ConsoleKey key)
    {
        if (_commands.TryGetValue(key, out var command))
        {
            command.Execute();
        }
        else
        {
            ConsoleUI.DisplayError("Invalid command.");
        }
    }

}
