using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(Guid orderItemId);
        Task RemoveFromCartAsync(Guid orderItemId);
        Task<List<OrderItem>> GetCartItemsAsync();
        Task<decimal> GetCartTotalAsync();
        void ClearCart();
    }
}