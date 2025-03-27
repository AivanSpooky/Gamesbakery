using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs.UserDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country)
        {
            var user = new User(Guid.NewGuid(), username, email, DateTime.UtcNow, country, password, false, 0);
            var createdUser = await _userRepository.AddAsync(user);
            return MapToProfileDTO(createdUser);
        }

        public async Task<UserProfileDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found.");
            return MapToProfileDTO(user);
        }

        public async Task<UserProfileDTO> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found.");
            return MapToProfileDTO(user);
        }

        public async Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.UpdateBalance(newBalance);
            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToProfileDTO(updatedUser);
        }

        public async Task BlockUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.Block();
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnblockUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.Unblock();
            await _userRepository.UpdateAsync(user);
        }

        private UserProfileDTO MapToProfileDTO(User user)
        {
            return new UserProfileDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                Country = user.Country,
                IsBlocked = user.IsBlocked,
                Balance = user.Balance
            };
        }

        private UserListDTO MapToListDTO(User user)
        {
            return new UserListDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }
    }
}