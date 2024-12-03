
namespace PersonalFinanceApp;

public class LoginManager
{
    private readonly UserService _userService;
    private readonly FileManager _fileManager;
    private readonly TransactionService _transactionService;
    private readonly ITransactionStorage _transactionStorage;

    public LoginManager(UserService userService, FileManager fileManager,
                       TransactionService transactionService, ITransactionStorage transactionStorage)
    {
        _userService = userService;
        _fileManager = fileManager;
        _transactionService = transactionService;
        _transactionStorage = transactionStorage;
    }

    public async Task<bool> HandleLogin()
    {
        var (username, password) = InputHandler.GetExistingUserCredentials();
        if (_userService.AuthenticateUser(username, password))
        {
            ConsoleUI.DisplaySuccess("Login successful...");
            ConsoleUI.WelcomeUser(_userService);

            if (await InitializeUserData())
            {
                Console.WriteLine($"{_transactionService.GetTransactionCountAsync(_userService.CurrentUser.UserId)} transactions loaded for {_userService.CurrentUser.Username}");
                if (_transactionService.GetTransactionCount(_userService) == 0)
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

        ConsoleUI.DisplayError("Login failed. Username or Password is incorrect.");
        Thread.Sleep(1500);
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
                ConsoleUI.WelcomeUser(_userService);
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
        Thread.Sleep(1500);
        return false;
    }

    public async Task<bool> HandleSignOut()
    {
        if (_userService.CurrentUser == null)
            return true;

        List<Transaction> transactions = _transactionService.GetCurrentUserTransactions(_userService.CurrentUser.UserId);
        return await _userService.SignOut(
            transactions,
            _userService.CurrentUser.UserId,
            _fileManager,
            _userService
        );
    }

    private async Task<bool> InitializeUserData()
    {
        while (true)
        {
            List<Transaction> loadedTransactions =
                await _transactionStorage.LoadTransactionsAsync(_userService.CurrentUser.UserId);

            Console.WriteLine($"Loaded {loadedTransactions.Count} transactions.");

            if (loadedTransactions.Count == 0)
            {
                if (!InputHandler.GetRetryChoice())
                {
                    return false;
                }
                Console.Clear();
                Console.WriteLine("Re-trying...");
                Thread.Sleep(1000);
                continue;
            }

            _transactionService.InitializeTransactions(loadedTransactions);
            ConsoleUI.DisplaySuccess("Data successfully loaded! Continuing...");
            return true;
        }
    }
}