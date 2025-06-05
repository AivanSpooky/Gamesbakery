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
    public class ClickHouseUserRepository : IUserRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseUserRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users";
            var reader = await cmd.ExecuteReaderAsync();
            var users = new List<User>();
            while (await reader.ReadAsync())
            {
                users.Add(new User(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetBoolean(6),
                    reader.GetDecimal(7)));
            }
            await _connection.CloseAsync();
            return users;
        }

        public async Task<User> AddAsync(User user, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can add users.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance) VALUES (@UserID, @Name, @Email, @RegistrationDate, @Country, @Password, @IsBlocked, @Balance)";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = user.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = user.Username });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Email", Value = user.Email });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "RegistrationDate", Value = user.RegistrationDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Country", Value = user.Country });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Password", Value = user.Password });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsBlocked", Value = user.IsBlocked });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Balance", Value = user.Balance });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return user;
        }

        public async Task<User> AddAsync(Guid userId, string username, string email, string password, string country, DateTime registrationDate, bool isBlocked, decimal balance, UserRole role)
        {
            var user = new User(userId, username, email, registrationDate, country, password, isBlocked, balance);
            return await AddAsync(user, role);
        }

        public async Task<User> GetByIdAsync(Guid userId, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE UserID = @UserID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = userId });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetBoolean(6),
                    reader.GetDecimal(7));
                await _connection.CloseAsync();
                return user;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        public async Task<User> UpdateAsync(User user, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can update users.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Users UPDATE Name = @Name, Email = @Email, RegistrationDate = @RegistrationDate, Country = @Country, Password = @Password, IsBlocked = @IsBlocked, Balance = @Balance WHERE UserID = @UserID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = user.Id });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = user.Username });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Email", Value = user.Email });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "RegistrationDate", Value = user.RegistrationDate });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Country", Value = user.Country });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Password", Value = user.Password });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "IsBlocked", Value = user.IsBlocked });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Balance", Value = user.Balance });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return user;
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can delete users.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Users DELETE WHERE UserID = @UserID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "UserID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<User> GetByEmailAsync(string email, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE Email = @Email";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Email", Value = email });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetBoolean(6),
                    reader.GetDecimal(7));
                await _connection.CloseAsync();
                return user;
            }
            await _connection.CloseAsync();
            return null;
        }

        public async Task<User> GetByUsernameAsync(string username, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE Name = @Name";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = username });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetBoolean(6),
                    reader.GetDecimal(7));
                await _connection.CloseAsync();
                return user;
            }
            await _connection.CloseAsync();
            return null;
        }
        public decimal GetUserTotalSpent(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}