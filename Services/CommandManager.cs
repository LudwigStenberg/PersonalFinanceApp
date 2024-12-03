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

    public void InitializeCommands(TransactionService transactionService, int userId)
    {
        RegisterCommand(ConsoleKey.D1, new DisplayTransactionsCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D2, new AddIncomeCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D3, new AddExpenseCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D6, new RemoveTransactionCommand(transactionService, userId));
    }

    public bool ExecuteCommand(ConsoleKey key)
    {
        if (_commands.TryGetValue(key, out ICommand command))
        {
            command.Execute();
            return true;
        }
        return false;
    }
}
