
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