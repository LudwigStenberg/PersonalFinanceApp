namespace PersonalFinanceApp
{
    /// <summary>
    /// Represents a summary of transactions, including grouping and totals.
    /// </summary>
    public class TransactionSummaryDTO
    {
        public List<Transaction> Transactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetResult { get; set; }
        public string TimeUnit { get; set; }
        public Dictionary<DateTime, List<Transaction>> GroupedTransactionsDTO { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Transactions.Count == 0;
            }
        }

        public TransactionSummaryDTO(string timeUnit)
        {
            Transactions = new List<Transaction>();
            TimeUnit = timeUnit;
            GroupedTransactionsDTO = new Dictionary<DateTime, List<Transaction>>();
        }
    }
}
