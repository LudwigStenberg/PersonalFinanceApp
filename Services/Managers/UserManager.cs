
using BCrypt.Net;

namespace PersonalFinanceApp;

public class UserManager
{
    private readonly DatabaseManager _dbManager;
    private Dictionary<string, User> users = new Dictionary<string, User>();
    public int HighestUserId { get; private set; }
    public User CurrentUser { get; private set; }

    public UserManager(DatabaseManager dbManager, int highestUserId)
    {
        _dbManager = dbManager;
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

        if (_dbManager == null)
        {
            throw new InvalidOperationException("DatabaseManager (_dbManager) is not initialized.");
        }

        User newUser = _dbManager.AddUser(username, passwordHashed);
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
        User user = _dbManager.GetUserByUsername(username);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
        {
            CurrentUser = user;
            return true;
        }

        // Authentication failed.
        return false;
    }


    public async Task<bool> SignOut(List<Transaction> transactions, int userId,
                                  FileManager fileManager, UserManager userManager)
    {
        if (!await fileManager.SaveToFileAsync(transactions, userId))
        {
            return false;
        }
        if (!await fileManager.SaveUsersAsync(userManager))
        {
            return false;
        }

        CurrentUser = null;
        return true;
    }
}
