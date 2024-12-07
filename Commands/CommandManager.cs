namespace PersonalFinanceApp;

public class CommandManager
{
    private readonly Dictionary<ConsoleKey, ICommand> _commands;

    public CommandManager()
    {
        _commands = new Dictionary<ConsoleKey, ICommand>();
    }


    public void InitializeCommands(TransactionService transactionService, int userId)
    {
        RegisterCommand(ConsoleKey.D1, new DisplayTransactionsCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D2, new AddIncomeCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D3, new AddExpenseCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D6, new DeleteTransactionCommand(transactionService, userId));
        RegisterCommand(ConsoleKey.D7, new DeleteTransactionsCommand(transactionService));
    }


    public void RegisterCommand(ConsoleKey key, ICommand command)
    {
        if (_commands.ContainsKey(key))
        {
            throw new ArgumentException($"Command already registered for key: {key}");
        }
        _commands[key] = command;
    }


    public void TryExecuteCommand(ConsoleKey key)
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
