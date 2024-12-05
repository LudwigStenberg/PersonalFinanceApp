namespace PersonalFinanceApp;

public class UserService
{
    private readonly DatabaseService _dbService;
    private Dictionary<string, User> users = new Dictionary<string, User>();
    public User CurrentUser { get; private set; }

    public UserService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

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


    public bool SignOut()
    {
        try
        {
            // Reset the current user session.
            CurrentUser = null;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during sign-out: {ex.Message}");
            return false;
        }
    }

}
