using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Gamesbakery.Core.DTOs.GiftDTO;

namespace Gamesbakery.BusinessLogic.Tests
{
    public class GiftServiceTests
    {
        private readonly Mock<IGiftRepository> _giftRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly GiftService _giftService;

        public GiftServiceTests()
        {
            _giftRepositoryMock = new Mock<IGiftRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _giftService = new GiftService(
                _giftRepositoryMock.Object,
                _orderItemRepositoryMock.Object,
                _gameRepositoryMock.Object,
                _authServiceMock.Object);
        }

        [Fact(DisplayName = "Создание подарка с корректными данными - успех")]
        public async Task CreateGiftAsync_ValidData_UpdatesIsGiftedAndAddsGift()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var role = UserRole.User;
            var orderItem = new OrderItem(orderItemId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "KEY123");

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, role, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync(orderItem);
            _orderItemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<OrderItem>(), role))
                .Returns(Task.CompletedTask);
            _giftRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Gift>()))
                .ReturnsAsync((Gift g) => g);
            

            // Act
            await _giftService.CreateGiftAsync(senderId, recipientId, orderItemId, role);

            // Assert
            Assert.True(orderItem.IsGifted);
            _giftRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Gift>(g =>
                g.SenderId == senderId &&
                g.RecipientId == recipientId &&
                g.OrderItemId == orderItemId)), Times.Once());
            _orderItemRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<OrderItem>(oi =>
                oi.Id == orderItemId && oi.IsGifted == true), role), Times.Once());
        }

        [Fact(DisplayName = "Создание подарка с несуществующим OrderItem - исключение")]
        public async Task CreateGiftAsync_InvalidOrderItem_ThrowsKeyNotFoundException()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var role = UserRole.User;

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, role, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync((OrderItem)null);
            

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _giftService.CreateGiftAsync(senderId, recipientId, orderItemId, role));
        }

        [Fact(DisplayName = "Получение подарков по отправителю - успех")]
        public async Task GetGiftsBySenderAsync_ValidData_ReturnsGiftDTOs()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var role = UserRole.User;
            var sentGifts = new List<SentGift>
            {
                new SentGift
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId,
                    RecipientId = Guid.NewGuid(),
                    OrderItemId = orderItemId,
                    GiftDate = DateTime.UtcNow
                }
            };
            var orderItem = new OrderItem(orderItemId, Guid.NewGuid(), gameId, Guid.NewGuid(), "KEY123");
            var game = new Game(gameId, Guid.NewGuid(), "Test Game", 59.99m, DateTime.UtcNow, "Description", true, "Publisher");

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _giftRepositoryMock.Setup(repo => repo.GetBySenderIdAsync(senderId, role))
                .ReturnsAsync(sentGifts);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, role, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync(orderItem);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, role))
                .ReturnsAsync(game);
            

            // Act
            var result = await _giftService.GetGiftsBySenderAsync(senderId, role);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var giftDTO = result.First();
            Assert.Equal("Test Game", giftDTO.GameTitle);
            Assert.Equal(sentGifts[0].Id, giftDTO.Id);
            Assert.Equal("KEY123", giftDTO.Key);
            Assert.Equal(gameId, giftDTO.GameId);
            Assert.Equal(sentGifts[0].SenderId, giftDTO.SenderId);
            Assert.Equal(sentGifts[0].RecipientId, giftDTO.RecipientId);
            Assert.Equal(sentGifts[0].GiftDate, giftDTO.GiftDate);
        }

        //[Fact(DisplayName = "Получение подарка по ID - успех")]
        //public async Task GetGiftByIdAsync_ValidData_ReturnsGiftDTO()
        //{
        //    // Arrange
        //    var giftId = Guid.NewGuid();
        //    var gameId = Guid.NewGuid();
        //    var orderItemId = Guid.NewGuid();
        //    var username = "JohnDoe";
        //    var email = "john.doe@example.com";
        //    var password = "password123";
        //    var country = "United States";
        //    var userId = Guid.NewGuid();
        //    var user = new User(userId, username, email, DateTime.UtcNow, country, password, false, 0);
        //    var role = UserRole.User;
        //    var orderItem = new OrderItem(orderItemId, Guid.NewGuid(), gameId, Guid.NewGuid(), "KEY123");
        //    var gift = new Gift(giftId, userId, Guid.NewGuid(), orderItemId, DateTime.UtcNow)
        //    {
        //        OrderItem = orderItem
        //    };
        //    var game = new Game(gameId, Guid.NewGuid(), "Test Game", 59.99m, DateTime.UtcNow, "Description", true, "Publisher");

        //    _userRepositoryMock.Setup(repo => repo.AddAsync(user, role))
        //        .ReturnsAsync(user);
        //    _gameRepositoryMock.Setup(repo => repo.AddAsync(game, role))
        //        .ReturnsAsync(game);
        //    _giftRepositoryMock.Setup(repo => repo.AddAsync(gift))
        //        .ReturnsAsync(gift);
        //    _orderItemRepositoryMock.Setup(repo => repo.AddAsync(orderItem, role))
        //        .ReturnsAsync(orderItem);
        //    _giftRepositoryMock.Setup(repo => repo.GetByIdAsync(giftId))
        //        .ReturnsAsync(gift);
        //    _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, role))
        //        .ReturnsAsync(game);
        //    _authServiceMock.Setup(auth => auth.GetCurrentRole())
        //        .Returns(role);

        //    // Act
        //    var result = await _giftService.GetGiftByIdAsync(giftId, role);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(giftId, result.Id);
        //    Assert.Equal("Test Game", result.GameTitle);
        //    Assert.Equal("KEY123", result.Key);
        //}

        [Fact(DisplayName = "Удаление подарка - успех")]
        public async Task DeleteGiftAsync_ValidData_CallsRepository()
        {
            // Arrange
            var giftId = Guid.NewGuid();
            var role = UserRole.Admin;
            var gift = new Gift(giftId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _giftRepositoryMock.Setup(repo => repo.GetByIdAsync(giftId, role, GiftSource.All, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync(gift);
            _giftRepositoryMock.Setup(repo => repo.DeleteAsync(giftId))
                .Returns(Task.CompletedTask);
            

            // Act
            await _giftService.DeleteGiftAsync(giftId, role);

            // Assert
            _giftRepositoryMock.Verify(repo => repo.DeleteAsync(giftId), Times.Once());
        }

        [Fact(DisplayName = "Удаление несуществующего подарка - исключение")]
        public async Task DeleteGiftAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var giftId = Guid.NewGuid();
            var role = UserRole.Admin;

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _giftRepositoryMock.Setup(repo => repo.GetByIdAsync(giftId, role, GiftSource.All, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync((Gift)null);
            

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _giftService.DeleteGiftAsync(giftId, role));
        }

        [Fact(DisplayName = "Удаление подарка не админом - исключение")]
        public async Task DeleteGiftAsync_NonAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var giftId = Guid.NewGuid();
            var role = UserRole.User;
            var gift = new Gift(giftId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

            _authServiceMock.Setup(auth => auth.GetCurrentRole())
                .Returns(role);
            _giftRepositoryMock.Setup(repo => repo.GetByIdAsync(giftId, role, GiftSource.All, _authServiceMock.Object.GetCurrentUserId()))
                .ReturnsAsync(gift);
            

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _giftService.DeleteGiftAsync(giftId, role));
        }
    }
}