using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ISellerRepository
    {
        Task<Seller> AddAsync(Seller seller);
        Task<Seller> GetByIdAsync(Guid id);
        Task<List<Seller>> GetAllAsync();
        Task<Seller> UpdateAsync(Seller seller);
    }
}