using Npgsql;

namespace PersonalFinanceApp
{ 
public class TransactionManager
{
    private readonly string _connectionString;
    private readonly int _userId;

    public TransactionManager(string connectionString, int userId)
    {
        _connectionString = connectionString;
        _userId = userId;
    }

    public void AddTransaction(DateTime date, decimal amount, string type, string description)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO transactions (user_id, date, amount, type, description)
                    VALUES (@userId, @date, @amount, @type, @description)", conn))
            {
                cmd.Parameters.AddWithValue("@userId", _userId);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.ExecuteNonQuery();
            }
        }
        Console.WriteLine("Transaction added successfully");
    }

    public bool DeleteTransaction(int index)
    {
        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                // First get the transaction_id for the given index
                using (var cmd = new NpgsqlCommand(@"
                        SELECT transaction_id FROM transactions 
                        WHERE user_id = @userId 
                        ORDER BY date, transaction_id 
                        OFFSET @index LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@index", index);
                    var transactionId = cmd.ExecuteScalar();

                    if (transactionId != null)
                    {
                        // Delete the transaction
                        using (var deleteCmd = new NpgsqlCommand(
                            "DELETE FROM transactions WHERE transaction_id = @transactionId AND user_id = @userId", conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@transactionId", transactionId);
                            deleteCmd.Parameters.AddWithValue("@userId", _userId);
                            deleteCmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting transaction: {ex.Message}");
        }
        return false;
    }

    public decimal GetCurrentBalance()
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand(@"
                    SELECT COALESCE(SUM(CASE WHEN type = 'Income' THEN amount ELSE -amount END), 0)
                    FROM transactions 
                    WHERE user_id = @userId", conn))
            {
                cmd.Parameters.AddWithValue("@userId", _userId);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }

    public List<Transaction> SearchTransactions(string type = "", string filter = "",
        int? year = null, int? month = null, int? week = null, DateTime? date = null)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"SELECT date, amount, type, description 
                            FROM transactions 
                            WHERE user_id = @userId";

            if (!string.IsNullOrEmpty(type))
                query += " AND type = @type";

            switch (filter)
            {
                case "Year" when year.HasValue:
                    query += " AND EXTRACT(YEAR FROM date) = @year";
                    break;
                case "Month" when year.HasValue && month.HasValue:
                    query += " AND EXTRACT(YEAR FROM date) = @year AND EXTRACT(MONTH FROM date) = @month";
                    break;
                case "Week" when year.HasValue && week.HasValue:
                    query += " AND EXTRACT(YEAR FROM date) = @year AND EXTRACT(WEEK FROM date) = @week";
                    break;
                case "Day" when date.HasValue:
                    query += " AND date = @date";
                    break;
            }

            query += " ORDER BY date";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", _userId);
                if (!string.IsNullOrEmpty(type))
                    cmd.Parameters.AddWithValue("@type", type);
                if (year.HasValue)
                    cmd.Parameters.AddWithValue("@year", year.Value);
                if (month.HasValue)
                    cmd.Parameters.AddWithValue("@month", month.Value);
                if (week.HasValue)
                    cmd.Parameters.AddWithValue("@week", week.Value);
                if (date.HasValue)
                    cmd.Parameters.AddWithValue("@date", date.Value);

                var transactions = new List<Transaction>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction(
                            reader.GetDateTime(0),
                            reader.GetDecimal(1),
                            reader.GetString(2),
                            reader.GetString(3)
                        ));
                    }
                }
                return transactions;
            }
        }
    }
}
}