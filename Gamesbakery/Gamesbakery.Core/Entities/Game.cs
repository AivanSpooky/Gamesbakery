// Game.cs
namespace Gamesbakery.Core.Entities
{
    public class Game
    {
        public int Id { get; private set; }
        public int CategoryId { get; private set; }
        public string Title { get; private set; }
        public decimal Price { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public string Description { get; private set; }
        public bool IsForSale { get; private set; }
        public string OriginalPublisher { get; private set; }

        public Game(int id, int categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            if (categoryId <= 0)
                throw new ArgumentException("CategoryId must be positive.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            if (string.IsNullOrWhiteSpace(originalPublisher))
                throw new ArgumentException("OriginalPublisher cannot be empty.", nameof(originalPublisher));

            Id = id;
            CategoryId = categoryId;
            Title = title;
            Price = price;
            ReleaseDate = releaseDate;
            Description = description;
            IsForSale = isForSale;
            OriginalPublisher = originalPublisher;
        }

        public void SetForSale(bool isForSale) => IsForSale = isForSale;
    }
}