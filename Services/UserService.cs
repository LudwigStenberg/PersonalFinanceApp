namespace PersonalFinanceApp
{
    /// <summary>
    /// Service class for managing user-related operations.
    /// </summary>
    public class UserService
    {
        private readonly DatabaseService _dbService;
        private Dictionary<string, User> users = new Dictionary<string, User>();
        public User CurrentUser { get; private set; }

        public UserService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        #region User Management

        /// <summary>
        /// Adds a new user to the user dictionary.
        /// </summary>
        public bool AddNewUser(string username, User newUser)
        {
            if (newUser == null || username == null || string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (users.ContainsKey(username.ToLower()))
            {
                return false;
            }

            users.Add(username.ToLower(), newUser);
            return true;
        }

        /// <summary>
        /// Creates a new user account with the specified username and password.
        /// </summary>
        public bool CreateAccount(string username, string password)
        {
            string passwordHashed = BCrypt.Net.BCrypt.HashPassword(password);

            if (_dbService == null)
            {
                throw new InvalidOperationException("DatabaseService (_dbService) is not initialized.");
            }

            User newUser = _dbService.AddUser(username, passwordHashed);
            if (AddNewUser(username, newUser))
            {
                return AuthenticateUser(username, password);
            }
            return false;
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Authenticates a user by verifying the username and password.
        /// </summary>
        public bool AuthenticateUser(string username, string password)
        {
            // Read user from the DB.
            User user = _dbService.GetUserByUsername(username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                CurrentUser = user;
                return true;
            }

            // Authentication failed.
            return false;
        }

        /// <summary>
        /// Signs out the current user and resets the session.
        /// </summary>
        public bool SignOut()
        {
            try
            {
                CurrentUser = null;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sign-out: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
