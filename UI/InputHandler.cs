using System.Globalization;

namespace PersonalFinanceApp;

/// <summary>
/// Handles user input and validation for the Personal Finance App.
/// Includes methods for collecting credentials, transaction details, and category input.
/// </summary>
public class InputHandler
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 15;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 20;

    #region Public methods

    /// <summary>
    /// Checks if the input key matches the target key, with Escape (ESC) as the default target.
    /// </summary>
    public static bool CheckForKey(ConsoleKey inputKey, ConsoleKey targetKey = ConsoleKey.Escape)
    {
        return inputKey == targetKey;
    }

    /// <summary>
    /// Collects and validates input for a new transaction.
    /// </summary>
    /// 
    public static TransactionInputDTO GetTransactionInput()
    {
        // Gather input in steps
        decimal amount = GetValidatedAmount();
        DateTime date = GetValidatedDate();
        string description = GetValidatedDescription();
        var (selectedCategory, customCategoryName) = GetCategorySelection();

        // Create and return the DTO
        return new TransactionInputDTO
        {
            Date = date,
            Amount = amount,
            Category = selectedCategory,
            CustomCategoryName = customCategoryName,
            Description = description
        };
    }

    /// <summary>
    /// Displays a list of transactions and prompts the user to select one for removal.
    /// Returns the index of the selected transaction or -1 if the operation is canceled.
    /// </summary>
    public static int GetTransactionRemovalIndex(TransactionSummaryDTO summary)
    {
        ConsoleUI.DisplayTransactionsByIndividual(summary, true);
        return GetTransactionIndex(summary.Transactions.Count);
    }

    /// <summary>
    /// Collects and validates new user credentials.
    /// </summary>
    public static (string username, string password) GetNewUserCredentials()
    {

        ConsoleUI.DisplayCreateAccountHeader();
        string username;
        string password;


        while (true)
        {
            username = GetValidatedStringInput("Choose a username: ");
            var (isValid, errorMessage) = ValidateUsername(username);

            if (isValid)
                break;

            ConsoleUI.DisplayError(errorMessage);
        }


        while (true)
        {
            password = GetMaskedPassword("Choose a password: ");
            var (isValid, errorMessage) = ValidatePassword(password);

            if (isValid)
                break;

            ConsoleUI.DisplayError(errorMessage, 500);
        }

        return (username, password);
    }

    /// <summary>
    /// Collects and validates credentials for an existing user.
    /// </summary>
    public static (string username, string password) GetExistingUserCredentials()
    {
        ConsoleUI.DisplayLoginHeader();
        string username;
        string password;

        while (true)
        {
            username = GetValidatedStringInput("Enter your username: ");
            if (!string.IsNullOrWhiteSpace(username))
                break;
            ConsoleUI.DisplayError("Username cannot be empty.");
        }

        while (true)
        {
            password = GetMaskedPassword("Enter your password: ");
            if (!string.IsNullOrEmpty(password))
                break;
            ConsoleUI.DisplayError("Password cannot be empty");
        }

        return (username, password);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Prompts the user to enter a transaction amount and validates it.
    /// Ensures the amount is greater than zero and within a reasonable range.
    /// </summary>
    private static decimal GetValidatedAmount()
    {
        while (true)
        {
            decimal amount = GetValidatedDecimalInput("Enter amount: ");
            if (amount <= 0)
            {
                ConsoleUI.DisplayError("Amount must be greater than 0.");
            }
            else if (amount > 999999999)
            {
                ConsoleUI.DisplayError("Amount is unreasonably large. Please check your input.");
            }
            else
            {
                return amount;
            }
        }
    }

    /// <summary>
    /// Collects and validates a date, defaulting to today if none is provided.
    /// </summary>
    private static DateTime GetValidatedDate()
    {
        while (true)
        {
            ConsoleUI.DisplayPrompt("Enter date (yyyy-mm-dd) or press Enter for today's date: ");
            string userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                // Pressing Enter defaults to today's date
                return DateTime.Today;
            }
            if (DateTime.TryParseExact(userInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                if (date < DateTime.Now.AddYears(-100))
                {
                    ConsoleUI.DisplayError("Date cannot be more than 100 years in the past.");
                }
                else if (date > DateTime.Now.AddMonths(6))
                {
                    ConsoleUI.DisplayError("Date cannot be more than 6 months in the future.");
                }
                else
                {
                    return date; // Valid date within range
                }
            }
            else
            {
                ConsoleUI.DisplayError("Invalid entry. Enter date as yyyy-mm-dd or press Enter for today's date.");
            }
        }
    }

    /// <summary>
    /// Collects and validates a description for a transaction.
    /// Ensures it does not exceed the character limit and avoids invalid characters.
    /// </summary>
    private static string GetValidatedDescription()
    {
        while (true)
        {
            string description = GetValidatedStringInput("Enter a description (max 40 characters): ", 40);
            if (string.IsNullOrWhiteSpace(description))
            {
                return "N/A";
            }
            if (description.Any(c => char.IsControl(c)))
            {
                ConsoleUI.DisplayError("Description contains invalid characters.");
            }
            else
            {
                return description;
            }
        }
    }

    /// <summary>
    /// Displays the category options and collects the user's selection.
    /// Handles both predefined and custom categories.
    /// </summary>
    private static (TransactionCategory selectedCategory, string customCategoryName) GetCategorySelection()
    {
        ConsoleUI.DisplayCategories();

        while (true)
        {
            try
            {
                var (customCategory, category) = GetValidatedCategoryInput("\nSelect a category (1-15): ");
                if (category == TransactionCategory.Custom)
                {
                    string validatedCustomCategory = ValidateCustomCategory(customCategory);
                    return (category, validatedCustomCategory);
                }
                return (category, null);
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayError($"Invalid category selection: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Collects a masked password input from the user, hiding the characters as they type.
    /// </summary>
    private static string GetMaskedPassword(string prompt)
    {
        Console.Write(prompt);
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1); // Remove last character

                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    /// <summary>
    /// Validates and retrieves the user's category input.
    /// Handles predefined and custom categories.
    /// </summary>
    private static (string customCategory, TransactionCategory selectedCategory) GetValidatedCategoryInput(string prompt)
    {

        while (true)
        {
            ConsoleUI.DisplayPrompt(prompt);

            if (byte.TryParse(Console.ReadLine(), out byte userChoice))
            {
                if (userChoice >= 1 && userChoice <= 14)
                {
                    return (null, (TransactionCategory)(userChoice - 1));
                }
                else if (userChoice == 15)
                {
                    while (true)
                    {
                        ConsoleUI.DisplayPrompt("Enter a custom category: "); // Add validation here
                        string customCategory = Console.ReadLine();
                        if (customCategory.Length > 15)
                        {
                            ConsoleUI.DisplayError("A custom category cannot be longer than 15 characters.");
                            continue;
                        }
                        return (customCategory, TransactionCategory.Custom);
                    }
                }
            }

            ConsoleUI.DisplayError("Invalid input. Enter a number between 1-15.");
        }
    }

    /// <summary>
    /// Prompts the user to enter a transaction index for removal.
    /// Allows cancellation by pressing the Escape (ESC) key.
    /// </summary>
    private static int GetTransactionIndex(int maxIndex)
    {
        while (true)
        {
            Console.WriteLine("Enter the index of the transaction to remove (or press ESC to cancel): ");

            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                return -1;
            }

            // If it's not ESC, display the pressed key and allow numeric input
            Console.Write(keyInfo.KeyChar);
            string input = keyInfo.KeyChar + Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int index))
            {
                if (index > 0 && index <= maxIndex)
                {
                    return index;
                }
                ConsoleUI.DisplayError($"Please enter a number between 1 and {maxIndex}.");
            }
            else
            {
                ConsoleUI.DisplayError("Invalid input. Please enter a valid number.");
            }
        }
    }

    /// <summary>
    /// Collects a string input with optional length validation.
    /// </summary>
    private static string GetValidatedStringInput(string prompt, int charLimit = 30)
    {
        while (true)
        {
            ConsoleUI.DisplayPrompt(prompt);
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (input.Length > charLimit)
            {
                ConsoleUI.DisplayError($"Input can be no longer than {charLimit} characters.");
            }
            else
            {
                return input;
            }
        }
    }

    /// <summary>
    /// Prompts the user to enter a decimal value and validates the input.
    /// Ensures the input is a valid decimal number.
    /// </summary>
    private static decimal GetValidatedDecimalInput(string prompt)
    {
        while (true)
        {
            ConsoleUI.DisplayPrompt(prompt);

            if (decimal.TryParse(Console.ReadLine(), out decimal result))
            {
                return result;
            }

            ConsoleUI.DisplayError("Invalid Input. Please try again. ");

        }

    }

    /// <summary>
    /// Validates the format and rules for usernames.
    /// </summary>
    private static (bool isValid, string errorMessage) ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username cannot be empty");


        if (username.Length < MinUsernameLength)
            return (false, $"Username must be at least {MinUsernameLength} characters.");

        if (username.Length > MaxUsernameLength)
            return (false, $"Username cannot be more than {MaxUsernameLength} characters.");

        if (username.Contains(' '))
            return (false, $"Username cannot contain spaces.");

        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
            {
                return (false, "Username can only contain letters, numbers, underscores and dots.");
            }
        }

        return (true, string.Empty);

    }

    /// <summary>
    /// Validates the format and rules for passwords.
    /// </summary>
    private static (bool isValid, string errorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return (false, "Password cannot be empty.");

        if (password.Length < MinPasswordLength)
            return (false, $"Password must be at least {MinPasswordLength} characters");

        if (password.Length > MaxPasswordLength)
            return (false, $"Password cannot be more than {MaxPasswordLength} characters");

        if (password.Contains(' '))
            return (false, "Password cannot contain spaces");

        bool hasUpper = false;
        bool hasLower = false;
        bool hasNumber = false;
        bool hasSpecial = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (char.IsDigit(c)) hasNumber = true;
            if (!char.IsLetterOrDigit(c)) hasSpecial = true;
        }

        if (!hasUpper)
            return (false, "Password must contain at least one uppercase letter.");

        if (!hasLower)
            return (false, "Password must contain at least one lowercase letter.");

        if (!hasNumber)
            return (false, "Password must contain at least one number.");

        if (!hasSpecial)
            return (false, "Password must contain at least one special character");

        return (true, string.Empty);

    }

    /// <summary>
    /// Validates a custom category name entered by the user.
    /// Ensures the name is not empty, does not exceed the character limit, and contains only valid characters.
    /// </summary>
    private static string ValidateCustomCategory(string customCategory)
    {
        while (true)
        {
            if (string.IsNullOrWhiteSpace(customCategory))
            {
                ConsoleUI.DisplayError("Custom category name cannot be empty.");
                customCategory = GetValidatedStringInput("Enter a custom category name: ");
            }
            else if (customCategory.Length > 15)
            {
                ConsoleUI.DisplayError("Custom category name cannot be longer than 15 characters.");
                customCategory = GetValidatedStringInput("Enter a custom category name: ");
            }
            else if (customCategory.Any(c => !char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '_'))
            {
                ConsoleUI.DisplayError("Custom category can only contain letters, numbers, spaces, hyphens, and underscores.");
                customCategory = GetValidatedStringInput("Enter a custom category name: ");
            }
            else
            {
                return customCategory;
            }
        }
    }

    #endregion
}
