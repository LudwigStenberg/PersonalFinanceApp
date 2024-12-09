using Npgsql;

namespace PersonalFinanceApp
{
    /// <summary>
    /// Handles the storage and retrieval of transactions from the database.
    /// </summary>
    public class DatabaseTransactionStorage : ITransactionStorage
    {
        private readonly DatabaseService _dbManager;

        public DatabaseTransactionStorage(DatabaseService dbManager)
        {
            _dbManager = dbManager;
        }

        #region Data Retrieval

        /// <summary>
        /// Loads all transactions for a given user from the database.
        /// </summary>
        public async Task<UserTransactionDataDTO> LoadTransactionsAsync(int userId)
        {
            // Initialization of an empty list to hold the transactions.
            var transactions = new List<Transaction>();
            string username = null;

            // SQL Query to select all transactions for the given user ID.
            string sql = @"
                SELECT t.transaction_id, t.date, t.type, t.amount, t.category, t.custom_category_name, t.description, u.username
                FROM transactions t
                JOIN users u ON t.user_id = u.user_id
                WHERE t.user_id = @UserId
                ";

            try
            {
                // Prepare the SQL command and set the userId parameter.
                using var cmd = new NpgsqlCommand(sql, _dbManager.Connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                // Execute the query and get a reader to iterate over the results.
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    // Map the current row's columns to a Transaction object.
                    var transaction = new Transaction(
                        transactionId: reader.GetInt32(0),
                        date: reader.GetDateTime(1),
                        type: Enum.Parse<TransactionType>(reader.GetString(2)),
                        amount: reader.GetDecimal(3),
                        category: Enum.TryParse<TransactionCategory>(reader.GetString(4), out var category) ? category : throw new InvalidOperationException("Invalid category"),
                        customCategoryName: reader.IsDBNull(5) ? null : reader.GetString(5),
                        description: reader.IsDBNull(6) ? null : reader.GetString(6),
                        userId: userId
                    );

                    username ??= reader.GetString(7); // Assign only once.

                    transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading database transactions: {ex.Message}");
                throw;
            }

            return new UserTransactionDataDTO
            {
                UserId = userId,
                Username = username,
                Transactions = transactions
            };
        }

        #endregion

        #region Data Storage

        /// <summary>
        /// Saves a list of transactions to the database for a specified user.
        /// </summary>
        public async Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId)
        {
            // SQL query to add a new transaction to the database.
            const string sql = @"
                INSERT INTO transactions (date, type, amount, category, custom_category_name, description, user_id)
                VALUES (@Date, @Type, @Amount, @Category, @CustomCategory, @Description, @UserId)
                ";

            using var sqlTransaction = await _dbManager.Connection.BeginTransactionAsync();
            try
            {
                foreach (var t in transactions)
                {
                    using var cmd = new NpgsqlCommand(sql, _dbManager.Connection);
                    cmd.Parameters.AddWithValue("@Date", t.Date);
                    cmd.Parameters.AddWithValue("@Type", t.Type.ToString());
                    cmd.Parameters.AddWithValue("@Amount", t.Amount);
                    cmd.Parameters.AddWithValue("@Category", t.Category.ToString());
                    cmd.Parameters.AddWithValue("@CustomCategory", string.IsNullOrWhiteSpace(t.CustomCategoryName) ? DBNull.Value : t.CustomCategoryName);
                    cmd.Parameters.AddWithValue("@Description", t.Description ?? ((object)DBNull.Value));
                    cmd.Parameters.AddWithValue("@UserId", t.UserId);

                    await cmd.ExecuteNonQueryAsync();
                }

                await sqlTransaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await sqlTransaction.RollbackAsync();
                Console.WriteLine($"Error saving transaction: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
