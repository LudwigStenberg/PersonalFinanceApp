namespace PersonalFinanceApp;


public class User
{
    public int UserId { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }
    public DateTime CreatedAt { get; private set; }


    public User(int userId, string username, string password, DateTime createdAt)
    {
        this.UserId = userId;
        this.Username = username;
        this.Password = password;
        this.CreatedAt = createdAt;
    }
}