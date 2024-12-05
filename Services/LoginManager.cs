
using System.Runtime.CompilerServices;

namespace PersonalFinanceApp;

public class LoginManager
{
    private readonly UserService _userService;
    private readonly CommandManager _commandManager;
    private readonly TransactionService _transactionService;
    private readonly ITransactionStorage _transactionStorage;


    public LoginManager(UserService userService, CommandManager commandManager,
                       TransactionService transactionService, ITransactionStorage transactionStorage)
    {
        _userService = userService;
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
                return true;
            }
        }

        // Authentication failed.
        ConsoleUI.DisplayError("Login failed. Username or Password is incorrect.");
        return false;
    }



    public bool HandleCreateAccount()
    {
        var (username, password) = InputHandler.GetNewUserCredentials();
        if (_userService.CreateAccount(username, password))
        {
            ConsoleUI.DisplaySuccess("Account creation successful!");
            ConsoleUI.WelcomeUser(username);
            return true;
        }

        ConsoleUI.DisplayError("Account creation failed.");
        return false;
    }


    public bool HandleSignOut()
    {
        if (_userService.CurrentUser == null)
        {
            return true;
        }

        return _userService.SignOut();
    }


    private async Task<bool> InitializeUserData()
    {
        List<Transaction> loadedTransactions =
            await _transactionService.GetCurrentUserTransactionsAsync(_userService.CurrentUser.UserId);

        Console.WriteLine($"Loaded {loadedTransactions.Count} transactions.");
        if (loadedTransactions.Count > 0)
        {
            ConsoleUI.DisplaySuccess("Data successfully loaded! Continuing...");
            return true;
        }

        ConsoleUI.DisplayError("No data found for this user.");
        return false;
    }


}