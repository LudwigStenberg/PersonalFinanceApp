
using BCrypt.Net;

namespace PersonalFinanceApp;

public class UserService
{
    private readonly DatabaseService _dbService;
    private Dictionary<string, User> users = new Dictionary<string, User>();
    public int HighestUserId { get; private set; }
    public User CurrentUser { get; private set; }

    public UserService(DatabaseService dbService, int highestUserId)
    {
        _dbService = dbService;
        HighestUserId = highestUserId;
    }

    public List<User> GetAllUsers()
    {
        return users.Values.ToList();
    }

    public void LoadUsers(List<User> loadedUsers)
    {
        users.Clear();
        foreach (User user in loadedUsers)
        {
            users[user.Username.ToLower()] = user;
        }

        HighestUserId = loadedUsers.Count > 0
            ? loadedUsers.Max(u => u.UserId)
            : 0;
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
            HighestUserId++;
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


    public async Task<bool> SignOut(List<Transaction> transactions, int userId,
                                  FileManager FileManager, UserService userService)
    {
        if (!await FileManager.SaveToFileAsync(transactions, userId))
        {
            return false;
        }
        if (!await FileManager.SaveUsersAsync(userService))
        {
            return false;
        }

        CurrentUser = null;
        return true;
    }
}
