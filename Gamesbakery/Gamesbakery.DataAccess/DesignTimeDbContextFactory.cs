using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gamesbakery.DataAccess
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GamesbakeryDbContext>
    {
        public GamesbakeryDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=localhost,1434;Database=Gamesbakery;User Id=sa;Password=YourStrong@Pass;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<GamesbakeryDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new GamesbakeryDbContext(optionsBuilder.Options);
        }
    }
}
