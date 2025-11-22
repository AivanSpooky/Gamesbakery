using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for managing games.
    /// </summary>
    [ApiController]
    [Route("api/v2/games")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <summary>
        /// Retrieves a paginated list of games with optional filtering.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of games per page (default is 10).</param>
        /// <param name="genre">The genre to filter by (optional).</param>
        /// <param name="minPrice">The minimum price to filter by (optional).</param>
        /// <param name="maxPrice">The maximum price to filter by (optional).</param>
        /// <returns>A paginated list of games.</returns>
        /// <response code="200">Returns the paginated list of games.</response>
        /// <response code="500">If an error occurs while retrieving games.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<GameListResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetGames(int page = 1, int limit = 10, string? genre = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            try
            {
                var role = User.GetRole();
                var games = await _gameService.GetFilteredGamesAsync(genre, minPrice, maxPrice, role);
                var totalCount = await _gameService.GetFilteredGamesCountAsync(genre, minPrice, maxPrice, role);
                var pagedGames = games.Skip((page - 1) * limit).Take(limit).Select(g => new GameListResponseDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Price = g.Price,
                    IsForSale = g.IsForSale
                }).ToList();
                return Ok(new PaginatedResponse<GameListResponseDTO>
                {
                    TotalCount = totalCount,
                    Items = pagedGames,
                    NextPage = pagedGames.Count == limit ? page + 1 : null,
                    PreviousPage = page > 1 ? page - 1 : null,
                    CurrentPage = page,
                    PageSize = limit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve games", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="dto">The game creation details.</param>
        /// <returns>The created game.</returns>
        /// <response code="201">Game successfully created.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the category is not found.</response>
        /// <response code="400">If the request data is invalid.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<GameDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateGame([FromBody] GameCreateDTO dto)
        {
            try
            {
                var role = User.GetRole();
                var gameDetails = await _gameService.AddGameAsync(
                    dto.CategoryId,
                    dto.Title,
                    dto.Price,
                    dto.ReleaseDate,
                    dto.Description ?? string.Empty,
                    dto.OriginalPublisher ?? string.Empty,
                    role,
                    dto.IsForSale
                );
                return CreatedAtAction(
                    nameof(GetGame),
                    new { id = gameDetails.Id },
                    new SingleResponse<GameDetailsResponseDTO>
                    {
                        Item = new GameDetailsResponseDTO
                        {
                            Id = gameDetails.Id,
                            CategoryId = gameDetails.CategoryId,
                            Title = gameDetails.Title,
                            Price = gameDetails.Price,
                            ReleaseDate = gameDetails.ReleaseDate,
                            Description = gameDetails.Description,
                            IsForSale = gameDetails.IsForSale,
                            OriginalPublisher = gameDetails.OriginalPublisher,
                            AverageRating = gameDetails.AverageRating
                        },
                        Message = "Game created successfully"
                    });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific game by ID.
        /// </summary>
        /// <param name="id">The ID of the game.</param>
        /// <param name="includeOrderItems">Whether to include available order items.</param>
        /// <returns>The game details.</returns>
        /// <response code="200">Returns the game details.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="500">If an error occurs while retrieving the game.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<GameDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetGame(Guid id, [FromQuery] bool includeOrderItems = false)
        {
            try
            {
                var role = User.GetRole();
                var gameDetails = await _gameService.GetGameByIdAsync(id, role, includeOrderItems);
                return Ok(new SingleResponse<GameDetailsResponseDTO>
                {
                    Item = new GameDetailsResponseDTO
                    {
                        Id = gameDetails.Id,
                        CategoryId = gameDetails.CategoryId,
                        Title = gameDetails.Title,
                        Price = gameDetails.Price,
                        ReleaseDate = gameDetails.ReleaseDate,
                        Description = gameDetails.Description,
                        IsForSale = gameDetails.IsForSale,
                        OriginalPublisher = gameDetails.OriginalPublisher,
                        AverageRating = gameDetails.AverageRating
                    },
                    Message = "Game retrieved successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve game", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing game.
        /// </summary>
        /// <param name="id">The ID of the game to update.</param>
        /// <param name="dto">The updated game details.</param>
        /// <returns>The updated game.</returns>
        /// <response code="200">Game successfully updated.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="400">If the request data is invalid.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<GameDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateGame(Guid id, [FromBody] GameUpdateDTO dto)
        {
            try
            {
                var role = User.GetRole();
                var updatedGame = await _gameService.UpdateGameAsync(
                    id,
                    dto.CategoryId,
                    dto.Title,
                    dto.Price,
                    dto.ReleaseDate,
                    dto.Description,
                    dto.OriginalPublisher,
                    dto.IsForSale,
                    role
                );
                return Ok(new SingleResponse<GameDetailsResponseDTO>
                {
                    Item = new GameDetailsResponseDTO
                    {
                        Id = updatedGame.Id,
                        CategoryId = updatedGame.CategoryId,
                        Title = updatedGame.Title,
                        Price = updatedGame.Price,
                        ReleaseDate = updatedGame.ReleaseDate,
                        Description = updatedGame.Description,
                        IsForSale = updatedGame.IsForSale,
                        OriginalPublisher = updatedGame.OriginalPublisher,
                        AverageRating = updatedGame.AverageRating
                    },
                    Message = "Game updated successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Partially updates an existing game.
        /// </summary>
        /// <param name="id">The ID of the game to update.</param>
        /// <param name="updates">The partial update details.</param>
        /// <returns>The updated game.</returns>
        /// <response code="200">Game successfully updated.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="400">If the request data is invalid.</response>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<GameDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PartialUpdateGame(Guid id, [FromBody] Dictionary<string, object> updates)
        {
            try
            {
                var role = User.GetRole();
                var updatedGame = await _gameService.PartialUpdateGameAsync(id, updates, role);
                return Ok(new SingleResponse<GameDetailsResponseDTO>
                {
                    Item = new GameDetailsResponseDTO
                    {
                        Id = updatedGame.Id,
                        CategoryId = updatedGame.CategoryId,
                        Title = updatedGame.Title,
                        Price = updatedGame.Price,
                        ReleaseDate = updatedGame.ReleaseDate,
                        Description = updatedGame.Description,
                        IsForSale = updatedGame.IsForSale,
                        OriginalPublisher = updatedGame.OriginalPublisher,
                        AverageRating = updatedGame.AverageRating
                    },
                    Message = "Game partially updated successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a game.
        /// </summary>
        /// <param name="id">The ID of the game to delete.</param>
        /// <returns>A response indicating the game was deleted.</returns>
        /// <response code="204">Game successfully deleted.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="500">If an error occurs while deleting the game.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGame(Guid id)
        {
            try
            {
                var role = User.GetRole();
                await _gameService.DeleteGameAsync(id, role);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete game", details = ex.Message });
            }
        }

        /// <summary>
        /// Sets the sale status of a game.
        /// </summary>
        /// <param name="id">The ID of the game.</param>
        /// <param name="isForSale">The new sale status.</param>
        /// <returns>The updated game.</returns>
        /// <response code="200">Game sale status updated successfully.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the game is not found.</response>
        [HttpPatch("{id}/for-sale")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<GameDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetGameForSale(Guid id, [FromQuery] bool isForSale)
        {
            try
            {
                var role = User.GetRole();
                var gameDetails = await _gameService.SetGameForSaleAsync(id, isForSale, role);
                return Ok(new SingleResponse<GameDetailsResponseDTO>
                {
                    Item = new GameDetailsResponseDTO
                    {
                        Id = gameDetails.Id,
                        CategoryId = gameDetails.CategoryId,
                        Title = gameDetails.Title,
                        Price = gameDetails.Price,
                        ReleaseDate = gameDetails.ReleaseDate,
                        Description = gameDetails.Description,
                        IsForSale = gameDetails.IsForSale,
                        OriginalPublisher = gameDetails.OriginalPublisher,
                        AverageRating = gameDetails.AverageRating
                    },
                    Message = "Game sale status updated successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update game sale status", details = ex.Message });
            }
        }
    }
}