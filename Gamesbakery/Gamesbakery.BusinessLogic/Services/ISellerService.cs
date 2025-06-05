using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface ISellerService
    {
        Task<Seller> RegisterSellerAsync(string sellerName, string password);
        Task<Seller> RegisterSellerAsync(string sellerName, string password, bool proc);
        Task<Seller> GetSellerByIdAsync(Guid id);
        Task<OrderItem> CreateKeyAsync(Guid gameId, string key);
        Task<List<Seller>> GetAllSellersAsync();
        Task UpdateSellerRatingAsync(Guid sellerId, double newRating);
    }
}
