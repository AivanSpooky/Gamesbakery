using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;

namespace Gamesbakery.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact(DisplayName = "Регистрация пользователя с корректными данными - успех")]
        public async Task RegisterUserAsync_ValidData_ReturnsUserDTO()
        {
            // Arrange
            var username = "JohnDoe";
            var email = "john.doe@example.com";
            var password = "password123";
            var country = "United States";
            var userId = Guid.NewGuid();
            var user = new User(userId, username, email, DateTime.UtcNow, country, password, false, 0);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password, country);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
        }

        [Fact(DisplayName = "Регистрация пользователя с пустым именем - исключение")]
        public async Task RegisterUserAsync_EmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var username = "";
            var email = "john.doe@example.com";
            var password = "password123";
            var country = "United States";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterUserAsync(username, email, password, country));
        }

        [Fact(DisplayName = "Регистрация пользователя с длинным паролем - исключение")]
        public async Task RegisterUserAsync_LongPassword_ThrowsArgumentException()
        {
            // Arrange
            var username = "JohnDoe";
            var email = "john.doe@example.com";
            var password = new string('a', 101); // Длина > 100
            var country = "United States";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterUserAsync(username, email, password, country));
        }

        [Fact(DisplayName = "Получение пользователя по ID - успех")]
        public async Task GetUserByIdAsync_UserExists_ReturnsUserDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact(DisplayName = "Получение пользователя по несуществующему ID - исключение")]
        public async Task GetUserByIdAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetUserByIdAsync(userId));
        }

        [Fact(DisplayName = "Обновление баланса с корректным значением - успех")]
        public async Task UpdateBalanceAsync_ValidBalance_ReturnsUserDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newBalance = 200m;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.UpdateBalanceAsync(userId, newBalance);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBalance, result.Balance);
        }

        [Fact(DisplayName = "Обновление баланса с отрицательным значением - исключение")]
        public async Task UpdateBalanceAsync_NegativeBalance_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newBalance = -50m;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateBalanceAsync(userId, newBalance));
        }

        [Fact(DisplayName = "Блокировка пользователя - успех")]
        public async Task BlockUserAsync_UserExists_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            await _userService.BlockUserAsync(userId);

            // Assert
            Assert.True(user.IsBlocked);
        }

        [Fact(DisplayName = "Блокировка несуществующего пользователя - исключение")]
        public async Task BlockUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.BlockUserAsync(userId));
        }
    }
}