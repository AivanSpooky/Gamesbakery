using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V1
{
    public class CreateOrderV1DTO
    {
        public List<Guid> GameIds { get; set; } = new List<Guid>();
    }
}
