using Npgsql;
using PersonalFinanceApp;



public class DatabaseManager
{
    private NpgsqlConnection connection;
    private readonly string _connectionString =
        "Host=localhost;Port=5432;Database=PersonalFinanceApp;Username=Postgres;Password=password;";

    public DatabaseManager()
    {
        this.connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // Create the tables (IF NOT EXIST) here
        string createUsersTableSql =
            @"CREATE TABLE IF NOT EXISTS users (
                user_id SERIAL PRIMARY KEY,

                username VARCHAR(50) UNIQUE NOT NULL,
                password_hashed TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
            )";

        using var createTableCmd = new NpgsqlCommand(createUsersTableSql, connection);
        createTableCmd.ExecuteNonQuery();
    }


    // Testing a wrapper-method that helps me by removing the 
    // need to manually close the connection when DatabaseManager is no longer neeeded.
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



    // Not including Users CreatedAt and UserId as params here since the DB will create these.
    public User AddUsers(string username, string hashedPassword)
    {
        string sql =
            @"
            INSERT INTO users (username, password_hashed) 
            VALUES (@Username, @PasswordHash) 
            RETURNING user_id, created_At
            ";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);


        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {

            int userId = reader.GetInt32(0); // column 1
            DateTime createdAt = reader.GetDateTime(3); // column 4

            return new User(userId, username, hashedPassword, createdAt);
        }

        return null;
    }
}