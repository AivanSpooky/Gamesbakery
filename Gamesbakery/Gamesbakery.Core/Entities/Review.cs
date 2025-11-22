using System;

namespace Gamesbakery.Core.Entities
{
    public class Review
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public string Text { get; private set; }
        public int Rating { get; private set; }
        public DateTime CreationDate { get; private set; }

        public User User { get; private set; } // Added
        public Game Game { get; private set; } // Added

        public Review()
        {
        }

        public Review(Guid id, Guid userId, Guid gameId, string text, int rating, DateTime creationDate)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (gameId == Guid.Empty)
                throw new ArgumentException("GameId cannot be empty.", nameof(gameId));
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

        public void Update(string text, int rating)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
            Text = text;
            Rating = rating;
        }
    }
}