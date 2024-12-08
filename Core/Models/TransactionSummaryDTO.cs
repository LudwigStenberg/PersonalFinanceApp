namespace PersonalFinanceApp
{
    public class TransactionSummaryDTO // Flytta till Models? Maybe, but I don't think so.
    {
        public List<Transaction> Transactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetResult { get; set; }
        public string TimeUnit { get; set; }
        public Dictionary<DateTime, List<Transaction>> GroupedTransactions { get; set; }
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
            GroupedTransactions = new Dictionary<DateTime, List<Transaction>>();
        }


    }
}