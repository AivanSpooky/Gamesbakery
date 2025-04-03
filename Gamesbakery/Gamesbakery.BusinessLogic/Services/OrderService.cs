using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core;

namespace Gamesbakery.BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IAuthenticationService _authService;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            IGameRepository gameRepository,
            ISellerRepository sellerRepository,
            IAuthenticationService authService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _sellerRepository = sellerRepository;
            _authService = authService;
        }

        public async Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> gameIds)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            // Проверяем, что пользователь создаёт заказ от своего имени, если он не администратор
            if (currentRole != UserRole.Admin && userId != currentUserId)
                throw new UnauthorizedAccessException("You can only create orders for yourself.");

            // Проверка пользователя
            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            if (user.IsBlocked)
                throw new InvalidOperationException("Cannot create order for a blocked user.");

            // Проверка игр
            decimal totalPrice = 0;
            var games = new List<(Game Game, Guid SellerId)>();
            foreach (var gameId in gameIds)
            {
                var game = await _gameRepository.GetByIdAsync(gameId, currentRole);
                if (game == null)
                    throw new KeyNotFoundException($"Game with ID {gameId} not found.");
                if (!game.IsForSale)
                    throw new InvalidOperationException($"Game with ID {gameId} is not for sale.");

                var sellers = await _sellerRepository.GetAllAsync(currentRole);
                var seller = sellers?.FirstOrDefault();
                if (seller == null)
                    throw new InvalidOperationException("No sellers available.");

                totalPrice += game.Price;
                games.Add((game, seller.Id));
            }

            // Проверка баланса
            if (user.Balance < totalPrice)
                throw new InvalidOperationException("Insufficient balance to complete the order.");

            // Создание заказа
            var order = new Order(Guid.NewGuid(), userId, DateTime.UtcNow, totalPrice, false, false);
            var createdOrder = await _orderRepository.AddAsync(order, currentRole);

            // Создание элементов заказа
            foreach (var (game, sellerId) in games)
            {
                var orderItem = new OrderItem(Guid.NewGuid(), createdOrder.Id, game.Id, sellerId, null);
                await _orderItemRepository.AddAsync(orderItem, currentRole);
            }

            // Обновление баланса пользователя
            user.UpdateBalance(user.Balance - totalPrice);
            await _userRepository.UpdateAsync(user, currentRole);

            return MapToListDTO(createdOrder);
        }

        public async Task<OrderListDTO> GetOrderByIdAsync(Guid id)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            var order = await _orderRepository.GetByIdAsync(id, currentRole);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");

            // Проверяем, что пользователь запрашивает свой заказ, если он не администратор
            if (currentRole != UserRole.Admin && order.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own orders.");

            return MapToListDTO(order);
        }

        public async Task<List<OrderListDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            // Проверяем, что пользователь запрашивает свои заказы, если он не администратор
            if (currentRole != UserRole.Admin && userId != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own orders.");

            var orders = await _orderRepository.GetByUserIdAsync(userId, currentRole);
            return orders.Select(MapToListDTO).ToList();
        }

        public async Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId)
        {
            var currentRole = _authService.GetCurrentRole();

            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, currentRole);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found.");

            if (orderItem.SellerId != sellerId)
                throw new InvalidOperationException("Seller can only set keys for their own OrderItems.");

            orderItem.SetKey(key);
            await _orderItemRepository.UpdateAsync(orderItem, currentRole);
        }

        public async Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentSellerId = sellerId;

            // Проверяем, что продавец запрашивает свои элементы заказов, если он не администратор
            if (currentRole != UserRole.Admin && currentRole == UserRole.Seller && sellerId != currentSellerId)
                throw new UnauthorizedAccessException("You can only view your own order items.");

            var orderItems = await _orderItemRepository.GetBySellerIdAsync(sellerId, currentRole);
            return orderItems.Select(MapToOrderItemDTO).ToList();
        }

        private OrderListDTO MapToListDTO(Order order)
        {
            return new OrderListDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Price = order.Price,
                IsCompleted = order.IsCompleted,
                IsOverdue = order.IsOverdue
            };
        }

        private OrderItemDTO MapToOrderItemDTO(OrderItem orderItem)
        {
            return new OrderItemDTO
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                GameId = orderItem.GameId,
                SellerId = orderItem.SellerId,
                Key = orderItem.Key
            };
        }
    }
}