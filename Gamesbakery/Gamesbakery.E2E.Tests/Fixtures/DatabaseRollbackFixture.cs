using Microsoft.Data.SqlClient;

namespace Gamesbakery.E2E.Tests.Fixtures
{
    public class DatabaseRollbackFixture : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public DatabaseRollbackFixture(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _connection.Close();
        }
    }
}
