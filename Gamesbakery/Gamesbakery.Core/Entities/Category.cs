namespace Gamesbakery.Core.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string GenreName { get; private set; }
        public string Description { get; private set; }

        public Category(int id, string genreName, string description)
        {
            if (string.IsNullOrWhiteSpace(genreName) || genreName.Length > 50)
                throw new ArgumentException("GenreName must be between 1 and 50 characters.", nameof(genreName));

            Id = id;
            GenreName = genreName;
            Description = description;
        }
    }
}