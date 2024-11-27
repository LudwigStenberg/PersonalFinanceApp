using System.Reflection.Metadata;
using Npgsql;
using PersonalFinanceApp;



public class DatabaseManager
{
    private NpgsqlConnection connection;
    private readonly string _connectionString =
        "Host=localhost;Port=5432;Database=PersonalFinanceApp;Username=postgres;Password=assword;";

    private const string CreateUsersTableSql = @"
        CREATE TABLE IF NOT EXISTS users (
            user_id SERIAL PRIMARY KEY,
            username VARCHAR(50) UNIQUE NOT NULL,
            password_hashed TEQXT NOT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
        )";

    private const string CreateTransactionsTableSql = @"
        CREATE TABLE transactions (
            transaction_id SERIAL PRIMARY KEY,
            user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
            type transaction_type_enum NOT NULL,
            amount DECIMAL(10, 2) NOT NULL,
            date TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0),
            category VARCHAR(25) NOT NULL,
            description VARCHAR(50)
        )";

    private const string CreateTypeEnumSql = @"
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'transaction_type_enum') THEN
                CREATE TYPE transaction_type_enum AS ENUM ('Income', 'Expense');
            END IF;
        END $$;
        ";


    public DatabaseManager()
    {
        try
        {
            this.connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            InitializeDatabase();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            throw;
        }
    }



    private void InitializeDatabase()
    {
        try
        {
            Console.WriteLine("Creating users table...");
            using var createUsersTableCmd = new NpgsqlCommand(CreateUsersTableSql, connection);
            createUsersTableCmd.ExecuteNonQuery();

            Console.WriteLine("Creating transaction type enum...");
            using var createTypeEnumSqlCmd = new NpgsqlCommand(CreateTypeEnumSql, connection);
            createTypeEnumSqlCmd.ExecuteNonQuery();

            Console.WriteLine("Creating transactions table...");
            using var createTransactionsTableSqlCmd = new NpgsqlCommand(CreateTransactionsTableSql, connection);
            createTransactionsTableSqlCmd.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database initialization: {ex.Message}");
            throw;
        }
    }







    public void ExecuteNonQuery(string sql)
    {
        try
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL: {ex.Message}");
        }
    }




    public User AddUser(string username, string passwordHashed)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHashed))
        {
            throw new ArgumentException("Username and password cannot be null or empty.");
        }

        string sql =
            @"
            INSERT INTO users (username, password_hashed) 
            VALUES (@Username, @PasswordHash) 
            RETURNING user_id
            ";

        try
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHashed);

            // Add Try-catch?
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {

                int userId = reader.GetInt32(0);

                return new User(userId, username, passwordHashed);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
        }

        return null;
    }
}