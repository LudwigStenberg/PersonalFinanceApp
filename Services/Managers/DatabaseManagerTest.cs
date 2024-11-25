using BCrypt.Net;
using PersonalFinanceApp;

public class DatabaseManagerTest
{
    public void TestAddUser()
    {
        var dbManager = new DatabaseManager();

        string username = "test_user";
        string password = "seccurepassword";

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        User newUser = dbManager.AddUser(username, hashedPassword);

        if (newUser != null)
        {
            Console.WriteLine($"User added successfully:");
            Console.WriteLine($"ID: {newUser.UserId}");
            Console.WriteLine($"Username: {newUser.Username}");
            Console.WriteLine($"Created At: {newUser.CreatedAt}");
        }
        else
        {
            Console.WriteLine("User insertion failed.");
        }

    }
}