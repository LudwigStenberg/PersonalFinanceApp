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

        if (_userService.AuthenticateUser(username, password))
        {
            ConsoleUI.DisplaySuccess("  Login successful...");

            if (_userService.CurrentUser == null)
            {
                ConsoleUI.DisplayError("User authentication succeeded, but CurrentUser is not set.");
                return false;
            }

            // Fetch and cache user data
            UserTransactionDataDTO userData = await _transactionService.GetUserTransactionDataAsync(_userService.CurrentUser.UserId);

            if (userData.Transactions.Count > 0)
            {
                ConsoleUI.DisplaySuccess($"  Data successfully loaded: {userData.Transactions.Count} transactions.");
                return true;
            }

            ConsoleUI.DisplayError("No data found for this user.");
            return false;
        }

        ConsoleUI.DisplayError("Login failed. Username or Password is incorrect.");
        return false;
    }


    public bool HandleCreateAccount()
    {
        var (username, password) = InputHandler.GetNewUserCredentials();
        if (_userService.CreateAccount(username, password))
        {
            ConsoleUI.DisplaySuccess("Account creation successful!");
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
}