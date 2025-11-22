using Allure.Commons;
using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.BusinessLogic.Tests.Patterns;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Moq;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection("UserServiceCollection")]
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _userService = new UserService(_userRepositoryMock.Object, _authServiceMock.Object);
        }

        [AllureXunit(DisplayName = "Регистрация пользователя с корректными данными - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-601")]
        [Trait("Category", "Unit")]
        public async Task RegisterUserAsync_ValidData_ReturnsUserDTO()
        {
            // Arrange
            var username = "JohnDoe";
            var email = "john.doe@example.com";
            var password = "password123";
            var country = "United States";
            var userId = Guid.NewGuid();
            var user = new UserBuilder()
                .WithId(userId)
                .WithUsername(username)
                .WithEmail(email)
                .WithPassword(password)
                .WithCountry(country)
                .WithBalance(0m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.Is<User>(u => u.Username == username && u.Email == email && u.Password == password), UserRole.User)).ReturnsAsync(user);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password, country);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
        }

        [AllureXunit(DisplayName = "Регистрация пользователя с пустым именем - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-602")]
        [Trait("Category", "Unit")]
        public async Task RegisterUserAsync_EmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var username = "";
            var email = "john.doe@example.com";
            var password = "password123";
            var country = "United States";
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterUserAsync(username, email, password, country));
        }

        [AllureXunit(DisplayName = "Регистрация пользователя с длинным паролем - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-603")]
        [Trait("Category", "Unit")]
        public async Task RegisterUserAsync_LongPassword_ThrowsArgumentException()
        {
            // Arrange
            var username = "JohnDoe";
            var email = "john.doe@example.com";
            var password = new string('a', 101); // Длина > 100
            var country = "United States";
            var userId = Guid.NewGuid();
            var validUser = new UserBuilder()
                .WithId(userId)
                .WithUsername(username)
                .WithEmail(email)
                .WithPassword("validPassword") // Valid password for mock
                .WithCountry(country)
                .WithBalance(0m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>(), UserRole.User)).Throws(new ArgumentException("Password must be between 1 and 100 characters.", nameof(password)));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterUserAsync(username, email, password, country));
        }

        [AllureXunit(DisplayName = "Получение пользователя по ID - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-604")]
        [Trait("Category", "Unit")]
        public async Task GetUserByIdAsync_UserExists_ReturnsUserDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserBuilder()
                .WithId(userId)
                .WithUsername("JohnDoe")
                .WithEmail("john.doe@example.com")
                .WithPassword("password123")
                .WithCountry("United States")
                .WithBalance(100m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [AllureXunit(DisplayName = "Получение пользователя по несуществующему ID - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-605")]
        [Trait("Category", "Unit")]
        public async Task GetUserByIdAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetUserByIdAsync(userId));
        }

        [AllureXunit(DisplayName = "Обновление баланса с корректным значением - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-606")]
        [Trait("Category", "Unit")]
        public async Task UpdateBalanceAsync_ValidBalance_ReturnsUserDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newBalance = 200m;
            var user = new UserBuilder()
                .WithId(userId)
                .WithUsername("JohnDoe")
                .WithEmail("john.doe@example.com")
                .WithPassword("password123")
                .WithCountry("United States")
                .WithBalance(100m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.Is<User>(u => u.Balance == newBalance), UserRole.User)).ReturnsAsync(user);

            // Act
            var result = await _userService.UpdateBalanceAsync(userId, newBalance);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBalance, result.Balance);
        }

        [AllureXunit(DisplayName = "Обновление баланса с отрицательным значением - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-607")]
        [Trait("Category", "Unit")]
        public async Task UpdateBalanceAsync_NegativeBalance_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newBalance = -50m;
            var user = new UserBuilder()
                .WithId(userId)
                .WithUsername("JohnDoe")
                .WithEmail("john.doe@example.com")
                .WithPassword("password123")
                .WithCountry("United States")
                .WithBalance(100m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateBalanceAsync(userId, newBalance));
        }

        [AllureXunit(DisplayName = "Блокировка пользователя - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-608")]
        [Trait("Category", "Unit")]
        public async Task BlockUserAsync_UserExists_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserBuilder()
                .WithId(userId)
                .WithUsername("JohnDoe")
                .WithEmail("john.doe@example.com")
                .WithPassword("password123")
                .WithCountry("United States")
                .WithBalance(100m)
                .Build();
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.Admin)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.Is<User>(u => u.IsBlocked), UserRole.Admin)).ReturnsAsync(user);

            // Act
            await _userService.BlockUserAsync(userId);

            // Assert
            Assert.True(user.IsBlocked);
        }

        [AllureXunit(DisplayName = "Блокировка несуществующего пользователя - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("User API Docs", "https://dev.gamesbakery.com/api/users")]
        [AllureIssue("USER-609")]
        [Trait("Category", "Unit")]
        public async Task BlockUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.Admin)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.BlockUserAsync(userId));
        }
    }
}