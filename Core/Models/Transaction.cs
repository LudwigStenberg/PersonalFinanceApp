﻿namespace PersonalFinanceApp;

public class Transaction
{
    public int TransactionId { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; } // Income or Expense
    public decimal Amount { get; set; }
    public TransactionCategory Category { get; set; }
    public string CustomCategoryName { get; set; }
    public string Description { get; set; }
    public int UserId { get; private set; }

    // Constructor for transactions retrieved from the database (with TransactionId).
    public Transaction(int transactionId, DateTime date, TransactionType type, decimal amount,
                       TransactionCategory category, string customCategoryName, string description, int userId)
    {
        TransactionId = transactionId;
        Date = date;
        Type = type;
        Amount = amount;
        Category = category;
        CustomCategoryName = customCategoryName;
        Description = description;
        UserId = userId;
    }

    // Constructor for new transactions (without TransactionId).
    public Transaction(DateTime date, TransactionType type, decimal amount,
                       TransactionCategory category, string customCategoryName, string description, int userId)
    {
        Date = date;
        Type = type;
        Amount = amount;
        Category = category;
        CustomCategoryName = customCategoryName;
        Description = description;
        UserId = userId;
    }
}
