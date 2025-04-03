using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IAuthenticationService _authService;

        public SellerService(ISellerRepository sellerRepository, IAuthenticationService authService)
        {
            _sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            _authService = authService;
        }

        public async Task<Seller> RegisterSellerAsync(string sellerName, string password)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can register sellers.");

            var seller = new Seller(
                id: Guid.NewGuid(),
                sellerName: sellerName,
                registrationDate: DateTime.UtcNow,
                avgRating: 0,
                password: password
            );

            return await _sellerRepository.AddAsync(seller, currentRole);
        }

        public async Task<Seller> RegisterSellerAsync(string sellerName, string password, bool proc)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can register sellers.");

            return await _sellerRepository.AddAsync(Guid.NewGuid(), sellerName, password, DateTime.UtcNow, 0, currentRole);
        }

        public async Task<Seller> GetSellerByIdAsync(Guid id)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentSellerId = id;

            // Проверяем, что продавец запрашивает свои данные, если он не администратор
            if (currentRole != UserRole.Admin && currentRole == UserRole.Seller && id != currentSellerId)
                throw new UnauthorizedAccessException("You can only view your own seller profile.");

            var seller = await _sellerRepository.GetByIdAsync(id, currentRole);
            if (seller == null)
                throw new KeyNotFoundException($"Seller with ID {id} not found.");

            return seller;
        }

        public async Task<List<Seller>> GetAllSellersAsync()
        {
            var currentRole = _authService.GetCurrentRole();
            return await _sellerRepository.GetAllAsync(currentRole);
        }

        public async Task UpdateSellerRatingAsync(Guid sellerId, double newRating)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentSellerId = sellerId;

            // Проверяем, что продавец обновляет свой рейтинг, если он не администратор
            if (currentRole != UserRole.Admin && currentRole == UserRole.Seller && sellerId != currentSellerId)
                throw new UnauthorizedAccessException("You can only update your own seller rating.");

            var seller = await _sellerRepository.GetByIdAsync(sellerId, currentRole);
            if (seller == null)
                throw new KeyNotFoundException($"Seller with ID {sellerId} not found.");

            seller.UpdateRating(newRating);
            await _sellerRepository.UpdateAsync(seller, currentRole);
        }
    }
}