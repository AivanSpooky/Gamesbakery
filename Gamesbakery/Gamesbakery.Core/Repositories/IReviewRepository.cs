using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IReviewRepository
    {
        Task<ReviewDTO> AddAsync(ReviewDTO dto, UserRole role);

        Task DeleteAsync(Guid id, UserRole role);

        Task<IEnumerable<ReviewDTO>> GetAllAsync(UserRole role);

        Task<ReviewDTO?> GetByIdAsync(Guid id, UserRole role);

        Task<ReviewDTO> UpdateAsync(ReviewDTO dto, UserRole role);

        Task<IEnumerable<ReviewDTO>> GetByGameIdAsync(Guid gameId, UserRole role, Guid? userId = null, int? minRating = null, int? maxRating = null);

        Task<IEnumerable<ReviewDTO>> GetByUserIdAsync(Guid userId, UserRole role);
    }
}
