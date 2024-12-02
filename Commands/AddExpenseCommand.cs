using System.ComponentModel;

namespace PersonalFinanceApp;

public class AddExpenseCommand : ICommand
{
    private readonly TransactionManager _transactionManager;
    private readonly UserService _userManager;

    public AddExpenseCommand(TransactionManager transactionManager, UserService userManager)
    {
        _transactionManager = transactionManager;
        _userManager = userManager;
    }

    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("Add Expense\n");

        TransactionInputDTO transactionData = InputHandler.GetTransactionInput();
        Transaction transaction = _transactionManager.CreateTransaction(transactionData, TransactionType.Expense, _userManager.CurrentUser.UserId);
        _transactionManager.AddTransaction(transaction);
        ConsoleUI.DisplaySuccess("Expense added successfully.");

        Console.ReadKey();


    }
}
