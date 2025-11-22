using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly GamesbakeryDbContext _context;

        public GameRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<GameDetailsDTO> AddAsync(GameDetailsDTO dto, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add games.");
            var entity = new Game(dto.Id, dto.CategoryId, dto.Title, dto.Price, dto.ReleaseDate, dto.Description, dto.IsForSale, dto.OriginalPublisher);
            _context.Games.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDetailsDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete games.");
            var entity = await _context.Games.FindAsync(id);
            if (entity != null)
            {
                _context.Games.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GameListDTO>> GetAllAsync(UserRole role)
        {
            var games = await _context.Games.Include(g => g.Category).ToListAsync();
            return games.Select(MapToListDTO);
        }

        public async Task<GameDetailsDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            var entity = await _context.Games.Include(g => g.Category).FirstOrDefaultAsync(g => g.Id == id);
            return entity != null ? MapToDetailsDTO(entity) : null;
        }

        public async Task<GameDetailsDTO> UpdateAsync(GameDetailsDTO dto, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update games.");
            var entity = await _context.Games.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Game {dto.Id} not found");
            entity.SetCategoryId(dto.CategoryId);
            entity.SetTitle(dto.Title);
            entity.SetPrice(dto.Price);
            entity.SetReleaseDate(dto.ReleaseDate);
            entity.SetDescription(dto.Description);
            entity.SetOriginalPublisher(dto.OriginalPublisher);
            entity.SetForSale(dto.IsForSale);
            _context.Games.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDetailsDTO(entity);
        }

        public async Task<IEnumerable<GameListDTO>> GetFilteredAsync(string genre, decimal? minPrice, decimal? maxPrice, UserRole role)
        {
            var query = _context.Games.Include(g => g.Category).AsQueryable();
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(g => g.Category.GenreName == genre);
            if (minPrice.HasValue)
                query = query.Where(g => g.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(g => g.Price <= maxPrice.Value);
            var games = await query.ToListAsync();
            return games.Select(MapToListDTO);
        }

        public async Task<int> GetCountAsync(string genre, decimal? minPrice, decimal? maxPrice, UserRole role)
        {
            var query = _context.Games.AsQueryable();
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(g => g.Category.GenreName == genre);
            if (minPrice.HasValue)
                query = query.Where(g => g.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(g => g.Price <= maxPrice.Value);
            return await query.CountAsync();
        }

        public decimal GetAverageRating(Guid gameId)
        {
            return _context.Reviews.Where(r => r.GameId == gameId).Average(r => (decimal?)r.Rating) ?? 0;
        }

        private GameDetailsDTO MapToDetailsDTO(Game entity)
        {
            return new GameDetailsDTO
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                Title = entity.Title,
                Price = entity.Price,
                ReleaseDate = entity.ReleaseDate,
                Description = entity.Description,
                IsForSale = entity.IsForSale,
                OriginalPublisher = entity.OriginalPublisher,
                AverageRating = GetAverageRating(entity.Id)
            };
        }

        private GameListDTO MapToListDTO(Game entity)
        {
            return new GameListDTO
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                Title = entity.Title,
                Price = entity.Price,
                IsForSale = entity.IsForSale
            };
        }
    }
}