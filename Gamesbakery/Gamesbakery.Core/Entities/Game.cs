namespace Gamesbakery.Core.Entities
{
    public class Game
    {
        public Guid Id { get; private set; }
        public Guid CategoryId { get; private set; }
        public void SetCategoryId(Guid categoryId) => CategoryId = categoryId;
        public string Title { get; private set; }
        public void SetTitle(string title) => Title = title;
        public decimal Price { get; private set; }
        public void SetPrice(decimal price) => Price = price;
        public DateTime ReleaseDate { get; private set; }
        public void SetReleaseDate(DateTime rd) => ReleaseDate = rd;
        public string Description { get; private set; }
        public void SetDescription(string desc) => Description = desc;
        public bool IsForSale { get; private set; }
        public void SetForSale(bool isForSale) => IsForSale = isForSale;
        public string OriginalPublisher { get; private set; }
        public void SetOriginalPublisher(string op) => OriginalPublisher = op;

        public Game()
        {
        }

        public Game(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));
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

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
            Price = newPrice;
        }

        public void UpdateTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("newTitle cannot be empty.", nameof(newTitle));
            Title = newTitle;
        }
    }
}