namespace PersonalFinanceApp
{
    /// <summary>
    /// Manages console commands and their execution in the Personal Finance application.
    /// </summary>
    public class CommandManager
    {
        private readonly Dictionary<ConsoleKey, ICommand> _commands;

        public CommandManager()
        {
            _commands = new Dictionary<ConsoleKey, ICommand>();
        }

        #region Command Initialization

        /// <summary>
        /// Initializes the command dictionary with predefined commands for transaction operations.
        /// </summary>
        public void InitializeCommands(TransactionService transactionService, int userId)
        {
            _commands.Clear();

            RegisterCommand(ConsoleKey.D1, new DisplayTransactionsCommand(transactionService, userId));
            RegisterCommand(ConsoleKey.D2, new AddIncomeCommand(transactionService, userId));
            RegisterCommand(ConsoleKey.D3, new AddExpenseCommand(transactionService, userId));
            RegisterCommand(ConsoleKey.D6, new DeleteTransactionCommand(transactionService, userId));
            RegisterCommand(ConsoleKey.D7, new DeleteTransactionsCommand(transactionService));
        }

        /// <summary>
        /// Registers a command for a specified console key.
        /// </summary>
        public void RegisterCommand(ConsoleKey key, ICommand command)
        {
            if (_commands.ContainsKey(key))
            {
                throw new ArgumentException($"Command already registered for key: {key}");
            }
            _commands[key] = command;
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Attempts to execute a command associated with the specified console key.
        /// </summary>
        public async Task<bool> TryExecuteCommandAsync(ConsoleKey key)
        {
            if (_commands.TryGetValue(key, out var command))
            {
                // Use await to handle the async execution properly
                await command.Execute();  // Awaiting the async task to complete before moving forward
                return false; // Indicate the Main Menu should not process input
            }

            ConsoleUI.DisplayError("Invalid command.");
            return true; // Continue Main Menu processing on invalid command
        }


        #endregion
    }
}
