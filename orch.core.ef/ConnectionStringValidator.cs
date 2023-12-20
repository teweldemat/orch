using Npgsql;

namespace orch.core.ef
{
    public static class ConnectionStringValidator
    {
        static public bool IsValidPostgresConStr(string connectionString)
        {
            try
            {
                _ = new NpgsqlConnectionStringBuilder(connectionString);
                return true; // If parsing succeeds, it's a valid PostgreSQL connection string format
            }
            catch (ArgumentException)
            {
                // Parsing failed, the connection string format is invalid
                return false;
            }
        }

        static public bool CanConnectToPostgresDb(string connectionString)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                return true; // If we reached here, connection succeeded
            }
            catch
            {
                // Connection failed
                return false;
            }
        }
    }
}
