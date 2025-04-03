namespace Gamesbakery.Infrastructure
{
    public interface IDatabaseConnectionChecker
    {
        Task<bool> CanConnectAsync();
    }
}
