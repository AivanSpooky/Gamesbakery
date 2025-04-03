using Gamesbakery.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.Infrastructure
{
    public class DatabaseConnectionChecker : IDatabaseConnectionChecker
    {
        private readonly GamesbakeryDbContext _context;

        public DatabaseConnectionChecker(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> CanConnectAsync()
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
