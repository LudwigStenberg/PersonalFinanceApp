namespace PersonalFinanceApp;

class Program
{
    private static ITransactionStorage _transactionStorage;
    private static TransactionService _transactionService;
    private static UserService _userService;
    private static LoginManager _loginManager;
    private static CommandManager _commandManager;

    static async Task Main(string[] args)
    {
        try
        {
            Initialize();

            bool isRunning = true;
            while (isRunning)
            {
                isRunning = await RunLoginMenu();
            }

            Console.WriteLine("Goodbye!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex}");
        }
    }




    static void Initialize()
    {
        var _dbService = new DatabaseService();
        _userService = new UserService(_dbService);
        _transactionStorage = new DatabaseTransactionStorage(_dbService);
        _transactionService = new TransactionService(_transactionStorage, _dbService);
        _loginManager = new LoginManager(_userService, _commandManager, _transactionService, _transactionStorage);
        _commandManager = new CommandManager();
    }


    static async Task<bool> RunLoginMenu()
    {
        ConsoleUI.DisplayLoginMenu();
        ConsoleKey userChoice = Console.ReadKey(true).Key;

        switch (userChoice)
        {
            case ConsoleKey.D1:
                if (await _loginManager.HandleLogin())
                {
                    RunMainMenu();
                }
                break;
            case ConsoleKey.D2:
                if (_loginManager.HandleCreateAccount())
                {
                    RunMainMenu();
                }
                break;
            case ConsoleKey.Escape:
                return false;
            default:
                ConsoleUI.DisplayError("Invalid input.", 800);
                break;
        }

        return true;
    }

    static void RunMainMenu()
    {
        bool userSignedIn = true;

        // Ensure commands are initialized
        _commandManager.InitializeCommands(_transactionService, _userService.CurrentUser.UserId);

        while (userSignedIn)
        {
            try
            {
                ConsoleUI.DisplayMainMenu(_transactionService);
                ConsoleKey userChoice = Console.ReadKey().Key;

                switch (userChoice)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                        _commandManager.TryExecuteCommand(userChoice);
                        break;

                    case ConsoleKey.D6:
                        if (_loginManager.HandleSignOut())
                        {
                            ConsoleUI.DisplaySuccess("Successfully signed out.");
                            return;
                        }
                        else
                        {
                            ConsoleUI.DisplayError("Error while signing out.");
                        }
                        break;

                    case ConsoleKey.Escape:
                        Console.WriteLine("Are you sure you want to exit? (Y/N)");
                        if (Console.ReadKey(true).Key == ConsoleKey.Y)
                        {
                            Environment.Exit(0);
                        }
                        break;

                    default:
                        ConsoleUI.DisplayError("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}

