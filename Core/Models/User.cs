namespace PersonalFinanceApp;


public class User
{
    public string UserId { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }


    public User(string userId, string username, string password)
    {
        this.UserId = userId;
        this.Username = username;
        this.Password = password;
    }






}
