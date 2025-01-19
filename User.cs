using Npgsql;
using System.Security.Cryptography;

namespace PersonalFinanceApp
{
    public class User
    {
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool Login(string connectionString, string username, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string hashedPassword = HashPassword(password);

                using (var cmd = new NpgsqlCommand(
                    "SELECT user_id FROM users WHERE username = @username AND password = @password", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }
                }
            }
            Console.WriteLine("Invalid username or password");
            return false;
        }

        public static void CreateAccount(string connectionString, string username, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                try
                {
                    string hashedPassword = HashPassword(password);

                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO users (username, password) VALUES (@username, @password)", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Account created successfully");
                    }
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == "23505") // Unique violation
                    {
                        Console.WriteLine("Username already exists");
                    }
                    else
                    {
                        Console.WriteLine($"Error creating account: {ex.Message}");
                    }
                }
            }
        }

        public static int GetUserId(string connectionString, string username)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT user_id FROM users WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }
    }
}