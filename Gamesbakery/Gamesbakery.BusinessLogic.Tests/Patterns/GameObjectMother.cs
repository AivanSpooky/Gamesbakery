using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Tests.Patterns
{
    public static class GameObjectMother
    {
        public static Game ValidGame(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
            return new Game(id, categoryId, title, price, releaseDate, description, isForSale, originalPublisher);
        }

        public static Game InvalidPriceGame(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            return new Game(id, categoryId, title, 0m, releaseDate, description, isForSale, originalPublisher);
        }

        public static Game NotForSaleGame(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            return new Game(id, categoryId, title, price, releaseDate, description, false, originalPublisher);
        }

        public static Game CreateCustomGame(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string description, bool isForSale, string originalPublisher)
        {
            if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
            return new Game(id, categoryId, title, price, releaseDate, description, isForSale, originalPublisher);
        }
    }
}
