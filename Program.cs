using PersonalFinanceApp.Services.Implementation;

namespace PersonalFinanceApp;

class Program
{
    private static CommandManager _commandManager;
    private static UserService _userService;
    private static FileManager _fileManager;
private static ITransactionStorage _transactionStorage;
    private static TransactionService _transactionService;
    private static LoginManager _loginManager;

    static async Task Main(string[] args)
    {
        try
        {
            // TESTING
            // var dbService = new DatabaseService();
            // var user = dbService.AddUser("testUser2", "hashedPassword");
            // Console.WriteLine(user != null ? $"User created: {user.UserId}" : "User creation failed");
            // Console.ReadKey();

            await Initialize();
            RegisterCommands();

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
        IIdGenerator idGenerator = new TransactionIdGenerator();
        _transactionStorage = new FileTransactionStorage(_fileManager);
        _transactionService = new TransactionService(idGenerator, _transactionStorage);
        _loginManager = new LoginManager(_userService, _fileManager, _transactionService, _transactionStorage);
    }


    static async Task<bool> RunLoginMenu()
    {
        Console.Clear();
        Console.WriteLine("1.   - Sign in");
        Console.WriteLine("2.   - Create Account");
        Console.WriteLine("Esc. - Exit Application");
        ConsoleKeyInfo choice = Console.ReadKey(true);

        switch (choice.Key)
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
                ConsoleUI.DisplayDashboard(_transactionService);
                Console.WriteLine("\n1.   - Show Transactions");
                Console.WriteLine("2.   - Add Income");
                Console.WriteLine("3.   - Add Expense ");
                Console.WriteLine("4.   - Sign out");
                Console.WriteLine("Esc. - Exit Program");

                if (_userService.CurrentUser != null)
                {
                    Console.WriteLine($"\nLogged in as: {_userService.CurrentUser.Username}");
                }

                ConsoleKeyInfo userChoice = Console.ReadKey(true);

                switch (userChoice.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                        _commandManager.ExecuteCommand(userChoice.Key);
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

    private static void RegisterCommands() // Move to C.M?
    {
        _commandManager = new CommandManager();
        _commandManager.RegisterCommand(ConsoleKey.D1,
            new DisplayTransactionsCommand(_transactionService, _userService));
        _commandManager.RegisterCommand(ConsoleKey.D2,
            new AddIncomeCommand(_transactionService, _userService));
        _commandManager.RegisterCommand(ConsoleKey.D3,
            new AddExpenseCommand(_transactionService, _userService));
        _commandManager.RegisterCommand(ConsoleKey.D6,
            new RemoveTransactionCommand(_transactionService, _userService));
    }

    static async Task SaveAndExit()
    {
        if (_userService.CurrentUser != null)
        {
            int userId = _userService.CurrentUser.UserId;
            List<Transaction> transactions = _transactionService.GetCurrentUserTransactions(userId);

            await _transactionStorage.SaveTransactionsAsync(transactions, userId);
            await _fileManager.SaveUsersAsync(_userService);
        }
    }
}

