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
            Console.WriteLine($"Input error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
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
        _loginManager = new LoginManager(_userService, _fileManager, _transactionService, _transactionStorage);

        // Initialize commands
        _commandManager = new CommandManager();
        int userId = _userService.CurrentUser.UserId;
        _commandManager.InitializeCommands(_transactionService, userId);
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

                if (_userService.CurrentUser != null)
                {
                    Console.WriteLine($"\nLogged in as: {_userService.CurrentUser.Username}");
                }

                switch (userChoice)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                        _commandManager.ExecuteCommand(userChoice);
                        break;

                    case ConsoleKey.D4:
                        if (await _loginManager.HandleSignOut())
                        {
                            ConsoleUI.DisplaySuccess("Successfully signed out.");
                            return;
                        }
                        else
                        {
                            ConsoleUI.DisplayError("There was an error while signing out. Some data might not have been saved.");
                        }
                        break;

                    case ConsoleKey.Escape:
                        Console.WriteLine("\nAre you sure you want to exit the program? (Y/N)");
                        if (Console.ReadKey(true).Key == ConsoleKey.Y)
                        {
                            if (await _loginManager.HandleSignOut())
                            {
                                await SaveAndExit();
                                ConsoleUI.DisplaySuccess("All data saved. Goodbye!");
                                Thread.Sleep(2500);
                                Environment.Exit(0);
                            }
                            else
                            {
                                ConsoleUI.DisplayError("Error saving data before exit.");
                            }
                        }
                        break;

                    default:
                        ConsoleUI.DisplayError("Invalid option. Please try again.");
                        break;
                }
            }
            catch (IOException ex)
            {
                ConsoleUI.DisplayError($"File operation error: {ex.Message}");
                await SaveAndExit();
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"An unexpected error occurred: {ex.Message}");
                await SaveAndExit();
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

