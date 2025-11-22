using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IGiftRepository
    {
        Task<GiftDTO> AddAsync(GiftDTO dto, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<GiftDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null);
        Task<GiftDTO> UpdateAsync(GiftDTO dto, UserRole role);
        Task<List<GiftDTO>> GetBySenderIdAsync(Guid senderId, UserRole role);
        Task<List<GiftDTO>> GetByRecipientIdAsync(Guid recipientId, UserRole role);
        Task<List<GiftDTO>> GetAllForUserAsync(Guid userId, UserRole role, GiftSource source);
    }
}