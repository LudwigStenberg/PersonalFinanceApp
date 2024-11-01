Ludwig Stenberg


[Guide]
***  =  High priority
**   =  Medium priority
*    =  Low priority
X    =  Completed

-------------

== Core Features ==
1. ShowTransactions
    - Remove Transaction
2. Add Income
3. Add Expense
4. Account Details
5. Options
    - CurrencyChanger
Esc. Save & Exit

-------------

== Completed ==


X - 1. ShowTransactions
X - 1.1 Remove Transaction
X - 2. Add Income
X - 3. Add Expense
  - 4 Log-In feature with accounts.
    - 4.1 Account Details
  - 5. Options
     - 5.1 CurrencyChanger
X  - 7. Esc. Save & Exit

X - Change property name back to original
X - DisplayCategories()
X - GetValidatedCategoryInput with tuple return



-------------

== To do == 

X - Co-operative function for Add Income / Add Expense
X - Finish TransactionInputHandler
X - Finish Add Income Logic
X - Finish Add Expense Logic

- Error handling Add Income
- Error handling Add Expense
- General error handling
- UI for Add Income
- UI for Add Expense

- Finish CommandManager
- Allow dates to be entered as e.g 2024-03-20 and 16/7 if possible.
- Allow User to press 'X' to automatically select the current date.
- Add string validation for custom category within GetValidatedCategoryInput
- Create a class for categories and remove use of tuples (e.g. in GetTransactionInputHandler)
- Always allow the user to go back by hitting escape - maybe with a prompt as well "are you sure?"
- Allow shortcut option to pick today's date.
- Also, display today's date in the hud at all times? Maybe.
- Remove Transaction method  - Where should this go? Probably an option within feature to display transactions
- Account Balance
- Account Details
- Options (CurrencyChanger, EUR, SEK, USD, POUNDS..?)
- Refactor tuple return from GetValidatedCategoryInput into an object from a Category class which handles more of the category logic.
- Search function
- Evaluate entire project to achieve abstraction, (well, good OO-Design in general)
- File manager
- Display all the necessary information with ShowTransactions - type, date, amount, description, category..
- DisplayTransaction: Change color of the currently active view.
- Custom category padding between category/description in transaction view - maximum chars to not destroy formating?
- Implement filtered transactions?
- Change month view to display "year, month" yyyy - MMMM
- Consider adding two types of IDs - "composite key" one as a property of the Transaction class and another for sequentially displayed for the User for better UX. 
- Lägg till fler sorting features e.g. transactions.OrderBy(t => t.Type/Category/Amount..).ToList() 
- Byta PrepareTransactionData till non-static och flytta den till FinancialManager.
- Refaktorisera delar av PrepareTransactionData till FinancialManager som ex. CalculateTotalIncome/Expenses (gör nya metoder i FM)
- TransactionId/UI-Index
- Finish implementation of TransactionId and getting it to work with a separate UI-index
- Create a Esc method to go back in every single menu.
- Issue with category number mapping. User input does not reflect the correct category. e.g. 4. Healthcare = Utilities.
- Lägg till både save user och saveusertransactions(saveTofile) i en och samma metod e.g. ConsoleUI.SaveUserData 



-------------

== Questions == 
1. Is the Transaction class reundant atm? Does the GroupedTransaction take presidence and should they be merged or adapted?
2. Should I change this to readonly?  private List<Transaction> transactions = new List<Transaction>(); // field - Why? or why not?
3. Are GetAccountBalance and GetTotalIncome/Expenses overlapping?
4. Is TransactionManager following SRP?

-------------

== Documentation ==
Oct 6
Renamed TransactionInputHandler to CreateTransaction and moved it to FinanceManager. Its utilises the other input 
handling methods but its own responsibility is to create a new transaction from the Transaction class, not handling input.
Both add income and Add Expense will be able to make use of it and will only need to input if its an income or expense.

- Changed class name from TransactionSummary to TransactionData. At this time I believe it's a better suited name.
- Added GroupedTransactions class
- Learned a bit about what's called ternary operators in C# which I plan to include for my GroupedTransactions


-------------

Oct 9
- Added property TransactionId to Transaction class. 
- Created a method GenerateTransactionId - string based
- Renamed FinanceManager to TransactionManager
- Added a private dictionary property to TransactionManager to help track the transaction IDs.
- Refactored the TransactionManager class
    - Encapsulated the transactions list, making it private.
    - Added methods like GetTransactionCount() and RemoveTransactionAt() to interact with the transactions list.
    - Created a GetOrderedTransactions() method for sorting transactions.

- Updated DisplayBalanceCommand to work with the new TransactionManager structure.
- Modified HandleRemoveTransaction to use TransactionManager methods instead of directly accessing the transactions list.
- Started updating TransactionSummary.PrepareTransactionData to work with TransactionManager:
    - Changed how we check for empty transactions list.
    - Used TransactionManager's GetOrderedTransactions method.


