namespace PersonalFinanceApp;

class Program
{
    private static ITransactionStorage _transactionStorage;
    private static TransactionService _transactionService;
    private static UserService _userService;
    private static LoginManager _loginManager;
    private static CommandManager _commandManager;
    private static FileManager _fileManager;

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

            await SaveAndExit();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Input error: {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex}");
        }
    }



    static async Task Initialize()
    {
        _fileManager = new FileManager();
        _fileManager.EnsureUserDataDirectoryExists();

        // Load users and calculate the highest UserId
        List<User> loadedUsers = await _fileManager.LoadUsersAsync();
        int highestUserId = loadedUsers.Count > 0
            ? loadedUsers.Max(u => (u.UserId))
            : 0;

        // Initialize DatabaseService and UserService
        var _dbService = new DatabaseService();
        _userService = new UserService(_dbService, highestUserId);
        _userService.LoadUsers(loadedUsers);

        // Initialize other Services
        _transactionStorage = new FileTransactionStorage(_fileManager);
        _transactionService = new TransactionService(_transactionStorage);
        _loginManager = new LoginManager(_userService, _fileManager, _commandManager, _transactionService, _transactionStorage);

        // Initialize
        _commandManager = new CommandManager();
    }


    static async Task<bool> RunLoginMenu()
    {
        ConsoleKey userChoice = ConsoleUI.DisplayMenuAndGetChoice(new[]
        {
            "1.   Sign In",
            "2.   Create Account",
            "Esc. Exit Application"
        });

        switch (userChoice)
        {
            case ConsoleKey.D1:
                if (await _loginManager.HandleLogin())
                {
                    await RunMainMenu();
                }
                break;
            case ConsoleKey.D2:
                if (await _loginManager.HandleCreateAccount())
                {
                    await RunMainMenu();
                }
                break;
            case ConsoleKey.Escape:
                return false;
            default:
                ConsoleUI.DisplayError("Invalid input.");
                Thread.Sleep(1500);
                break;
        }

        return true;
    }

    static async Task RunMainMenu()
    {
        bool userSignedIn = true;

        // Ensure commands are initialized
        _commandManager.InitializeCommands(_transactionService, _userService.CurrentUser.UserId);

        while (userSignedIn)
        {
            try
            {
                await ConsoleUI.DisplayDashboard(_transactionService, _userService.CurrentUser.UserId);
                ConsoleKey userChoice = ConsoleUI.DisplayMenuAndGetChoice(new[]
                {
                "1. Show Transactions",
                "2. Add Income",
                "3. Add Expense",
                "4. Sign Out",
                "Esc. Exit Program"
            }, false);

                switch (userChoice)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                        await _commandManager.ExecuteCommand(userChoice);
                        break;

                    case ConsoleKey.D4:
                        if (await _loginManager.HandleSignOut())
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
                            await SaveAndExit();
                            ConsoleUI.DisplaySuccess("All data saved. Goodbye!");
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



    static async Task SaveAndExit()
    {
        if (_userService.CurrentUser != null)
        {
            int userId = _userService.CurrentUser.UserId;
            List<Transaction> transactions = await _transactionService.GetCurrentUserTransactionsAsync(userId);

            await _transactionStorage.SaveTransactionsAsync(transactions, userId);
            await _fileManager.SaveUsersAsync(_userService);
        }
    }
}

