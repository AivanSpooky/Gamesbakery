using Gamesbakery.Core.DTOs.GiftDTO;

namespace Gamesbakery.WebGUI.Models
{
    public class GiftIndexViewModel
    {
        public IEnumerable<GiftDTO> SentGifts { get; set; }
        public IEnumerable<GiftDTO> ReceivedGifts { get; set; }
    }

    public class GiftCreateViewModel
    {
        public Guid RecipientId { get; set; }
        public Guid OrderItemId { get; set; }
    }
}
