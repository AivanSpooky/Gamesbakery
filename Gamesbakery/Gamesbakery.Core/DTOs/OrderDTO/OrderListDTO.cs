using System;

namespace Gamesbakery.Core.DTOs.OrderDTO
{
    public class OrderListDTO
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
    }
}