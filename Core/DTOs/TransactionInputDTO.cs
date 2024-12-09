namespace PersonalFinanceApp
{
    /// <summary>
    /// Represents input data for a transaction.
    /// </summary>
    public class TransactionInputDTO
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public TransactionCategory Category { get; set; }
        public string CustomCategoryName { get; set; }
        public string Description { get; set; }
    }
}
