using System.Reflection.Metadata;
using Npgsql;
using PersonalFinanceApp;



public class DatabaseManager : IDisposable
{
    private readonly NpgsqlConnection connection;

    public NpgsqlConnection Connection
    {
        get { return connection; }
    }

    private readonly string _connectionString =
        "Host=localhost; Port=5432; Database=PersonalFinanceApp; Username=postgres; Password=assword;";

    private const string CreateUsersTableSql = @"
        CREATE TABLE IF NOT EXISTS users (
            user_id SERIAL PRIMARY KEY,
            username VARCHAR(50) UNIQUE NOT NULL,
            password_hashed TEXT NOT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
        )";

    private const string CreateTransactionsTableSql = @"
        CREATE TABLE IF NOT EXISTS transactions (
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


    public void Dispose()
    {
        if (connection == null) return; // 1. Guard-clause for immediate handling.

        if (connection.State == System.Data.ConnectionState.Open) // 2. Checks if the connection is open.
        {
            connection.Close(); // 3. Close the connection.
        }

        connection.Dispose(); // Dispose the connection to release resources.
    }

    private void InitializeDatabase()
    {
        try
        {
            Console.WriteLine("Creating users table...");
            ExecuteNonQuery(CreateUsersTableSql);

            Console.WriteLine("Creating transaction type enum...");
            ExecuteNonQuery(CreateTypeEnumSql);

            Console.WriteLine("Creating transactions table...");
            ExecuteNonQuery(CreateTransactionsTableSql);

            Console.WriteLine("Database initialization completed sucessfully.");

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
            Console.WriteLine($"Sucessfully executed SQL: {sql}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL: {sql}. Exception: {ex.Message}");
        }
    }



    public User AddUser(string username, string passwordHashed)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHashed))
        {
            throw new ArgumentException("Username and password cannot be null or empty.");
        }

        string sql = @"
            INSERT INTO users (username, password_hashed) 
            VALUES (@Username, @PasswordHash) 
            RETURNING user_id
            ";

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
        string sql = @"
            SELECT user_id, username, password_hashed
            FROM users
            WHERE username = @Username
            ";

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