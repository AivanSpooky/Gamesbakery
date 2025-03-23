namespace Gamesbakery.Core.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
        public DateTime CreationDate { get; set; }
    }
}