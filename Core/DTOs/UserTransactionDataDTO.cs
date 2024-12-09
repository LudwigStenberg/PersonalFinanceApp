namespace PersonalFinanceApp
{
    /// <summary>
    /// Represents user-specific transaction data.
    /// </summary>
    public class UserTransactionDataDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
