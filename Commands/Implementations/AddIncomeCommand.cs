using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;

namespace PersonalFinanceApp;

public class AddIncomeCommand : ICommand
{
    private readonly TransactionManager _transactionManager;
    private readonly UserManager _userManager;
    public AddIncomeCommand(TransactionManager transactionManager, UserManager userManager)
    {
        _transactionManager = transactionManager;
        _userManager = userManager;
    }


    public void Execute()
    {

        Console.Clear();
        Console.WriteLine("== Add Income ==\n");

        TransactionInputDTO transactionData = InputHandler.GetTransactionInput();
        Transaction transaction = _transactionManager.CreateTransaction(transactionData, TransactionType.Income, _userManager.CurrentUser.UserId);
        _transactionManager.AddTransaction(transaction);

        ConsoleUI.DisplaySuccess("\nIncome added successfully.");
        Thread.Sleep(2250);

    }


}
