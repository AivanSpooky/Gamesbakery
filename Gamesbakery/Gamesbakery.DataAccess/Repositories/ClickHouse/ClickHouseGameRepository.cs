using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseGameRepository : IGameRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseGameRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<Game> AddAsync(Game game, UserRole role)
        {
            if (role != UserRole.Seller && role != UserRole.Admin) throw new UnauthorizedAccessException("Only sellers or admins can add games.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Games (GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale) VALUES (@GameID, @CategoryID, @Title, @Price, @ReleaseDate, @Description, @OriginalPublisher, @IsForSale)";
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "GameID",
                Value = game.Id.ToString("N")
            });
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "CategoryID",
                Value = game.CategoryId.ToString("N")
            });
            // ... other parameters
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return game;
        }

        public async Task<Game> GetByIdAsync(Guid id, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale FROM Games WHERE GameID = @GameID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = id });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var game = new Game(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.GetDateTime(4),
                    reader.GetString(5),
                    reader.GetBoolean(7),
                    reader.GetString(6));
                await _connection.CloseAsync();
                return game;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"Game with ID {id} not found.");
        }

        public async Task<List<Game>> GetAllAsync(UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT GameID, CategoryID, Title, Price, ReleaseDate, Description, OriginalPublisher, IsForSale FROM Games";
            var reader = await cmd.ExecuteReaderAsync();
            var games = new List<Game>();
            while (await reader.ReadAsync())
            {
                games.Add(new Game(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.GetDateTime(4),
                    reader.GetString(5),
                    reader.GetBoolean(7),
                    reader.GetString(6)));
            }
            await _connection.CloseAsync();
            return games;
        }

        public async Task<Game> UpdateAsync(Game game, UserRole role)
        {
            if (role != UserRole.Seller && role != UserRole.Admin) throw new UnauthorizedAccessException("Only sellers or admins can update games.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Games UPDATE CategoryID = @CategoryID, Title = @Title, Price = @Price, ReleaseDate = @ReleaseDate, Description = @Description, OriginalPublisher = @OriginalPublisher, IsForSale = @IsForSale WHERE GameID = @GameID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = game.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "CategoryID", Value = game.CategoryId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Title", Value = game.Title });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Price", Value = game.Price });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "ReleaseDate", Value = game.ReleaseDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Description", Value = game.Description });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OriginalPublisher", Value = game.OriginalPublisher });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsForSale", Value = game.IsForSale });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return game;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Seller && role != UserRole.Admin) throw new UnauthorizedAccessException("Only sellers or admins can delete games.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Games DELETE WHERE GameID = @GameID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public decimal GetGameAverageRating(Guid gameId)
        {
            throw new NotImplementedException();
        }
    }
}