using System.ComponentModel;

namespace PersonalFinanceApp;

public class AddExpenseCommand : ICommand
{
    private readonly TransactionService _transactionService;
    private readonly UserService _userService;

    public AddExpenseCommand(TransactionService transactionService, UserService userService)
    {
        _transactionService = transactionService;
        _userService = userService;
    }

    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("Add Expense\n");

        TransactionInputDTO transactionData = InputHandler.GetTransactionInput();
        Transaction transaction = _transactionService.CreateTransaction(transactionData, TransactionType.Expense, _userService.CurrentUser.UserId);
        _transactionService.AddTransaction(transaction);
        ConsoleUI.DisplaySuccess("Expense added successfully.");

        Console.ReadKey();


    }
}
