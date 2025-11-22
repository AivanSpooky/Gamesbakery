using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGames()
        {
            try
            {
                var games = await _gameService.GetAllGamesAsync();
                return Ok(new { games });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get games" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGame(Guid id)
        {
            try
            {
                var role = User.GetRole();
                var game = await _gameService.GetGameByIdAsync(id, role);
                return Ok(game);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "Game not found" });
            }
        }
    }
}