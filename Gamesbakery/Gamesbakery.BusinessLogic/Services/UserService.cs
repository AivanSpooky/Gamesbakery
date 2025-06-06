﻿using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core;

namespace Gamesbakery.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authService;

        public UserService(IUserRepository userRepository, IAuthenticationService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country)
        {
            var currentRole = _authService.GetCurrentRole();
            var user = new User(Guid.NewGuid(), username, email, DateTime.UtcNow, country, password, false, 0);
            var createdUser = await _userRepository.AddAsync(user, currentRole);
            return MapToProfileDTO(createdUser);
        }

        public async Task<IEnumerable<UserListDTO>> GetAllUsersExceptAsync(Guid excludedUserId)
        {
            if (excludedUserId == Guid.Empty)
                throw new ArgumentException("ExcludedUserId cannot be empty.", nameof(excludedUserId));

            var users = await _userRepository.GetAllAsync();
            return users
                .Where(u => u.Id != excludedUserId)
                .Select(u => new UserListDTO
                {
                    Id = u.Id,
                    Username = u.Username
                });
        }

        public async Task<UserListDTO> GetByUsernameAsync(string username, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));

            var user = await _userRepository.GetByUsernameAsync(username, role);
            if (user == null)
                return null;

            return new UserListDTO
            {
                Id = user.Id,
                Username = user.Username
            };
        }

        public async Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country, bool proc)
        {
            var currentRole = _authService.GetCurrentRole();
            var createdUser = await _userRepository.AddAsync(Guid.NewGuid(), username, email, password, country, DateTime.UtcNow, false, 0, currentRole);
            return MapToProfileDTO(createdUser);
        }

        public async Task<UserProfileDTO> GetUserByIdAsync(Guid userId)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            // Проверяем, что пользователь запрашивает свои данные, если он не администратор
            if (currentRole != UserRole.Admin && userId != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own profile.");

            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            // Получаем TotalSpent через репозиторий
            var totalSpent = _userRepository.GetUserTotalSpent(userId);

            var userProfile = MapToProfileDTO(user);
            userProfile.TotalSpent = totalSpent; // Добавляем TotalSpent
            return userProfile;
        }

        public async Task<UserProfileDTO> GetUserByEmailAsync(string email)
        {
            var currentRole = _authService.GetCurrentRole();
            var user = await _userRepository.GetByEmailAsync(email, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found.");

            // Проверяем, что пользователь запрашивает свои данные, если он не администратор
            var currentUserId = _authService.GetCurrentUserId();
            if (currentRole != UserRole.Admin && user.Id != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own profile.");

            // Получаем TotalSpent через репозиторий
            var totalSpent = _userRepository.GetUserTotalSpent(user.Id);

            var userProfile = MapToProfileDTO(user);
            userProfile.TotalSpent = totalSpent; // Добавляем TotalSpent
            return userProfile;
        }

        public async Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            // Проверяем, что пользователь обновляет свой баланс, если он не администратор
            if (currentRole != UserRole.Admin && userId != currentUserId)
                throw new UnauthorizedAccessException("You can only update your own balance.");

            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.UpdateBalance(newBalance);
            var updatedUser = await _userRepository.UpdateAsync(user, currentRole);

            // Получаем TotalSpent через репозиторий
            var totalSpent = _userRepository.GetUserTotalSpent(userId);

            var userProfile = MapToProfileDTO(updatedUser);
            userProfile.TotalSpent = totalSpent; // Добавляем TotalSpent
            return userProfile;
        }

        public async Task<UserProfileDTO> BlockUserAsync(Guid userId)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can block users.");

            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.Block();
            var updatedUser = await _userRepository.UpdateAsync(user, currentRole);

            // Получаем TotalSpent через репозиторий
            var totalSpent = _userRepository.GetUserTotalSpent(userId);

            var userProfile = MapToProfileDTO(updatedUser);
            userProfile.TotalSpent = totalSpent; // Добавляем TotalSpent
            return userProfile;
        }

        public async Task<UserProfileDTO> UnblockUserAsync(Guid userId)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can unblock users.");

            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.Unblock();
            var updatedUser = await _userRepository.UpdateAsync(user, currentRole);

            // Получаем TotalSpent через репозиторий
            var totalSpent = _userRepository.GetUserTotalSpent(userId);

            var userProfile = MapToProfileDTO(updatedUser);
            userProfile.TotalSpent = totalSpent; // Добавляем TotalSpent
            return userProfile;
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
                Balance = user.Balance,
                TotalSpent = 0 // Значение будет перезаписано в методах
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