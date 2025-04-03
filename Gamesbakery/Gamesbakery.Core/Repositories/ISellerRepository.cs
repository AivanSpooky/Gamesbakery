using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ISellerRepository
    {
        Task<Seller> AddAsync(Seller seller, UserRole role);
        Task<Seller> AddAsync(Guid sellerId, string sellerName, string password, DateTime registrationDate, double avgRating, UserRole role);
        Task<Seller> GetByIdAsync(Guid id, UserRole role);
        Task<List<Seller>> GetAllAsync(UserRole role);
        Task<Seller> UpdateAsync(Seller seller, UserRole role);
    }
}