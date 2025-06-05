using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseOrderRepository : IOrderRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseOrderRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders WHERE UserID = @UserID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = userId });
            var reader = await cmd.ExecuteReaderAsync();
            var orders = new List<Order>();
            while (await reader.ReadAsync())
            {
                orders.Add(new Order(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetDateTime(2),
                    reader.GetDecimal(3),
                    reader.GetBoolean(4),
                    reader.GetBoolean(5)));
            }
            await _connection.CloseAsync();
            return orders;
        }

        public async Task<Order> AddAsync(Order order, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Orders (OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue) VALUES (@OrderID, @UserID, @OrderDate, @TotalPrice, @IsCompleted, @IsOverdue)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = order.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = order.UserId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderDate", Value = order.OrderDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "TotalPrice", Value = order.Price });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsCompleted", Value = order.IsCompleted });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsOverdue", Value = order.IsOverdue });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return order;
        }

        public async Task<Order> GetByIdAsync(Guid orderId, UserRole role, Guid? currentUserId = null)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            string query = role == UserRole.Admin
                ? "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders WHERE OrderID = @OrderID"
                : "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders WHERE OrderID = @OrderID AND UserID = @UserID";
            cmd.CommandText = query;
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = orderId });
            if (role != UserRole.Admin && currentUserId.HasValue)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = currentUserId.Value });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var order = new Order(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetDateTime(2),
                    reader.GetDecimal(3),
                    reader.GetBoolean(4),
                    reader.GetBoolean(5));
                await _connection.CloseAsync();
                return order;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"Order with ID {orderId} not found or inaccessible.");
        }

        public async Task<Order> UpdateAsync(Order order, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            string query = role == UserRole.Admin
                ? "ALTER TABLE Orders UPDATE UserID = @UserID, OrderDate = @OrderDate, TotalPrice = @TotalPrice, IsCompleted = @IsCompleted, IsOverdue = @IsOverdue WHERE OrderID = @OrderID"
                : "ALTER TABLE Orders UPDATE OrderDate = @OrderDate, TotalPrice = @TotalPrice, IsCompleted = @IsCompleted, IsOverdue = @IsOverdue WHERE OrderID = @OrderID";
            cmd.CommandText = query;
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = order.Id });
            if (role == UserRole.Admin)
                cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = order.UserId });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderDate", Value = order.OrderDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "TotalPrice", Value = order.Price });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsCompleted", Value = order.IsCompleted });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsOverdue", Value = order.IsOverdue });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return order;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Orders DELETE WHERE OrderID = @OrderID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "OrderID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}