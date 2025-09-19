using Gamesbakery.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace Gamesbakery.DataAccess.Tests.Fixtures
{
    public class SqlServerDbContextFixture : IDbContextFixture, IDisposable
    {
        public GamesbakeryDbContext Context { get; private set; }

        public SqlServerDbContextFixture()
        {
            // Используем реальную базу Gamesbakery
            var connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ??
                "Server=db,1433;Database=Gamesbakery;User Id=sa;Password=YourStrong@Pass;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60;";

            var options = new DbContextOptionsBuilder<GamesbakeryDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            Context = new GamesbakeryDbContext(options);

            // Проверяем подключение
            try
            {
                Context.Database.OpenConnection();
                Context.Database.CloseConnection();
                Console.WriteLine("Successfully connected to Gamesbakery database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to Gamesbakery database: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}