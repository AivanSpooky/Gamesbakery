namespace Gamesbakery.BusinessLogic.Schedulers
{
    public interface IOrderStatusScheduler
    {
        Task UpdateOrderStatusesAsync();
    }
}