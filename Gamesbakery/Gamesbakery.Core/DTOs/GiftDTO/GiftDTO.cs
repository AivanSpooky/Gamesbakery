namespace Gamesbakery.Core.DTOs.GiftDTO
{
    public class GiftDTO
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public Guid GameId { get; set; }
        public string GameTitle { get; set; }
        public string Key { get; set; }
        public DateTime GiftDate { get; set; }
    }
}
