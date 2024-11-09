﻿namespace PersonalFinanceApp;

public class Transaction
{

    public string TransactionId { get; set; } // Could use GUID next time
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionCategory Category { get; set; }
    public string CustomCategoryName { get; set; }
    public string Description { get; set; }
    public string UserId { get; private set; }

    public Transaction(string transactionId, DateTime date, TransactionType type, decimal amount, TransactionCategory category, string description, string userId)  // Konstruktor som skapar en ny transaktion med angiven typ, datum, belopp och beskrivning
    {
        TransactionId = transactionId;
        Date = date;
        Type = type;
        Amount = amount;
        Category = category;
        Description = description;
        UserId = userId;
    }

}
