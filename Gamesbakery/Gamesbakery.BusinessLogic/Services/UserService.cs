using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Repositories;
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
            var userDto = new UserProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                RegistrationDate = DateTime.UtcNow,
                Country = country,
                Password = password,
                IsBlocked = false,
                Balance = 0m,
                TotalSpent = 0m
            };
            return await _userRepository.AddAsync(userDto, UserRole.User);
        }
        public async Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country, bool proc)
        {
            return await RegisterUserAsync(username, email, password, country);
        }
        public async Task<IEnumerable<UserListDTO>> GetAllUsersExceptAsync(Guid excludedUserId, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can view users");
            var users = await _userRepository.GetAllAsync(role);
            return users.Where(u => u.Id != excludedUserId).Select(u => new UserListDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsBlocked = u.IsBlocked
            });
        }
        public async Task<UserListDTO> GetByUsernameAsync(string username, UserRole role)
        {
            var user = await _userRepository.GetByUsernameAsync(username, role);
            return user != null ? new UserListDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsBlocked = user.IsBlocked
            } : null;
        }
        public async Task<UserProfileDTO> GetUserByIdAsync(Guid userId, Guid? curUserId, UserRole role)
        {
            if (role != UserRole.Admin && userId != curUserId)
                throw new UnauthorizedAccessException("Can only view own profile");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user == null)
                throw new KeyNotFoundException($"User {userId} not found");
            return user;
        }
        public async Task<UserProfileDTO> GetUserByEmailAsync(string email, Guid? curUserId, UserRole role)
        {
            var user = await _userRepository.GetByEmailAsync(email, role);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found");
            if (role != UserRole.Admin && user.Id != curUserId)
                throw new UnauthorizedAccessException("Can only view own profile");
            return user;
        }
        public async Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance, Guid? curUserId, UserRole role)
        {
            if (role != UserRole.Admin && userId != curUserId)
                throw new UnauthorizedAccessException("Can only update own balance");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user == null)
                throw new KeyNotFoundException($"User {userId} not found");
            user.Balance = newBalance;
            return await _userRepository.UpdateAsync(user, role);
        }
        public async Task<UserProfileDTO> BlockUserAsync(Guid userId, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can block users");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user == null)
                throw new KeyNotFoundException($"User {userId} not found");
            user.IsBlocked = true;
            return await _userRepository.UpdateAsync(user, role);
        }
        public async Task<UserProfileDTO> UnblockUserAsync(Guid userId, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can unblock users");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user == null)
                throw new KeyNotFoundException($"User {userId} not found");
            user.IsBlocked = false;
            return await _userRepository.UpdateAsync(user, role);
        }
    }
}