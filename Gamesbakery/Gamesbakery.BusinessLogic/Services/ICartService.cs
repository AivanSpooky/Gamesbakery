using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core
{
    public interface ICartService
    {
        Task AddToCartAsync(Guid orderItemId, Guid? userId);
        Task RemoveFromCartAsync(Guid orderItemId, Guid? userId);
        Task<List<CartItemDTO>> GetCartItemsAsync(Guid? userId);
        Task<decimal> GetCartTotalAsync(Guid? userId);
        Task ClearCartAsync(Guid? userId);
        void ClearCart(Guid? userId);
    }
}
