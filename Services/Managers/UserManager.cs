
namespace PersonalFinanceApp;

public class UserManager
{
    private Dictionary<string, User> users = new Dictionary<string, User>();
    private const string UserPrefix = "USER";
    public int HighestUserId { get; private set; }
    public User CurrentUser { get; private set; }

    public UserManager(int highestUserId)
    {
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
            ? loadedUsers.Max(u => int.Parse(u.UserId.Substring(4)))
            : 0;
    }

    private string GenerateUserId()
    {
        string userNumber = $"{HighestUserId + 1:D6}";
        return UserPrefix + userNumber;
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
        string userId = GenerateUserId();
        User newUser = new User(userId, username, password);
        if (AddNewUser(username, newUser))
        {
            HighestUserId++;
            return AuthenticateUser(username, password);
        }
        return false;
    }

    public bool AuthenticateUser(string username, string password)
    {
        if (users.TryGetValue(username.ToLower(), out User user))
        {
            if (user.Password == password)
            {
                CurrentUser = user;
                return true;
            }
        }
        return false;
    }

    public async Task<bool> SignOut(List<Transaction> transactions, string userId,
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
