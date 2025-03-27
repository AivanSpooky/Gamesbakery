using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Tests.Fixtures
{
    public class SqlServerDbContextFixture : IDbContextFixture, IDisposable
    {
        public GamesbakeryDbContext Context { get; private set; }

        public SqlServerDbContextFixture()
        {
            var options = new DbContextOptionsBuilder<GamesbakeryDbContext>() // LAPTOP-7106M2BU
                .UseSqlServer("Server=LAPTOP-7106M2BU;Database=GamesbakeryTest;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            Context = new GamesbakeryDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
