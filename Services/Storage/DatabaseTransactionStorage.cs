using Npgsql;

namespace PersonalFinanceApp;

public class DatabaseTransactionStorage : ITransactionStorage
{

    private readonly DatabaseManager _dbManager;

    public DatabaseTransactionStorage(DatabaseManager dbManager)
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
        try
        {
            foreach (var transaction in transactions)
            {
                using var cmd = new NpgsqlCommand(sql, _dbManager.Connection);
                cmd.Parameters.AddWithValue("@Date", transaction.Date);
                cmd.Parameters.AddWithValue("@Type", transaction.Type.ToString());
                cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                cmd.Parameters.AddWithValue("@Category", transaction.Category.ToString());
                cmd.Parameters.AddWithValue("@Description", transaction.Description);
                cmd.Parameters.AddWithValue("@UserId", transaction.UserId);

                await cmd.ExecuteNonQueryAsync(); // Executes the SQL insert command.
            }

            return true; // All transactions saved successfully.

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving transactions: .{ex.Message}");
            throw;
        }
    }
}

