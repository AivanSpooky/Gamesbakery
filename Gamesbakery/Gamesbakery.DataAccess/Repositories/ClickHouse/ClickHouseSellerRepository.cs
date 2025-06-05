using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseSellerRepository : ISellerRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseSellerRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<Seller> AddAsync(Seller seller, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can add sellers.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Sellers (SellerID, Name, RegistrationDate, AverageRating, Password) VALUES (@SellerID, @Name, @RegistrationDate, @AverageRating, @Password)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = seller.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = seller.SellerName });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "RegistrationDate", Value = seller.RegistrationDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "AverageRating", Value = seller.AvgRating });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Password", Value = seller.Password });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return seller;
        }

        public async Task<Seller> AddAsync(Guid sellerId, string sellerName, string password, DateTime registrationDate, double avgRating, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can add sellers.");
            var seller = new Seller(sellerId, sellerName, registrationDate, avgRating, password);
            return await AddAsync(seller, role);
        }

        public async Task<Seller> GetByIdAsync(Guid id, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM Sellers WHERE SellerID = @SellerID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = id });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var seller = new Seller(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetDouble(3),
                    reader.GetString(4));
                await _connection.CloseAsync();
                return seller;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"Seller with ID {id} not found.");
        }

        public async Task<List<Seller>> GetAllAsync(UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM Sellers";
            var reader = await cmd.ExecuteReaderAsync();
            var sellers = new List<Seller>();
            while (await reader.ReadAsync())
            {
                sellers.Add(new Seller(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetDouble(3),
                    reader.GetString(4)));
            }
            await _connection.CloseAsync();
            return sellers;
        }

        public async Task<Seller> UpdateAsync(Seller seller, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can update sellers.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Sellers UPDATE Name = @Name, RegistrationDate = @RegistrationDate, AverageRating = @AverageRating, Password = @Password WHERE SellerID = @SellerID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = seller.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = seller.SellerName });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "RegistrationDate", Value = seller.RegistrationDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "AverageRating", Value = seller.AvgRating });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Password", Value = seller.Password });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return seller;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can delete sellers.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Sellers DELETE WHERE SellerID = @SellerID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "SellerID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}