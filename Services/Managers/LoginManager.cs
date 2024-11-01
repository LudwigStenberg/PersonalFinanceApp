
namespace PersonalFinanceApp;

public class LoginManager
{
    private readonly UserManager _userManager;
    private readonly FileManager _fileManager;
    private readonly TransactionManager _transactionManager;
    private readonly ITransactionStorage _transactionStorage;

    public LoginManager(UserManager userManager, FileManager fileManager,
                       TransactionManager transactionManager, ITransactionStorage transactionStorage)
    {
        _userManager = userManager;
        _fileManager = fileManager;
        _transactionManager = transactionManager;
        _transactionStorage = transactionStorage;
    }

    public async Task<bool> HandleLogin()
    {
        var (username, password) = InputHandler.GetExistingUserCredentials();
        if (_userManager.AuthenticateUser(username, password))
        {
            ConsoleUI.DisplaySuccess("Login successful...");
            ConsoleUI.WelcomeUser(_userManager);

            if (await InitializeUserData())
            {
                Console.WriteLine($"{_transactionManager.GetTransactionCount(_userManager)} transactions loaded for {_userManager.CurrentUser.Username}");
                if (_transactionManager.GetTransactionCount(_userManager) == 0)
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
        if (_userManager.CreateAccount(username, password))
        {
            try
            {
                await _fileManager.SaveUsersAsync(_userManager);
                ConsoleUI.DisplaySuccess("Account creation successful!");
                ConsoleUI.WelcomeUser(_userManager);
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
        if (_userManager.CurrentUser == null)
            return true;

        List<Transaction> transactions = _transactionManager.GetCurrentUserTransactions(_userManager.CurrentUser.UserId);
        return await _userManager.SignOut(
            transactions,
            _userManager.CurrentUser.UserId,
            _fileManager,
            _userManager
        );
    }

    private async Task<bool> InitializeUserData()
    {
        while (true)
        {
            List<Transaction> loadedTransactions =
                await _transactionStorage.LoadTransactionsAsync(_userManager.CurrentUser.UserId);

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

            _transactionManager.InitializeTransactions(loadedTransactions);
            ConsoleUI.DisplaySuccess("Data successfully loaded! Continuing...");
            return true;
        }
    }
}