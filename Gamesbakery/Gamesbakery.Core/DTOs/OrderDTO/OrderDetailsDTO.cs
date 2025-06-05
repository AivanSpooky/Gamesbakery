namespace Gamesbakery.Core.DTOs.OrderDTO
{
    public class OrderDetailsDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}
