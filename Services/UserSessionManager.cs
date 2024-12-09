namespace PersonalFinanceApp
{
    /// <summary>
    /// Manages user sessions, including login, account creation, and sign-out.
    /// </summary>
    public class UserSessionManager
    {
        private readonly UserService _userService;
        private readonly CommandManager _commandManager;
        private readonly TransactionService _transactionService;
        private readonly ITransactionStorage _transactionStorage;

        public UserSessionManager(UserService userService, CommandManager commandManager,
                           TransactionService transactionService, ITransactionStorage transactionStorage)
        {
            _userService = userService;
            _commandManager = commandManager;
            _transactionService = transactionService;
            _transactionStorage = transactionStorage;
        }

        #region Login and Account Management

        /// <summary>
        /// Handles user login by verifying credentials and loading transaction data.
        /// </summary>
        public async Task<bool> HandleSignIn()
        {
            var (username, password) = InputHandler.GetExistingUserCredentials();

            if (_userService.AuthenticateUser(username, password))
            {
                ConsoleUI.DisplaySuccess("Login successful...");

                if (_userService.CurrentUser == null)
                {
                    ConsoleUI.DisplayError("User authentication succeeded, but CurrentUser is not set.");
                    return false;
                }

                // Fetch and cache user data
                UserTransactionDataDTO userData = await _transactionService.GetUserTransactionDataAsync(_userService.CurrentUser.UserId);
                if (userData.Transactions.Count > 0)
                {
                    ConsoleUI.DisplaySuccess($"Data successfully loaded: {userData.Transactions.Count} transactions.");
                    return true;
                }
            }

            ConsoleUI.DisplayError("Login failed. Username or Password is incorrect.");
            return false;
        }

        /// <summary>
        /// Handles user account creation by collecting and processing credentials.
        /// </summary>
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

        #endregion

        #region Session Management

        /// <summary>
        /// Handles user sign-out by resetting the current session.
        /// </summary>
        public bool HandleSignOut()
        {
            if (_userService.CurrentUser == null)
            {
                ConsoleUI.DisplayError("No user is currently signed in.");
                return false;
            }

            ConsoleUI.DisplaySuccess($"Signing out user: {_userService.CurrentUser.Username}");
            _userService.SignOut();
            return true;
        }


        #endregion
    }
}
