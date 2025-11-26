using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ISellerRepository
    {
        Task<SellerDTO> AddAsync(SellerDTO dto, UserRole role);

        Task DeleteAsync(Guid id, UserRole role);

        Task<IEnumerable<SellerDTO>> GetAllAsync(UserRole role);

        Task<SellerDTO?> GetByIdAsync(Guid id, UserRole role);

        Task<SellerDTO> UpdateAsync(SellerDTO dto, UserRole role);

        Task<SellerDTO?> GetProfileAsync(Guid id, UserRole role);

        Task<int> GetCountAsync(UserRole role);
    }
}
