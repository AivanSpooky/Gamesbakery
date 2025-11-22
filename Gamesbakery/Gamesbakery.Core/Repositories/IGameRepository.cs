using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IGameRepository
    {
        Task<GameDetailsDTO> AddAsync(GameDetailsDTO dto, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<IEnumerable<GameListDTO>> GetAllAsync(UserRole role);
        Task<GameDetailsDTO?> GetByIdAsync(Guid id, UserRole role);
        Task<GameDetailsDTO> UpdateAsync(GameDetailsDTO dto, UserRole role);
        Task<IEnumerable<GameListDTO>> GetFilteredAsync(string genre, decimal? minPrice, decimal? maxPrice, UserRole role);
        Task<int> GetCountAsync(string genre, decimal? minPrice, decimal? maxPrice, UserRole role);
        decimal GetAverageRating(Guid gameId);
    }
}