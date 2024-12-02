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

        // SQL Query to select all transactions for the given user ID.
        const string sql = @"
            SELECT transaction_id, date, type, amount, category, description, user_id
            FROM transactions
            WHERE user_id = @UserId
        ";

        // Initialization of an empty list to hold the transactions.
        var transactions = new List<Transaction>();

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
                    type: Enum.TryParse<TransactionType>(reader.GetString(2), out var type) ? type : throw new InvalidOperationException("Invalid transaction type."),
                    amount: reader.GetDecimal(3),
                    category: Enum.TryParse<TransactionCategory>(reader.GetString(4), out var category) ? category : throw new InvalidOperationException("Invalid category"),
                    description: reader.GetString(5),
                    userId: reader.GetInt32(6)
                    );

                // Add the Transaction object to the list.
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
            INSERT INTO transactions (date, type, amount, category, description, user_id)
            VALUES (@Date, @Type, @Amount, @Category, @Description, @UserId)
        ";

        using var transaction = await _dbManager.Connection.BeginTransactionAsync();
        try
        {
            foreach (var txn in transactions)
            {
                using var cmd = new NpgsqlCommand(sql, _dbManager.Connection);
                cmd.Parameters.AddWithValue("@Date", txn.Date);
                cmd.Parameters.AddWithValue("@Type", txn.Type.ToString());
                cmd.Parameters.AddWithValue("@Amount", txn.Amount);
                cmd.Parameters.AddWithValue("@Category", txn.Category.ToString());
                cmd.Parameters.AddWithValue("@Description", txn.Description ?? ((object)DBNull.Value));
                cmd.Parameters.AddWithValue("@UserId", txn.UserId);

                await cmd.ExecuteNonQueryAsync(); // Executes the SQL insert command.
            }

            await transaction.CommitAsync(); // Commit if all transactions succeed.
            return true;

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Rollback on error.
            Console.WriteLine($"Error saving transaction: {ex.Message}");
            return false;
        }
    }
}

