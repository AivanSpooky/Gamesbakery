namespace Gamesbakery.Core.DTOs.CartDTO
{
    public class CarTDTO
    {
        public Guid CartId { get; set; }

        public Guid UserId { get; set; }

        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    }
}
