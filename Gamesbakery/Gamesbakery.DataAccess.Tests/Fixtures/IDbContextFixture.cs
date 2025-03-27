namespace Gamesbakery.DataAccess.Tests.Fixtures
{
    public interface IDbContextFixture
    {
        GamesbakeryDbContext Context { get; }
    }
}
