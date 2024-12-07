
namespace PersonalFinanceApp;

public class UserTransactionDataDTO
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public List<Transaction> Transactions { get; set; }
}