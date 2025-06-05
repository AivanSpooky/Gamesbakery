using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.DataAccess.ClickHouse
{
    public class ClickHouseCategoryRepository : ICategoryRepository
    {
        private readonly ClickHouseConnection _connection;

        public ClickHouseCategoryRepository(string connectionString)
        {
            _connection = new ClickHouseConnection(connectionString);
        }

        public async Task<Category> AddAsync(Category category, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can add categories.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Categories (CategoryID, Name, Description) VALUES (@CategoryID, @Name, @Description)";
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "CategoryID",
                Value = category.Id.ToString("N") // Convert GUID to string without hyphens
            });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Name", Value = category.GenreName });
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "Description", Value = category.Description ?? "" });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
            return category;
        }

        public async Task<Category> GetByIdAsync(Guid id, UserRole role)
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT CategoryID, Name, Description FROM Categories WHERE CategoryID = @CategoryID";
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "CategoryID",
                Value = id.ToString("N") // Convert GUID to string without hyphens
            });
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var category = new Category(
                    Guid.Parse(reader.GetString(0)), // Parse string back to GUID
                    reader.GetString(1),
                    reader.GetString(2));
                await _connection.CloseAsync();
                return category;
            }
            await _connection.CloseAsync();
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        public async Task<List<Category>> GetAllAsync(UserRole role)
        {
            for (int i = 0; i < 3; i++) // Retry 3 times
            {
                try
                {
                    await _connection.OpenAsync();
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = "SELECT CategoryID, Name, Description FROM Categories";
                    var reader = await cmd.ExecuteReaderAsync();
                    var categories = new List<Category>();
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new Category(
                            reader.GetGuid(0),
                            reader.GetString(1),
                            reader.GetString(2)));
                    }
                    return categories;
                }
                catch (Exception ex) when (i < 2)
                {
                    await Task.Delay(1000); // Wait 1 second before retry
                    continue;
                }
                finally
                {
                    if (_connection.State == System.Data.ConnectionState.Open)
                        await _connection.CloseAsync();
                }
            }
            throw new Exception("Failed to connect to ClickHouse after 3 attempts.");
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin) throw new UnauthorizedAccessException("Only admins can delete categories.");
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE Categories DELETE WHERE CategoryID = @CategoryID";
            cmd.Parameters.Add(new ClickHouseParameter { ParameterName = "CategoryID", Value = id });
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}