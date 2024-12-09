namespace PersonalFinanceApp
{
    /// <summary>
    /// Defines the interface for all commands in the application.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command's logic.
        /// </summary>
        Task Execute();
    }
}
