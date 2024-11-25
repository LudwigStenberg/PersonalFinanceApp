
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
            return AuthenticateUser(username, passwordHashed);
        }
        return false;
    }

    public bool AuthenticateUser(string username, string passwordHashed)
    {
        if (users.TryGetValue(username.ToLower(), out User user))
        {
            if (BCrypt.Net.BCrypt.Verify(passwordHashed, user.HashedPassword))
            {
                CurrentUser = user;
                return true;
            }
        }
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
