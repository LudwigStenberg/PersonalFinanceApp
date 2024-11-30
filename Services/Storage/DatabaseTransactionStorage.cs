using Npgsql;

namespace PersonalFinanceApp;

public class DatabaseTransactionStorage : ITransactionStorage
{


    public async Task<List<Transaction>> LoadTransactionsAsync(int userId)
    {

        const string sql = @"
            SELECT transaction_id, date, type, amount, category, description, user_id
            FROM transactions
            WHERE user_id = @UserId
        ";

        var transactions = new List<Transaction>();

        try
        {
            using var cmd = new NpgsqlCommand(sql, DatabaseManager.Connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var transaction = new Transaction(
                    TransactionId = reader.GetInt32(0),
                    Date = reader.GetDateTime(1),
                    Type = Enum.Parse<TransactionType>(reader.GetString(2)),
                    Amount = reader.GetDecimal(3),
                    Category = Enum.Parse<TransactionCategory>(reader.GetString(4)),
                    Description = reader.GetString(5),
                    UserId = reader.GetInt32(6)
                    );

                transactions.Add(transaction);

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading database transactions... {ex.Message}");
            throw;
        }


    }



    public Task<bool> SaveTransactionsAsync(List<Transaction> transactions, int userId)
    {
        throw new NotImplementedException();
    }
}

