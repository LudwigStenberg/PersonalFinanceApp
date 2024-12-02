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
        Console.Clear();
        Console.WriteLine("== Create Account ==\n");

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

            ConsoleUI.DisplayError(errorMessage);
        }

        return (username, password);
    }




    public static (string username, string password) GetExistingUserCredentials()
    {
        Console.Clear();
        Console.WriteLine("== Sign in ==\n");

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




    // Might modularize: Currently long, but imo, clear method - This handles only user input and returns DTO - does not need UserId etc. like Transaction.cs 
    public static TransactionInputDTO GetTransactionInput()
    {
        // Amount validation
        decimal amount;
        while (true)
        {
            amount = InputHandler.GetValidatedDecimalInput("Enter amount: ");
            if (amount <= 0)
            {
                ConsoleUI.DisplayError("Amount must be greater than 0.");
                continue;
            }
            if (amount > 999999999) // Prevent unreasonable amounts!!
            {
                ConsoleUI.DisplayError("Amount is unreasonably large. Please check your input.");
                continue;
            }
            break;
        }

        // Date validation
        DateTime date;
        while (true)
        {
            date = GetValidatedDateInput("Enter date (yyyy-mm-dd) or 'today' for current date: ");


            if (date < DateTime.Now.AddYears(-100))
            {
                ConsoleUI.DisplayError("Date cannot be more than 100 years in the past.");
                continue;
            }

            if (date > DateTime.Now.AddMonths(6))
            {
                ConsoleUI.DisplayError("Date cannot be more than 6 months in the future.");
                continue;
            }
            break;
        }

        // Description validation
        string description;
        while (true)
        {
            description = GetValidatedStringInput("Enter a description (max 40 characters): ");

            if (string.IsNullOrWhiteSpace(description))
            {
                description = "N/A";
                break;
            }

            if (description.Length > 40)
            {
                ConsoleUI.DisplayError("Description cannot be longer than 40 characters.");
                continue;
            }


            if (description.Any(c => char.IsControl(c)))
            {
                ConsoleUI.DisplayError("Description contains invalid characters.");
                continue;
            }
            break;
        }

        // Category selection
        ConsoleUI.DisplayCategories();
        TransactionCategory selectedCategory;
        string customCategoryName = null;

        while (true)
        {
            try
            {
                var (customCategory, category) = GetValidatedCategoryInput("\nSelect a category (1-15): ");

                if (category == TransactionCategory.Custom)
                {
                    // Validate custom category name
                    while (true)
                    {
                        if (string.IsNullOrWhiteSpace(customCategory))
                        {
                            ConsoleUI.DisplayError("Custom category name cannot be empty.");
                            customCategory = GetValidatedStringInput("Enter a custom category name: ");
                            continue;
                        }
                        if (customCategory.Length > 15)
                        {
                            ConsoleUI.DisplayError("Custom category name cannot be longer than 15 characters.");
                            customCategory = GetValidatedStringInput("Enter a custom category name: ");
                            continue;
                        }
                        if (customCategory.Any(c => !char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '_'))
                        {
                            ConsoleUI.DisplayError("Custom category can only contain letters, numbers, spaces, hyphens, and underscores.");
                            customCategory = GetValidatedStringInput("Enter a custom category name: ");
                            continue;
                        }
                        break;
                    }
                    customCategoryName = customCategory;
                }

                selectedCategory = category;
                break;
            }
            catch (Exception)
            {
                ConsoleUI.DisplayError("Invalid category selection. Please try again.");
            }
        }

        // Create new DTO
        return new TransactionInputDTO
        {
            Date = date,
            Amount = amount,
            Category = selectedCategory,
            CustomCategoryName = customCategoryName,
            Description = description
        };
    }




    public static string GetValidatedStringInput(string prompt)
    {

        while (true)
        {
            ConsoleUI.DisplayPrompt(prompt);
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return "N/A";
            }

            if (input.Length > 50)
            {
                ConsoleUI.DisplayError("Input can be no longer than 30 characters.");
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



    public static bool GetRetryChoice() // Do I need this method? SRP?
    {
        Console.WriteLine("The file could not be loaded. Is the file missing?");
        Console.WriteLine("Would you like to retry?\n1. Yes\n2. No");

        if (!byte.TryParse(Console.ReadLine(), out byte choice))
        {
            Console.WriteLine("Invalid input, try again.");
            Thread.Sleep(1500);
            return false;
        }

        return choice == 1;
    }
}
