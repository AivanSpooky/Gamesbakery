namespace Gamesbakery.Core.DTOs
{
    public class ReviewDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid GameId { get; set; }

        public string Text { get; set; }

        public int Rating { get; set; }

        public DateTime CreationDate { get; set; }

        public string Username { get; set; } // Added
    }
}
