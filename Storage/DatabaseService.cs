using Npgsql;
using PersonalFinanceApp;


public class DatabaseService : IDisposable
{
    private readonly NpgsqlConnection connection;

    public NpgsqlConnection Connection
    {
        get { return connection; }
    }

    private readonly string _connectionString =
            "Host=localhost; Port=5432; Database=PersonalFinanceApp; Username=postgres; Password=assword;";

    private const string CreateUsersTableSql = @"CREATE TABLE IF NOT EXISTS users (
            user_id SERIAL PRIMARY KEY,
            username VARCHAR(50) UNIQUE NOT NULL,
            password_hashed TEXT NOT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL)";

    private const string CreateTransactionsTableSql = @"
            CREATE TABLE IF NOT EXISTS transactions (
            transaction_id SERIAL PRIMARY KEY,
            user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            date DATE NOT NULL,
            amount DECIMAL(10, 2) NOT NULL,
            type TEXT NOT NULL CHECK (type IN ('Income', 'Expense')),
            category TEXT NOT NULL,
            custom_category_name VARCHAR(20),
            description VARCHAR(50))";

    public DatabaseService()
    {
        try
        {
            this.connection = new NpgsqlConnection(_connectionString);
            connection.Open();
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Error initializing database: {ex.Message}");
            throw;
        }
    }

    public static async Task<DatabaseService> CreateAsync()
    {
        var dbService = new DatabaseService();
        await dbService.InitializeDatabaseAsync();
        return dbService;
    }

    // Used in insantiation of dbService through "using".
    public void Dispose()
    {
        if (connection == null) return;

        if (connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
        }

        connection.Dispose();
    }


    private async Task InitializeDatabaseAsync()
    {
        try
        {
            Console.WriteLine("Creating users table...");
            await ExecuteNonQueryAsync(CreateUsersTableSql);

            Console.WriteLine("\nCreating transactions table...");
            await ExecuteNonQueryAsync(CreateTransactionsTableSql);

            Console.WriteLine("Database initialization completed sucessfully.");
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Error during database initialization: {ex.Message}");
            throw;
        }
    }


    public async Task ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null)
    {
        try
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            AddParameters(cmd, parameters);
            await cmd.ExecuteNonQueryAsync();
            ConsoleUI.DisplaySuccess($"Sucessfully executed SQL: {sql}");
        }
        catch (Exception ex)
        {
            ConsoleUI.DisplayError($"Error executing SQL: {sql}. Exception: {ex.Message}");
            throw;
        }
    }


    private void AddParameters(NpgsqlCommand cmd, Dictionary<string, object> parameters)
    {
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
    }


    public User AddUser(string username, string passwordHashed)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHashed))
        {
            throw new ArgumentException("Username and password cannot be null or empty.");
        }

        string sql = @"INSERT INTO users (username, password_hashed) 
                       VALUES (@Username, @PasswordHash) 
                       RETURNING user_id";
        try
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHashed);

            int userId = Convert.ToInt32(cmd.ExecuteScalar());
            return new User(userId, username, passwordHashed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
        }

        return null;
    }

    public User GetUserByUsername(string username)
    {
        string sql = @"SELECT user_id, username, password_hashed
                       FROM users
                       WHERE username = @Username";
        try
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int userId = reader.GetInt32(0);
                string retrievedUsername = reader.GetString(1);
                string passwordHashed = reader.GetString(2);

                return new User(userId, retrievedUsername, passwordHashed);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user: {ex.Message}");
        }

        return null;
    }
}