
namespace PersonalFinanceApp;

public class LoginManager
{
    private readonly UserService _userService;
    private readonly FileManager _fileManager;
    private readonly CommandManager _commandManager;
    private readonly TransactionService _transactionService;
    private readonly ITransactionStorage _transactionStorage;


    public LoginManager(UserService userService, FileManager fileManager, CommandManager commandManager,
                       TransactionService transactionService, ITransactionStorage transactionStorage)
    {
        _userService = userService;
        _fileManager = fileManager;
        _commandManager = commandManager;
        _transactionService = transactionService;
        _transactionStorage = transactionStorage;
    }

    public async Task<bool> HandleLogin()
    {
        var (username, password) = InputHandler.GetExistingUserCredentials();

        // Authenticate the user using UserService.
        if (_userService.AuthenticateUser(username, password))
        {
            ConsoleUI.DisplaySuccess("Login successful...");
            ConsoleUI.WelcomeUser(username);

            if (_userService.CurrentUser == null)
            {
                ConsoleUI.DisplayError("User authentication succeeded, but CurrentUser is not set.");
                return false;
            }

            // Attempt to initialize user data.
            if (await InitializeUserData())
            {
                int transactionCount = await _transactionService.GetTransactionCountAsync(_userService.CurrentUser.UserId);
                Console.WriteLine($"{transactionCount} transactions loaded for {username}");

                if (transactionCount == 0)
                {
                    Console.WriteLine("No transactions found for this user.");
                }
                return true;
            }
            else
            {
                ConsoleUI.DisplayError("Failed to load user data. Some features may be unavailable.");
                return true; // Allow user to proceed despite initialization issues.
            }
        }

        // Authentication failed.
        ConsoleUI.DisplayError("Login failed. Username or Password is incorrect.");
        return false;
    }



    public async Task<bool> HandleCreateAccount()
    {
        var (username, password) = InputHandler.GetNewUserCredentials();
        if (_userService.CreateAccount(username, password))
        {
            try
            {
                await _fileManager.SaveUsersAsync(_userService);
                ConsoleUI.DisplaySuccess("Account creation successful!");
                ConsoleUI.WelcomeUser(username);
                return true;
            }
            catch (IOException ex)
            {
                ConsoleUI.DisplayError($"Could not save account due to file error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"Unexpected error while saving account: {ex.Message}");
                return false;
            }
        }

        ConsoleUI.DisplayError("Account creation failed.");
        return false;
    }



    public async Task<bool> HandleSignOut()
    {
        if (_userService.CurrentUser == null)
        {
            return true;
        }

        List<Transaction> transactions = await _transactionService.GetCurrentUserTransactionsAsync(_userService.CurrentUser.UserId);
        return await _userService.SignOut(
            transactions,
            _userService.CurrentUser.UserId,
            _fileManager,
            _userService
        );
    }

    private async Task<bool> InitializeUserData()
    {
        int retryCount = 3;
        while (retryCount > 0)
        {
            List<Transaction> loadedTransactions =
                await _transactionStorage.LoadTransactionsAsync(_userService.CurrentUser.UserId);

            Console.WriteLine($"Loaded {loadedTransactions.Count} transactions.");

            if (loadedTransactions.Count > 0)
            {
                ConsoleUI.DisplaySuccess("Data successfully loaded! Continuing...");
                return true;
            }

            if (!InputHandler.GetRetryChoice())
            {
                return false;
            }

            retryCount--;
            ConsoleUI.ClearAndWriteLine("Re-trying...", 1250);
        }

        ConsoleUI.DisplayError("Failed to load data after multiple attempts.");
        return false;
    }

}