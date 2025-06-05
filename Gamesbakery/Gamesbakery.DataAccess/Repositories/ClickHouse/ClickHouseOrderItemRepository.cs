using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseOrderItemRepository : IOrderItemRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseOrderItemRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO OrderItems (OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted) VALUES (@OrderItemID, @OrderID, @GameID, @SellerID, @KeyText, @IsGifted)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderItemID", Value = orderItem.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = orderItem.OrderId == Guid.Empty ? DBNull.Value : orderItem.OrderId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = orderItem.GameId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = orderItem.SellerId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "KeyText", Value = orderItem.Key ?? "" });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsGifted", Value = orderItem.IsGifted });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return orderItem;
        }

        public async Task<OrderItem> CreateKeyAsync(Guid gameId, Guid sellerId, string key, UserRole role)
        {
            if (role != UserRole.Seller && role != UserRole.Admin) throw new UnauthorizedAccessException("Only sellers or admins can create keys.");
            var orderItem = new OrderItem(Guid.NewGuid(), Guid.Empty, gameId, sellerId, key);
            return await AddAsync(orderItem, role);
        }

        public async Task<OrderItem> GetByIdAsync(Guid id, UserRole role, Guid? currentUserId = null)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            string query = role == UserRole.Admin
                ? "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderItemID = @OrderItemID"
                : role == UserRole.Seller
                ? "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderItemID = @OrderItemID AND SellerID = @SellerID"
                : "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderItemID = @OrderItemID AND (OrderID IS NULL OR OrderID IN (SELECT OrderID FROM Orders WHERE UserID = @UserID))";
            cmd.CommandText = query;
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderItemID", Value = id });
            if (role == UserRole.Seller && currentUserId.HasValue)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = currentUserId.Value });
            else if (role == UserRole.User && currentUserId.HasValue)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = currentUserId.Value });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var orderItem = new OrderItem(
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? Guid.Empty : reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetBoolean(5));
                await _connection.CloseAsync();
                return orderItem;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"OrderItem with ID {id} not found or inaccessible.");
        }

        public async Task<IEnumerable<OrderItem>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderID IN (SELECT OrderID FROM Orders WHERE UserID = @UserID)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = userId });
            var reader = await cmd.ExecuteReaderAsync();
            var items = new List<OrderItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItem(
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? Guid.Empty : reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetBoolean(5)));
            }
            await _connection.CloseAsync();
            return items;
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderID = @OrderID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = orderId });
            var reader = await cmd.ExecuteReaderAsync();
            var items = new List<OrderItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItem(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetBoolean(5)
                ));
            }
            await _connection.CloseAsync();
            return items;
        }

        public async Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE SellerID = @SellerID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = sellerId });
            var reader = await cmd.ExecuteReaderAsync();
            var items = new List<OrderItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItem(
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? Guid.Empty : reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetBoolean(5)));
            }
            await _connection.CloseAsync();
            return items;
        }

        public async Task UpdateAsync(OrderItem orderItem, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller) throw new UnauthorizedAccessException("Only admins or sellers can update order items.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            string query = role == UserRole.Admin
                ? "ALTER TABLE OrderItems UPDATE OrderID = @OrderID, KeyText = @KeyText, IsGifted = @IsGifted WHERE OrderItemID = @OrderItemID"
                : "ALTER TABLE OrderItems UPDATE KeyText = @KeyText WHERE OrderItemID = @OrderItemID AND SellerID = @SellerID";
            cmd.CommandText = query;
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderItemID", Value = orderItem.Id });
            if (role == UserRole.Admin)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = orderItem.OrderId == Guid.Empty ? DBNull.Value : orderItem.OrderId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "KeyText", Value = orderItem.Key ?? "" });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsGifted", Value = orderItem.IsGifted });
            if (role == UserRole.Seller)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = orderItem.SellerId });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<List<OrderItem>> GetAvailableByGameIdAsync(Guid gameId)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE GameID = @GameID AND OrderID IS NULL";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "GameID", Value = gameId });
            var reader = await cmd.ExecuteReaderAsync();
            var items = new List<OrderItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItem(
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? Guid.Empty : reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetBoolean(5)
                ));
            }
            await _connection.CloseAsync();
            return items;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller) throw new UnauthorizedAccessException("Only admins or sellers can delete order items.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE OrderItems DELETE WHERE OrderItemID = @OrderItemID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderItemID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}