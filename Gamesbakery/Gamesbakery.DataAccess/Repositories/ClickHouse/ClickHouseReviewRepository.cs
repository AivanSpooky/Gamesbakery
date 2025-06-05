using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseReviewRepository : IReviewRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseReviewRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<Review> AddAsync(Review review, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Reviews (ReviewID, UserID, GameID, Comment, StarRating, CreationDate) VALUES (@ReviewID, @UserID, @GameID, @Comment, @StarRating, @CreationDate)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "ReviewID", Value = review.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = review.UserId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = review.GameId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Comment", Value = review.Text });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "StarRating", Value = review.Rating });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "CreationDate", Value = review.CreationDate });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return review;
        }

        public async Task<List<Review>> GetByGameIdAsync(Guid gameId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT ReviewID, UserID, GameID, Comment, StarRating, CreationDate FROM Reviews WHERE GameID = @GameID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = gameId });
            var reader = await cmd.ExecuteReaderAsync();
            var reviews = new List<Review>();
            while (await reader.ReadAsync())
            {
                reviews.Add(new Review(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDateTime(5)));
            }
            await _connection.CloseAsync();
            return reviews;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Reviews DELETE WHERE ReviewID = @ReviewID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "ReviewID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}