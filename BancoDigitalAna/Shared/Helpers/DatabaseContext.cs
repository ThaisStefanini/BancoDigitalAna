using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;

namespace BancoDigitalAna.Shared.Helpers
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            // Ajuste o caminho conforme necessário
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "bancodigitalana.db");
            _connectionString = $"Data Source={dbPath};";
        }

        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
