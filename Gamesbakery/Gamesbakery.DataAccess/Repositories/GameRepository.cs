// GameRepository.cs
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Game> AddAsync(Game game)
        {
            try
            {
                await _context.Games.AddAsync(game);
                await _context.SaveChangesAsync();
                return game;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add game to the database.", ex);
            }
        }

        public async Task<Game> GetByIdAsync(Guid id)
        {
            //if (id <= 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var game = await _context.Games.FindAsync(id);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {id} not found.");

            return game;
        }

        public async Task<List<Game>> GetAllAsync()
        {
            try
            {
                return await _context.Games.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve games from the database.", ex);
            }
        }

        public async Task<Game> UpdateAsync(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            try
            {
                _context.Games.Update(game);
                await _context.SaveChangesAsync();
                return await _context.Games.FindAsync(game.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update game in the database.", ex);
            }
        }
    }
}