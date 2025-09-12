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

        public async Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> orderItemIds)
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

            // Проверка элементов заказа
            decimal totalPrice = 0;
            var orderItems = new List<OrderItem>();
            foreach (var orderItemId in orderItemIds)
            {
                var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, currentRole, currentUserId);
                if (orderItem == null)
                    throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found.");
                if (orderItem.OrderId != null && orderItem.OrderId != Guid.Empty)
                    throw new InvalidOperationException($"OrderItem with ID {orderItemId} is already part of an order.");

                var game = await _gameRepository.GetByIdAsync(orderItem.GameId, currentRole);
                if (game == null)
                    throw new KeyNotFoundException($"Game with ID {orderItem.GameId} not found.");
                if (!game.IsForSale)
                    throw new InvalidOperationException($"Game with ID {orderItem.GameId} is not for sale.");

                totalPrice += game.Price;
                orderItems.Add(orderItem);
            }

            // Проверка баланса
            if (user.Balance < totalPrice)
                throw new InvalidOperationException($"Insufficient balance to complete the order. Required: {totalPrice}, Available: {user.Balance}");

            // Создание заказа и обновление элементов в транзакции
            var order = new Order(Guid.NewGuid(), userId, DateTime.UtcNow, totalPrice, false, false);
            try
            {
                // Создать заказ
                var createdOrder = await _orderRepository.AddAsync(order, currentRole);

                // Обновить OrderId в существующих OrderItems
                foreach (var orderItem in orderItems)
                {
                    orderItem.SetOrderId(createdOrder.Id);
                    await _orderItemRepository.UpdateAsync(orderItem, UserRole.Admin); // Use Admin role to allow OrderID update
                }

                // Обновить баланс пользователя
                user.UpdateBalance(user.Balance - totalPrice);
                await _userRepository.UpdateAsync(user, currentRole);
                //await transaction.CommitAsync();
                return MapToListDTO(createdOrder);
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to create order.", ex);
            }
        }

        public async Task<OrderDetailsDTO> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                //_logger.Information("Retrieving order with OrderId: {OrderId}", orderId);
                var role = _authService.GetCurrentRole();
                var currentUserId = role == UserRole.Admin ? (Guid?)null : _authService.GetCurrentUserId();
                if (role != UserRole.Admin && currentUserId == null)
                {
                    //_logger.Error("CurrentUserId is null for non-admin role when retrieving OrderId: {OrderId}", orderId);
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }

                var order = await _orderRepository.GetByIdAsync(orderId, role, currentUserId);
                if (order == null)
                {
                    //_logger.Warning("Order with ID {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Order with ID {orderId} not found.");
                }

                var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, role);
                var orderItemDTOs = orderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    GameId = oi.GameId,
                    SellerId = oi.SellerId,
                    Key = oi.Key
                }).ToList();

                var orderDTO = new OrderDetailsDTO
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    TotalPrice = order.Price,
                    IsCompleted = order.IsCompleted,
                    IsOverdue = order.IsOverdue,
                    OrderItems = orderItemDTOs
                };
                //_logger.Information("Successfully retrieved order with OrderId: {OrderId}", orderId);
                return orderDTO;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Failed to retrieve order with OrderId: {OrderId}", orderId);
                throw;
            }
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
            var curUserId = _authService.GetCurrentUserId();
            var currentRole = _authService.GetCurrentRole();

            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, currentRole, sellerId);
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