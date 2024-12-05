using Npgsql;

namespace PersonalFinanceApp;

public class DatabaseTransactionStorage : ITransactionStorage
{

    private readonly DatabaseService _dbManager;

    public DatabaseTransactionStorage(DatabaseService dbManager)
    {
        _dbManager = dbManager;
    }


    public async Task<List<Transaction>> LoadTransactionsAsync(int userId)
    {
        // Initialization of an empty list to hold the transactions.
        var transactions = new List<Transaction>();

        // SQL Query to select all transactions for the given user ID.
        string sql = @"
            SELECT transaction_id, date, type, amount, category, custom_category_name, description, user_id
            FROM transactions
            WHERE user_id = @UserId
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
                    userId: reader.GetInt32(7)
                );


                transactions.Add(transaction);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading database transactions: {ex.Message}");
            throw;
        }

        return transactions;
    }



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
}

