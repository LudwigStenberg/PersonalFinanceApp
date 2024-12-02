﻿namespace PersonalFinanceApp;

public class Transaction
{

    public int TransactionId { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionCategory Category { get; set; }
    public string CustomCategoryName { get; set; }
    public string Description { get; set; }
    public int UserId { get; private set; }

    public Transaction(DateTime date, TransactionType type, decimal amount, TransactionCategory category, string description, int userId)  // Konstruktor som skapar en ny transaktion med angiven typ, datum, belopp och beskrivning
    {
        // TransactionId = transactionId;
        Date = date;
        Type = type;
        Amount = amount;
        Category = category;
        Description = description;
        UserId = userId;
    }

}
