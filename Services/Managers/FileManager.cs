
using System.Text.Json;

namespace PersonalFinanceApp;
public class FileManager
{
    private const string UserDataDir = "UserData";
    private const string UserFile = "users.json";

    public void EnsureUserDataDirectoryExists()
    {
        Directory.CreateDirectory(UserDataDir);
    }

    public async Task<bool> SaveToFileAsync(List<Transaction> transactions, string userId)
    {
        EnsureUserDataDirectoryExists();
        string filePath = Path.Combine(UserDataDir, $"{userId}.json");

        try
        {
            string jsonString = JsonSerializer.Serialize(transactions);
            await File.WriteAllTextAsync(filePath, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"An error occurred saving the file: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Transaction>> LoadFromFileAsync(string userId)
    {
        string filePath = Path.Combine(UserDataDir, $"{userId}.json");

        if (!File.Exists(filePath))
        {
            return new List<Transaction>();
        }

        string jsonString = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return new List<Transaction>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Transaction>>(jsonString);
        }
        catch (JsonException ex)
        {
            ConsoleUI.DisplayError($"Error deserializing file: {ex.Message}");
            return new List<Transaction>();
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Unexpected error: {ex.Message}");
            return new List<Transaction>();
        }
    }

    public async Task<bool> SaveUsersAsync(UserManager userManager)
    {
        try
        {
            string filePath = Path.Combine(UserDataDir, UserFile);
            List<User> users = userManager.GetAllUsers();
            string jsonString = JsonSerializer.Serialize(users);
            await File.WriteAllTextAsync(filePath, jsonString);
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"There was an error saving the user: {ex.Message}");
            return false;
        }

        return true;
    }

    public async Task<List<User>> LoadUsersAsync()
    {
        string filePath = Path.Combine(UserDataDir, UserFile);
        if (!File.Exists(filePath))
        {
            return new List<User>();
        }

        string jsonString = await File.ReadAllTextAsync(filePath);
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return new List<User>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<User>>(jsonString);
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"There was an error loading the users: {ex.Message}");
            return new List<User>();
        }
    }
}