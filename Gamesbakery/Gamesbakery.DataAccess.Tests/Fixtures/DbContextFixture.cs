using Microsoft.EntityFrameworkCore;
namespace Gamesbakery.DataAccess.Tests.Fixtures
{
    public class DbContextFixture : IDbContextFixture, IDisposable
    {
        public GamesbakeryDbContext Context { get; private set; }

        public DbContextFixture()
        {
            var options = new DbContextOptionsBuilder<GamesbakeryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new GamesbakeryDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}