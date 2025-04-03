using Gamesbakery.Core;
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

        public async Task<Game> AddAsync(Game game, UserRole role)
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

        public async Task<Game> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                var game = await _context.Games.FindAsync(id);
                if (game == null)
                    throw new KeyNotFoundException($"Game with ID {id} not found.");

                return game;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve game with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<List<Game>> GetAllAsync(UserRole role)
        {
            try
            {
                return await _context.Games.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Detailed error in GameRepository.GetAllAsync: {ex}");
                throw new InvalidOperationException("Failed to retrieve games from the database.", ex);
            }
        }

        public async Task<Game> UpdateAsync(Game game, UserRole role)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            try
            {
                var existingGame = await _context.Games.FindAsync(game.Id);
                if (existingGame == null)
                    throw new KeyNotFoundException($"Game with ID {game.Id} not found.");

                existingGame.SetCategoryId(game.CategoryId);
                existingGame.SetTitle(game.Title);
                existingGame.SetPrice(game.Price);
                existingGame.SetReleaseDate(game.ReleaseDate);
                existingGame.SetDescription(game.Description);
                existingGame.SetForSale(game.IsForSale);
                existingGame.SetOriginalPublisher(game.OriginalPublisher);

                await _context.SaveChangesAsync();
                return existingGame;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update game in the database.", ex);
            }
        }
    }
}