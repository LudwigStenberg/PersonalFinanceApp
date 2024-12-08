namespace PersonalFinanceApp;

class Program
{
    private static DatabaseService _dbService;
    private static UserService _userService;
    private static TransactionService _transactionService;
    private static ITransactionStorage _transactionStorage;
    private static LoginManager _loginManager;
    private static CommandManager _commandManager;

    static async Task Main(string[] args)
    {
        try
        {
            await Initialize();
            bool isRunning = true;
            while (isRunning)
            {
                isRunning = await RunLoginMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex}");
        }
    }


    static async Task Initialize()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += OnCancelKeyPress;

        _dbService = await DatabaseService.CreateAsync();
        _userService = new UserService(_dbService);
        _transactionStorage = new DatabaseTransactionStorage(_dbService);
        _transactionService = new TransactionService(_transactionStorage, _dbService);
        _loginManager = new LoginManager(_userService, _commandManager, _transactionService, _transactionStorage);
        _commandManager = new CommandManager();
    }

    private static void OnProcessExit(object sender, EventArgs e)
    {
        _dbService?.Dispose();
    }

    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _dbService?.Dispose();
        Environment.Exit(0);
    }

    static async Task<bool> RunLoginMenu()
    {
        ConsoleUI.DisplayStartMenu();
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
        _commandManager.InitializeCommands(_transactionService, _userService.CurrentUser.UserId);

        while (userSignedIn)
        {
            try
            {
                ConsoleUI.DisplayMainMenu(_transactionService);
                ConsoleKey userChoice = Console.ReadKey(true).Key;

                // Handle user choice
                switch (userChoice)
                {
                    case ConsoleKey.D1: // Show Transactions
                    case ConsoleKey.D2: // Add Income
                    case ConsoleKey.D3: // Add Expense

                        if (!_commandManager.TryExecuteCommand(userChoice))
                        {
                            break; // Wait for command to complete before continuing
                        }
                        break;

                    case ConsoleKey.D6: // Sign Out
                        if (_loginManager.HandleSignOut())
                        {
                            ConsoleUI.DisplaySuccess("Successfully signed out.");
                            userSignedIn = false; // Exit the Main Menu loop
                        }
                        else
                        {
                            ConsoleUI.DisplayError("Error while signing out.");
                        }
                        break;

                    case ConsoleKey.Escape: // Exit Program
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

