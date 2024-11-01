using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PersonalFinanceApp
{
    public class TransactionSummary // Flytta till Models? Maybe, but I don't think so.
    {
        public List<Transaction> Transactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetResult { get; set; }
        public string TimeUnit { get; set; }
        public Dictionary<string, List<Transaction>> GroupedTransactions { get; set; }
        public bool IsEmpty
        {
            get
            {
                return Transactions.Count == 0;
            }
        }

        public TransactionSummary(string timeUnit)
        {
            Transactions = new List<Transaction>();
            TimeUnit = timeUnit;
            GroupedTransactions = new Dictionary<string, List<Transaction>>();
        }


    }
}