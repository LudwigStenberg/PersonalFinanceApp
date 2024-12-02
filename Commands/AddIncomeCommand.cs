using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;

namespace PersonalFinanceApp;

public class AddIncomeCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly UserService _userService;
    public AddIncomeCommand(TransactionService transactionManager, UserService userService)
    {
        _transactionService = transactionManager;
        _userService = userService;
    }


    public void Execute()
    {

        Console.Clear();
        Console.WriteLine("== Add Income ==\n");

        TransactionInputDTO transactionData = InputHandler.GetTransactionInput();
        Transaction transaction = _transactionService.CreateTransaction(transactionData, TransactionType.Income, _userService.CurrentUser.UserId);
        _transactionService.AddTransaction(transaction);

        ConsoleUI.DisplaySuccess("\nIncome added successfully.");
        Thread.Sleep(2250);

    }


}
