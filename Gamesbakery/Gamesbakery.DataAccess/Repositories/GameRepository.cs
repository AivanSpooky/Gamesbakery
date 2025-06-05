using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
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
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Games (GameID, CategoryID, Title, Price, ReleaseDate, Description, IsForSale, OriginalPublisher) " +
                    "VALUES (@GameID, @CategoryID, @Title, @Price, @ReleaseDate, @Description, @IsForSale, @OriginalPublisher)",
                    new SqlParameter("@GameID", game.Id),
                    new SqlParameter("@CategoryID", game.CategoryId),
                    new SqlParameter("@Title", game.Title),
                    new SqlParameter("@Price", game.Price),
                    new SqlParameter("@ReleaseDate", game.ReleaseDate),
                    new SqlParameter("@Description", game.Description),
                    new SqlParameter("@IsForSale", game.IsForSale),
                    new SqlParameter("@OriginalPublisher", game.OriginalPublisher));
                return game;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add game to the database.", ex);
            }
        }

        public async Task<Game> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                var games = await _context.Games
                    .FromSqlRaw("SELECT GameID, CategoryID, Title, Price, ReleaseDate, Description, IsForSale, OriginalPublisher " +
                                "FROM Games WHERE GameID = @GameID",
                        new SqlParameter("@GameID", id))
                    .ToListAsync();
                var game = games.FirstOrDefault();
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
                return await _context.Games
                    .FromSqlRaw("SELECT GameID, CategoryID, Title, Price, ReleaseDate, Description, IsForSale, OriginalPublisher FROM Games")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve games from the database.", ex);
            }
        }

        public async Task<Game> UpdateAsync(Game game, UserRole role)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            try
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Games SET CategoryID = @CategoryID, Title = @Title, Price = @Price, ReleaseDate = @ReleaseDate, " +
                    "Description = @Description, IsForSale = @IsForSale, OriginalPublisher = @OriginalPublisher " +
                    "WHERE GameID = @GameID",
                    new SqlParameter("@GameID", game.Id),
                    new SqlParameter("@CategoryID", game.CategoryId),
                    new SqlParameter("@Title", game.Title),
                    new SqlParameter("@Price", game.Price),
                    new SqlParameter("@ReleaseDate", game.ReleaseDate),
                    new SqlParameter("@Description", game.Description),
                    new SqlParameter("@IsForSale", game.IsForSale),
                    new SqlParameter("@OriginalPublisher", game.OriginalPublisher));

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"Game with ID {game.Id} not found.");

                return await GetByIdAsync(game.Id, role);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update game in the database.", ex);
            }
        }
        public decimal GetGameAverageRating(Guid gameId)
        {
            return _context.GetGameAverageRating(gameId);
        }
    }
}