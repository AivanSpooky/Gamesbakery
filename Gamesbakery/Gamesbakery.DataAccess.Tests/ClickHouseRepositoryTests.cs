using ClickHouse.Ado;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess.ClickHouse;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Gamesbakery.DataAccess.Tests
{
    public class ClickHouseRepositoryTests : IDisposable
    {
        private readonly ClickHouseConnection _connection;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IGiftRepository _giftRepository;
        private readonly string _connectionString = "Host=localhost;Port=9000;Database=gamesbakery;User=default;Password=1";

        public ClickHouseRepositoryTests()
        {
            _connection = new ClickHouseConnection(_connectionString);
            _categoryRepository = new ClickHouseCategoryRepository(_connectionString);
            _gameRepository = new ClickHouseGameRepository(_connectionString);
            _giftRepository = new ClickHouseGiftRepository(_connectionString);

            // Initialize tables (run once if not exists)
            SetupTables().GetAwaiter().GetResult();
        }

        private async Task SetupTables()
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();

            // Create Categories table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    CategoryID UUID,
                    Name String,
                    Description String
                ) ENGINE = MergeTree() ORDER BY CategoryID";
            await cmd.ExecuteNonQueryAsync();

            // Create Games table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Games (
                    GameID UUID,
                    CategoryID UUID,
                    Title String,
                    Price Decimal(10, 2),
                    ReleaseDate DateTime,
                    Description String,
                    OriginalPublisher String,
                    IsForSale UInt8
                ) ENGINE = MergeTree() ORDER BY GameID";
            await cmd.ExecuteNonQueryAsync();

            // Create Gifts table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Gifts (
                    GiftID UUID,
                    SenderID UUID,
                    RecipientID UUID,
                    OrderItemID UUID,
                    GiftDate DateTime
                ) ENGINE = MergeTree() ORDER BY GiftID";
            await cmd.ExecuteNonQueryAsync();

            await _connection.CloseAsync();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        //[Fact]
        //public async Task AddCategoryAsync_SuccessfulAdd_ReturnsAddedCategory()
        //{
        //    // Arrange
        //    var category = new Category(Guid.NewGuid(), "Action", "Action-packed games");
        //    var role = UserRole.Admin;

        //    // Act
        //    var addedCategory = await _categoryRepository.AddAsync(category, role);

        //    // Assert
        //    Assert.NotNull(addedCategory);
        //    Assert.Equal(category.Id, addedCategory.Id);
        //    Assert.Equal(category.GenreName, addedCategory.GenreName);
        //    Assert.Equal(category.Description, addedCategory.Description);
        //}

        //[Fact]
        //public async Task GetByIdCategoryAsync_ExistingId_ReturnsCategory()
        //{
        //    // Arrange
        //    var category = new Category(Guid.NewGuid(), "RPG", "Role-playing games");
        //    var role = UserRole.Admin;
        //    await _categoryRepository.AddAsync(category, role);

        //    // Act
        //    var retrievedCategory = await _categoryRepository.GetByIdAsync(category.Id, role);

        //    // Assert
        //    Assert.NotNull(retrievedCategory);
        //    Assert.Equal(category.Id, retrievedCategory.Id);
        //    Assert.Equal(category.GenreName, retrievedCategory.GenreName);
        //    Assert.Equal(category.Description, retrievedCategory.Description);
        //}

        //[Fact]
        //public async Task GetAllCategoriesAsync_EmptyTable_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var role = UserRole.Admin;

        //    // Act
        //    var categories = await _categoryRepository.GetAllAsync(role);

        //    // Assert
        //    Assert.NotNull(categories);
        //    Assert.Empty(categories);
        //}

        //[Fact]
        //public async Task AddGameAsync_SuccessfulAdd_ReturnsAddedGame()
        //{
        //    // Arrange
        //    var categoryId = Guid.NewGuid();
        //    await _categoryRepository.AddAsync(new Category(categoryId, "Action", "Action games"), UserRole.Admin);
        //    var game = new Game(Guid.NewGuid(), categoryId, "Game 1", 29.99m, DateTime.Now, "Great game", true, "Publisher A");
        //    var role = UserRole.Seller;

        //    // Act
        //    var addedGame = await _gameRepository.AddAsync(game, role);

        //    // Assert
        //    Assert.NotNull(addedGame);
        //    Assert.Equal(game.Id, addedGame.Id);
        //    Assert.Equal(game.CategoryId, addedGame.CategoryId);
        //    Assert.Equal(game.Title, addedGame.Title);

        //}

        //[Fact]
        //public async Task AddGiftAsync_SuccessfulAdd_ReturnsAddedGift()
        //{
        //    // Arrange
        //    var gift = new Gift(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

        //    // Act
        //    var addedGift = await _giftRepository.AddAsync(gift);

        //    // Assert
        //    Assert.NotNull(addedGift);
        //    Assert.Equal(gift.Id, addedGift.Id);
        //    Assert.Equal(gift.SenderId, addedGift.SenderId);

        //    // Clean up
        //    await _giftRepository.DeleteAsync(gift.Id);
        //}
    }
}