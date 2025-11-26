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
    [ApiController]
    [Route("api/v1/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService gameService;

        public GamesController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGames()
        {
            try
            {
                var games = await this.gameService.GetAllGamesAsync();
                return this.Ok(new { games });
            }
            catch (Exception)
            {
                return this.StatusCode(500, new { error = "Failed to get games" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGame(Guid id)
        {
            try
            {
                var role = this.User.GetRole();
                var game = await this.gameService.GetGameByIdAsync(id, role);
                return this.Ok(game);
            }
            catch (Exception)
            {
                return this.NotFound(new { error = "Game not found" });
            }
        }
    }
}
