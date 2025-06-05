using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseGiftRepository : IGiftRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseGiftRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<Gift> AddAsync(Gift gift)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Gifts (GiftID, SenderID, RecipientID, OrderItemID, GiftDate) VALUES (@GiftID, @SenderID, @RecipientID, @OrderItemID, @GiftDate)";
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "GiftID",
                Value = gift.Id.ToString("N")
            });
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "SenderID",
                Value = gift.SenderId.ToString("N")
            });
            // ... other parameters
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return gift;
        }

        public async Task<Gift> GetByIdAsync(Guid giftId, UserRole role, GiftSource source, Guid? currentUserId)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            string query = role == UserRole.Admin
                ? "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate FROM Gifts WHERE GiftID = @GiftID"
                : source == GiftSource.Sent
                ? "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate FROM Gifts WHERE GiftID = @GiftID AND SenderID = @UserID"
                : "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate FROM Gifts WHERE GiftID = @GiftID AND RecipientID = @UserID";
            cmd.CommandText = query;
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GiftID", Value = giftId });
            if (role != UserRole.Admin && currentUserId.HasValue)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = currentUserId.Value });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var gift = new Gift(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.GetDateTime(4));
                await _connection.CloseAsync();
                return gift;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"Gift with ID {giftId} not found or inaccessible.");
        }

        public async Task<IEnumerable<SentGift>> GetBySenderIdAsync(Guid senderId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate FROM Gifts WHERE SenderID = @SenderID ORDER BY GiftDate DESC";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SenderID", Value = senderId });
            var reader = await cmd.ExecuteReaderAsync();
            var gifts = new List<SentGift>();
            while (await reader.ReadAsync())
            {
                gifts.Add(new SentGift
                {
                    Id = reader.GetGuid(0),
                    SenderId = reader.GetGuid(1),
                    RecipientId = reader.GetGuid(2),
                    OrderItemId = reader.GetGuid(3),
                    GiftDate = reader.GetDateTime(4)
                });
            }
            await _connection.CloseAsync();
            return gifts;
        }

        public async Task<IEnumerable<ReceivedGift>> GetByRecipientIdAsync(Guid recipientId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate FROM Gifts WHERE RecipientID = @RecipientID ORDER BY GiftDate DESC";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "RecipientID", Value = recipientId });
            var reader = await cmd.ExecuteReaderAsync();
            var gifts = new List<ReceivedGift>();
            while (await reader.ReadAsync())
            {
                gifts.Add(new ReceivedGift
                {
                    Id = reader.GetGuid(0),
                    SenderId = reader.GetGuid(1),
                    RecipientId = reader.GetGuid(2),
                    OrderItemId = reader.GetGuid(3),
                    GiftDate = reader.GetDateTime(4)
                });
            }
            await _connection.CloseAsync();
            return gifts;
        }

        public async Task DeleteAsync(Guid id)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Gifts DELETE WHERE GiftID = @GiftID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GiftID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}