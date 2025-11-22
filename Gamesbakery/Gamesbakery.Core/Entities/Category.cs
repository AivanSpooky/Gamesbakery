using System;
using System.Collections.Generic;

namespace Gamesbakery.Core.Entities
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string GenreName { get; private set; }
        public string Description { get; private set; }

        public List<Game> Games { get; private set; } // Added

        public Category()
        {
            Games = new List<Game>();
        }

        public Category(Guid id, string genreName, string description)
        {
            if (string.IsNullOrWhiteSpace(genreName) || genreName.Length > 50)
                throw new ArgumentException("GenreName must be between 1 and 50 characters.", nameof(genreName));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            Id = id;
            GenreName = genreName;
            Description = description;
            Games = new List<Game>();
        }

        public void Update(string genreName, string description)
        {
            if (string.IsNullOrWhiteSpace(genreName) || genreName.Length > 50)
                throw new ArgumentException("GenreName must be between 1 and 50 characters.", nameof(genreName));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            GenreName = genreName;
            Description = description;
        }
    }
}