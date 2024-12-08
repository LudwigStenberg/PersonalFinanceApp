using System.Globalization;

namespace PersonalFinanceApp;

public class InputHandler
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 15;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 20;

    public static (string username, string password) GetNewUserCredentials()
    {

        ConsoleUI.DisplayCreateAccountHeader();
        string username;
        string password;


        while (true)
        {
            username = GetValidatedStringInput("  Choose a username: ");
            var (isValid, errorMessage) = ValidateUsername(username);

            if (isValid)
                break;

            ConsoleUI.DisplayError(errorMessage);
        }


        while (true)
        {
            password = GetMaskedPassword("  Choose a password: ");
            var (isValid, errorMessage) = ValidatePassword(password);

            if (isValid)
                break;

            ConsoleUI.DisplayError(errorMessage);
        }

        return (username, password);
    }


    public static (string username, string password) GetExistingUserCredentials()
    {
        ConsoleUI.DisplayLoginHeader();
        string username;
        string password;

        while (true)
        {
            username = GetValidatedStringInput("  Enter your username: ");
            if (!string.IsNullOrWhiteSpace(username))
                break;
            ConsoleUI.DisplayError("  Username cannot be empty.");
        }

        while (true)
        {
            password = GetMaskedPassword("  Enter your password: ");
            if (!string.IsNullOrEmpty(password))
                break;
            ConsoleUI.DisplayError("  Password cannot be empty");
        }

        return (username, password);
    }



    public static (bool isValid, string errorMessage) ValidateUsername(string username)
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

    public static (bool isValid, string errorMessage) ValidatePassword(string password)
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

    public static string GetMaskedPassword(string prompt)
    {
        Console.Write(prompt);
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true); // True = Don't display characters 

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                // Remove last character
                password = password.Substring(0, password.Length - 1);

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



    public static (string customCategory, TransactionCategory selectedCategory) GetValidatedCategoryInput(string prompt)
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


    public static int GetTransactionIndex(int maxIndex)
    {
        while (true)
        {
            ConsoleUI.DisplayPrompt("\nEnter the index of the transaction to remove (or ESC to cancel): ");

            if (CheckForExit())
            {
                return -1;
            }

            if (int.TryParse(Console.ReadLine(), out int index))
            {
                if (index > 0 && index <= maxIndex)
                {
                    return index;
                }
                ConsoleUI.DisplayError($"Please enter a number between 1 and {maxIndex}.");
            }
            else
            {
                ConsoleUI.DisplayError("Invalid input. Please enter a number.");
            }
        }
    }

    public static bool CheckForExit(ConsoleKey exitKey = ConsoleKey.Escape)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        return key.Key == exitKey;
    }

    public static bool CheckForReturn(ConsoleKey userChoice)
    {
        if (userChoice == ConsoleKey.Escape)
        {
            return true;
        }

        return false;
    }

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

    // Step 4: Handle Category Selection
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

    // Helper for validating custom category
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



    public static string GetValidatedStringInput(string prompt, int charLimit = 30)
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



    public static decimal GetValidatedDecimalInput(string prompt)
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



    public static DateTime GetValidatedDateInput(string prompt)
    {
        while (true)
        {
            ConsoleUI.DisplayPrompt(prompt); // Enter a date in format (yyyy-MM-dd):
            string userInput = Console.ReadLine();

            if (userInput == "today")
            {
                return DateTime.Today;
            }
            if (DateTime.TryParseExact(userInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            ConsoleUI.DisplayError("Invalid entry. Enter date as yyyy-mm-dd or 'today'");
        }
    }

}
