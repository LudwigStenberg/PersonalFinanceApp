namespace PersonalFinanceApp;


public class User
{
    public int UserId { get; private set; }
    public string Username { get; private set; }
    public string HashedPassword { get; private set; }
    public DateTime CreatedAt { get; private set; }


    public User(int userId, string username, string hashedPassword)
    {
        this.UserId = userId;
        this.Username = username;
        this.HashedPassword = hashedPassword;
    }
}