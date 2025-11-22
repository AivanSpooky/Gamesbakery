using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.Repositories;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for managing sellers.
    /// </summary>
    [ApiController]
    [Route("api/v2/sellers")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class SellersController : ControllerBase
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerService _sellerService;
        private readonly IOrderItemRepository _orderItemRepository;

        public SellersController(ISellerRepository sellerRepository, ISellerService sellerService, IOrderItemRepository orderItemRepository)
        {
            _sellerRepository = sellerRepository;
            _sellerService = sellerService;
            _orderItemRepository = orderItemRepository;
        }

        /// <summary>
        /// Retrieves a paginated list of sellers.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of sellers per page (default is 10).</param>
        /// <returns>A paginated list of sellers.</returns>
        /// <response code="200">Returns the paginated list of sellers.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<SellerResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetSellers(int page = 1, int limit = 1000, bool getAll = false)
        {
            var role = User.GetRole();
            var sellers = await _sellerRepository.GetAllAsync(role);
            var totalCount = await _sellerRepository.GetCountAsync(role);

            if (getAll)
            {
                limit = totalCount; // Fetch all
                page = 1;
            }

            var pagedSellers = sellers.Skip((page - 1) * limit).Take(limit).Select(s => new SellerResponseDTO
            {
                Id = s.Id,
                SellerName = s.SellerName,
                RegistrationDate = s.RegistrationDate,
                AvgRating = s.AvgRating
            }).ToList();

            return Ok(new PaginatedResponse<SellerResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedSellers,
                NextPage = (page * limit < totalCount) ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit
            });
        }

        /// <summary>
        /// Retrieves a specific seller by ID.
        /// </summary>
        /// <param name="id">The ID of the seller.</param>
        /// <returns>The seller details.</returns>
        /// <response code="200">Returns the seller details.</response>
        /// <response code="404">If the seller is not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<SellerResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetSeller(Guid id)
        {
            var role = User.GetRole();
            var seller = await _sellerRepository.GetByIdAsync(id, role);
            if (seller == null)
            {
                return NotFound(new { error = "Seller not found" });
            }
            return Ok(new SingleResponse<SellerResponseDTO>
            {
                Item = new SellerResponseDTO
                {
                    Id = seller.Id,
                    SellerName = seller.SellerName,
                    RegistrationDate = seller.RegistrationDate,
                    AvgRating = seller.AvgRating
                },
                Message = "Seller retrieved successfully"
            });
        }

        /// <summary>
        /// Registers a new seller.
        /// </summary>
        /// <param name="dto">The seller registration details.</param>
        /// <returns>The created seller.</returns>
        /// <response code="201">Seller successfully registered.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<SellerResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> RegisterSeller([FromBody] SellerRegisterDTO dto)
        {
            var role = User.GetRole();
            try
            {
                var seller = await _sellerService.RegisterSellerAsync(dto.SellerName, dto.Password, role);
                return CreatedAtAction(nameof(GetSellers), new { id = seller.Id }, new SingleResponse<SellerResponseDTO>
                {
                    Item = new SellerResponseDTO
                    {
                        Id = seller.Id,
                        SellerName = seller.SellerName,
                        RegistrationDate = seller.RegistrationDate,
                        AvgRating = seller.AvgRating
                    },
                    Message = "Seller registered successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to register seller", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new key for a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="dto">The key creation details.</param>
        /// <returns>The created order item.</returns>
        /// <response code="201">Key successfully created.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost("{sellerId}/order-items")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<OrderItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateKey(Guid sellerId, [FromBody] CreateKeyDTO dto)
        {
            var role = User.GetRole();
            var currentSellerId = User.GetSellerId();
            if (sellerId != currentSellerId) return Forbid();
            var orderItem = await _sellerService.CreateKeyAsync(dto.GameId, dto.Key, currentSellerId, role);
            return CreatedAtAction(nameof(OrderItemsController.GetOrderItem), "OrderItems", new { id = orderItem.Id }, new SingleResponse<OrderItemResponseDTO>
            {
                Item = new OrderItemResponseDTO
                {
                    Id = orderItem.Id,
                    GameId = orderItem.GameId,
                    GameTitle = orderItem.GameTitle,
                    SellerId = orderItem.SellerId,
                    SellerName = orderItem.SellerName
                },
                Message = "Key created successfully"
            });
        }
    }
}