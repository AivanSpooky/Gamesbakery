using System;

namespace Gamesbakery.Core.DTOs
{
    public class CreateKeyDTO
    {
        public Guid GameId { get; set; }

        public string Key { get; set; }
    }
}
