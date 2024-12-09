namespace PersonalFinanceApp;

/// <summary>
/// Entry point for the Personal Finance App. 
/// Initializes the application, manages user sessions, and handles menus.
/// </summary>
class Program
{
    private static DatabaseService _dbService;
    private static UserService _userService;
    private static TransactionService _transactionService;
    private static ITransactionStorage _transactionStorage;
    private static UserSessionManager _userSessionManager;
    private static CommandManager _commandManager;

    static async Task Main(string[] args)
    {
        try
        {
            await Initialize();

            bool isRunning = true;
            while (isRunning)
            {
                isRunning = await RunLoginMenu(); // Show Login Menu
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex}");
        }
    }

    #region Initialization

    /// <summary>
    /// Sets up database services, user management, and session handling.
    /// </summary>
    static async Task Initialize()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += OnCancelKeyPress;

        _dbService = await DatabaseService.CreateAsync();
        _userService = new UserService(_dbService);
        _transactionStorage = new DatabaseTransactionStorage(_dbService);
        _transactionService = new TransactionService(_transactionStorage, _dbService);
        _commandManager = new CommandManager();
        _userSessionManager = new UserSessionManager(_userService, _commandManager, _transactionService, _transactionStorage);
    }

    #endregion

    #region Event Handlers

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

    #endregion

    #region Menus

    /// <summary>
    /// Displays the login menu and processes user input for login or account creation.
    /// </summary>
    /// <returns>A boolean indicating whether the application should continue running.</returns>
    static async Task<bool> RunLoginMenu()
    {
        ConsoleUI.DisplayStartMenu();
        ConsoleKey userChoice = Console.ReadKey(true).Key;

        switch (userChoice)
        {
            case ConsoleKey.D1: // Sign In
                if (await _userSessionManager.HandleSignIn())
                {
                    await RunMainMenu();
                }
                break;

            case ConsoleKey.D2: // Create Account
                if (_userSessionManager.HandleCreateAccount())
                {
                    await RunMainMenu();
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

    /// <summary>
    /// Displays the transaction menu after login and processes user actions.
    /// </summary>
    static async Task RunMainMenu()
    {
        bool userSignedIn = true;
        _commandManager.InitializeCommands(_transactionService, _userService.CurrentUser.UserId);

        while (userSignedIn)
        {
            try
            {
                ConsoleUI.DisplayMainMenu(_userService);
                ConsoleKey userChoice = Console.ReadKey(true).Key;

                switch (userChoice)
                {
                    case ConsoleKey.D1: // Show Transactions
                    case ConsoleKey.D2: // Add Income
                    case ConsoleKey.D3: // Add Expense
                        if (!await _commandManager.TryExecuteCommandAsync(userChoice))
                        {
                            break; // Continue on failure
                        }
                        break;

                    case ConsoleKey.D6: // Sign Out
                        if (_userSessionManager.HandleSignOut())
                        {
                            ConsoleUI.DisplaySuccess("Successfully signed out.");
                            userSignedIn = false; // Exit the loop after signing out
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

    #endregion
}
