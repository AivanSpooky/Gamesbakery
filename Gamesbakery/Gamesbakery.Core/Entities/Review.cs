namespace Gamesbakery.Core.Entities
{
    public class Review
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int GameId { get; private set; }
        public string Text { get; private set; }
        public int Rating { get; private set; }
        public DateTime CreationDate { get; private set; }

        public Review(int id, int userId, int gameId, string text, int rating, DateTime creationDate)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive.", nameof(userId));
            if (gameId <= 0)
                throw new ArgumentException("GameId must be positive.", nameof(gameId));
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

            Id = id;
            UserId = userId;
            GameId = gameId;
            Text = text;
            Rating = rating;
            CreationDate = creationDate;
        }
    }
}