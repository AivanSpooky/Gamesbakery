namespace Gamesbakery.Core.DTOs.OrderDTO
{
    public class OrderListDTO // (для списка заказов)
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Price { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
    }
}