-------------
Oct 13
Created FileManager class currently supporting json.
    - Added SaveToFile
    - Added LoadFromFile
Integrated SaveToFile into the Save & Exit feature with ConsoleUI.SaveOnExit method.
Integrated LoadFromFile into the program startup with ConsoleUI.LoadOnStart method.
Implemented basic structure for UserManager class
Created GenerateUserId method with proper formatting and prefix
Designed initial version of CreateAccount method
Drafted basic login and account creation UI in ConsoleUI

To do:
Refine and modularize login and account creation process in ConsoleUI
Implement AuthenticateUser method in UserManager
Add error handling and input validation for login and account creation
Integrate user management with existing transaction system
Update file operations to handle user data and HighestUserId
Implement logout functionality
Test user creation, login, and logout processes

-------------
Oct 14

Continuation on Login features.
Currently have a list to check for username/password - would it be better to change to a Dictionary?

-------------
Oct 17
Issue with category number mapping. User input does not reflect the correct category. e.g. 4. Healthcare = Utilities.
Worked on UserManager and FileManager 
    - worked on Save to and Load from file from user data 
    - added save and load feature for users for e.g. account info
    - fixed lots of bugs around the login/logout features

to do:
fix user transactions not saving anything to UserData
-------------
Oct 24
- Created folder for Enums and moved Category and Transaction type away from the transaction class and into their own.
- Refactored CreateTransactionFromUserInput to GetTransactionInput
- Created DTO object TransactionInputDTO to encapsulate and make it easier to transfer only necessary data. 
- Created Models folder to store models. TransactionSummary may or may not go there. It has some business logic.
- Created Services folder with Interface/ and Implementation/
- Moved GenerateTransactionID to Services -> TransactionIdGenerator
- added Interface/IIdGeneratorService
- Added private const string TRANSACTION_PREFIX to TransactionIdGenerator p
- Moved transactionCount dictionary to TransactionIdGenerator and made it private readonly, readonly for extra security (readonly - no new instances can be created)
- Added Implementation/FileTransactioNStorage.cs
- Added Interfaces/ITransactionStorage.cs

Refactoring idea: SoC/SRP/Data Persistence
FileTransactionStorage will:
- User FileManager to read/write files
- Handle the logic of what transactions belong to which user
- Manage how transactions are stored/retrieved
TransactionManager will:
- Get rid of e.g. LoadTransactions
- Focus on mainly CRUD
- Handle business logic around transactions specifically
- Not need to think about how/where the data is stored/persisted


To do:
Remove data/storage responsibility from TransactionManager and separating FileManager's file operation from the actual storing of data.
- Currently: Create ITransactionStorage and move LoadTransactions from TM. 
    - Declare LoadTransactions method
    - Implement it in LoadTransactionsStorage
    - What is async? Does it have a use here - learn about it.
Fix errors that occurred due to changing categories to its own enum class.
Add logging system - e.g for Loading/saving erros

-------------
Oct 26
Implemented ITransactionStorage interface
Separated storage logic from file operations
Added async operations
Uses FileManager for raw file operations
Refactored FileManager
Made file operations async (LoadUsersAsync, SaveUsersAsync)
Clear separation of concerns
Updated TransactionManager
Added ITransactionStorage dependency
Better dependency management
Modified Program class

To Do:
Error Handling
Add more specific exception types
Implement proper logging system
Consider creating custom exceptions

Technical Debt?
Replace Console.WriteLine with proper logging
Add cancellation token support for async operations ?
Add retry mechanisms for file operations 

MAYBE - If needs grow: Add IUserStorage / implementation - Reason: While it could be considered overkill due to the small size of this program, I think it adheres to the program structure that has already been established. We already have several interfaces for services. Also I think it will be easier to switch from file to database as well. So more flexibility, better structure and I want to practice it. But ultimately, the current need for it is too small and I don't really need it now, so it's on hold.

-------------
Nov 1
Accidentally removed all my managers and lost a lot of work....
E.g. Lots of stuff Login/Create account validation
Re-added what I could remember of it.

Moved input handling: ConsoleUI -> InputHandler
Fixed category display/selection mismatch
Standardized messages:
Added these where necessary:
- ConsoleUI.DisplayError for errors
- ConsoleUI.DisplaySuccess for confirmations

Refactored RemoveTransactionCommand and cleared up DisplayTransactionCommand

Also:
- Removed unused CurrencyChanger (due to time limitations)
- Organized file structure (Core/Services/UI)
- Interfaces near implementations
- Removed "dead" code where I could find it.

To do: 
***! Add basic exception handling (too many generic catches)
***Add XML(?) comments to main classes/interfaces
Add Regions?

Check TransactionDateHelper for unused methods
Consider simpler date input (allow "today" input)
Add escape option in all menus
Consider basic logging instead of console errors
Password encryption (currently stored as plain text)
------------