using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.OrderDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ISellerRepository _sellerRepository;

        public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IUserRepository userRepository, IGameRepository gameRepository, ISellerRepository sellerRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _sellerRepository = sellerRepository;
        }

        public async Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> gameIds)
        {
            // Проверка пользователя
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            if (user.IsBlocked)
                throw new InvalidOperationException("Cannot create order for a blocked user.");

            // Проверка игр
            decimal totalPrice = 0;
            var games = new List<(Game Game, Guid SellerId)>();
            foreach (var gameId in gameIds)
            {
                var game = await _gameRepository.GetByIdAsync(gameId);
                if (game == null)
                    throw new KeyNotFoundException($"Game with ID {gameId} not found.");
                if (!game.IsForSale)
                    throw new InvalidOperationException($"Game with ID {gameId} is not for sale.");

                var sellers = await _sellerRepository.GetAllAsync();
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
            var createdOrder = await _orderRepository.AddAsync(order);

            // Создание элементов заказа
            foreach (var (game, sellerId) in games)
            {
                var orderItem = new OrderItem(Guid.NewGuid(), createdOrder.Id, game.Id, sellerId, null);
                await _orderItemRepository.AddAsync(orderItem);
            }

            // Обновление баланса пользователя
            user.UpdateBalance(user.Balance - totalPrice);
            await _userRepository.UpdateAsync(user);

            return MapToListDTO(createdOrder);
        }

        public async Task<OrderListDTO> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");
            return MapToListDTO(order);
        }

        public async Task<List<OrderListDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapToListDTO).ToList();
        }

        public async Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found.");

            if (orderItem.SellerId != sellerId)
                throw new InvalidOperationException("Seller can only set keys for their own OrderItems.");

            orderItem.SetKey(key);
            await _orderItemRepository.UpdateAsync(orderItem);
        }

        public async Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId)
        {
            var orderItems = await _orderItemRepository.GetBySellerIdAsync(sellerId);
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