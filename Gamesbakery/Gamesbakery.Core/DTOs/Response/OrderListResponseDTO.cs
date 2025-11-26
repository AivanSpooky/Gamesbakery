namespace Gamesbakery.Core.DTOs.Response
{
    public class OrderListResponseDTO
    {
        public Guid OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsOverdue { get; set; }
    }
}
