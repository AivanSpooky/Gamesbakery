using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamesbakery.Core.DTOs.UserDTO
{
    public class UserUpdateDTO
    {
        public decimal? Balance { get; set; }

        public string? Country { get; set; }
    }
}
