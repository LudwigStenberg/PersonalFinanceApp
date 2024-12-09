# UPPGIFT: Integration av Databas

## Krav för G
- **Använd Git för versionshantering**  
- **Använd PostgreSQL som databas**:
  - `DatabaseService` hanterar PostgreSQL-anslutning och tabellskapande (`users`, `transactions`).
  - CRUD-operationer implementerade via `DatabaseTransactionStorage` och `TransactionService`.
- **Kontosystem**:
  - Registrering: `UserService.CreateAccount` hashar lösenord och sparar användare.
  - Inloggning: `UserSessionManager.HandleSignIn` autentiserar med hashade lösenord.
  - Utloggning: `UserSessionManager.HandleSignOut` återställer användarsessioner.

## Krav för VG
- **Spara kontoinformation på ett säkert sätt (hashing av lösenord)**:
  - Lösenord hashas med BCrypt i `UserService.CreateAccount` och verifieras i `UserService.AuthenticateUser`.
- **Använd SQL JOINS för datahämtning när det går**:
  - `DatabaseTransactionStorage.LoadTransactionsAsync` använder JOIN för att hämta transaktioner med användardata.
  - `TransactionService.GetGroupedTransactionsDTOAsync` använder GROUP BY för kategoriserad data.
- **Använd minst två SQL TRANSACTIONS**:
  - `DatabaseTransactionStorage.SaveTransactionsAsync` använder transaktioner för batchinserts.
  - `TransactionService.DeleteTransactionsAsync` säkerställer atomisk radering med SQL-transaktioner.
- **Använd alla normalformer (1NF, 2NF, 3NF)**:
  - Tabellskapande följer normalformer i `DatabaseService`.
- **Felhantera alla databasoperationer**:
  - `try/catch`-block hanterar fel i CRUD-metoder i `TransactionService` och `DatabaseService`.
  - `using`-satser säkerställer resurshantering för databasanslutningar.

---

# Egna tankar inför uppgiften

Siktar på att uppnå alla krav för VG, men vill inte lägga mer tid än nödvändigt på projektet. 
Mitt fokus ligger på att lära mig inom ramen för uppgiftens specifikation. När detta är klart 
planerar jag att utforska egna projekt från grunden

---

# Development Log

### November 23 (Saturday)
- Added **Npgsql** package.
- Created `DatabaseManager` class.
- Set up connection specs and SQL table creation for `users`.
- Began implementing `AddUser` method to add new users, using SQL `RETURNING`.
- Updated `User.UserId` from `string` to `int` to match DB type.

### November 25 (Monday)
- Refactored `DatabaseManager.AddUser` to improve error handling.
- Added `try/catch` in `Main` for testing `AddUser`.
- Incorporated BCrypt for secure password handling in `UserService`.

### November 27 (Wednesday)
- Added `transaction_type_enum` and `transactions` tables.
- Refactored SQL commands into constant fields for clarity.
- Simplified database initialization in `DatabaseManager`.

### November 29 (Friday)
- Added `GetUserByUsername` to `DatabaseManager` for retrieving users.
- Refactored `AuthenticateUser` to fetch user data from the database.

### November 30 (Saturday)
- Updated `Transaction.TransactionId` from `string` to `int`.

### December 1 (Sunday)
- Implemented `SaveTransactionsAsync` in `DatabaseTransactionStorage`.

### December 2 (Monday)
- Finalized `SaveTransactionsAsync` with SQL transactions.
- Renamed core services for consistency:
  - `DatabaseManager` → `DatabaseService`
  - `TransactionManager` → `TransactionService`
  - `UserManager` → `UserService`.

### December 3 (Tuesday)
- Consolidated transaction logic into `TransactionService`.
- Updated UI and command-handling methods for better structure.

### December 4 (Wednesday)
- Improved session handling in `UserSessionManager`.
- Temporarily disabled file storage functionality.

### December 5 (Thursday)
- Removed legacy file storage features.
- Updated SQL to replace custom enums with `TEXT` and `CHECK` constraints.

### December 6 (Friday)
- Refactored transaction grouping logic with dynamic SQL `GROUP BY` queries.
- Improved DTOs for handling grouped transaction data.

### December 7 (Saturday)
- Introduced cached transaction data for reduced database queries.
- Added batch transaction deletion functionality with robust UI prompts.

### December 8 (Sunday)
- Enhanced SQL execution methods with better parameter handling.
- Improved cleanup handlers for process termination.

### December 9 (Monday)
- Finalized project with XML comments and cleanup.
- Merged repetitive methods and removed redundant DTOs.

---

# TODO
- Move `CurrentUser` from `UserService` to `UserSessionManager` och refactor accordingly.
- Add proper logging.