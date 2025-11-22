using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;

/*
Заказы переходят в статус «выполнен» только после того, как все ключи (OrderItem.Key) для игр в заказе были сгенерированы и отправлены пользователю.
Если ключи не сгенерированы в течение 14 дней, заказ помечается как «просрочен» (IsOverdue).
Если заказ уже выполнен или просрочен, он больше не обрабатывается.*/

namespace Gamesbakery.BusinessLogic.Schedulers
{
    public class OrderStatusScheduler : IOrderStatusScheduler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IAuthenticationService _authService;

        public OrderStatusScheduler(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IAuthenticationService authService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _authService = authService;
        }

        //public async Task UpdateOrderStatusesAsync()
        //{
        //    var currentRole = _authService.GetCurrentRole();
        //    var orders = await _orderRepository.GetByUserIdAsync(Guid.Empty, currentRole);

        //    foreach (var order in orders)
        //    {
        //        if (order.IsCompleted || order.IsOverdue)
        //            continue;

        //        // Проверяем, прошло ли 14 дней
        //        var daysSinceOrder = (DateTime.UtcNow - order.OrderDate).TotalDays;
        //        if (daysSinceOrder >= 14)
        //        {
        //            order.MarkAsOverdue();
        //            await _orderRepository.UpdateAsync(order, currentRole);
        //            continue;
        //        }

        //        // Проверяем, все ли ключи сгенерированы
        //        var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id, currentRole);
        //        var allKeysGenerated = orderItems.All(item => !string.IsNullOrWhiteSpace(item.Key));

        //        if (allKeysGenerated)
        //        {
        //            order.Complete();
        //            await _orderRepository.UpdateAsync(order, currentRole);
        //        }
        //    }
        //}
    }
}