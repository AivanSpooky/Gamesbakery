using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.Repositories;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V2
{
    /// <summary>
    /// Controller for managing game categories.
    /// </summary>
    [ApiController]
    [Route("api/v2/categories")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryRepository categoryRepository, ICategoryService categoryService)
        {
            this.categoryRepository = categoryRepository;
            this.categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves a paginated list of categories.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of categories per page (default is 10).</param>
        /// <returns>A paginated list of categories.</returns>
        /// <response code="200">Returns the paginated list of categories.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<CategoryResponseDTO>))]
        public async Task<ActionResult> GetCategories(int page = 1, int limit = 10)
        {
            var role = this.User.GetRole();
            var categories = await this.categoryRepository.GetAllAsync(role);
            var totalCount = await this.categoryRepository.GetCountAsync(role);
            var pagedCategories = categories.Skip((page - 1) * limit).Take(limit).Select(c => new CategoryResponseDTO
            {
                Id = c.Id,
                GenreName = c.GenreName,
                Description = c.Description,
            }).ToList();
            return this.Ok(new PaginatedResponse<CategoryResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedCategories,
                NextPage = pagedCategories.Count == limit ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit,
            });
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="dto">The category creation details.</param>
        /// <returns>The created category.</returns>
        /// <response code="201">Category successfully created.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<CategoryResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateCategory([FromBody] CategoryCreateDTO dto)
        {
            var category = await this.categoryService.AddCategoryAsync(dto.GenreName, dto.Description, this.User.GetRole());
            return this.CreatedAtAction(nameof(this.GetCategory), new { id = category.Id }, new SingleResponse<CategoryResponseDTO>
            {
                Item = new CategoryResponseDTO
                {
                    Id = category.Id,
                    GenreName = category.GenreName,
                    Description = category.Description,
                },
                Message = "Category created successfully",
            });
        }

        /// <summary>
        /// Retrieves a specific category by ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>The category details.</returns>
        /// <response code="200">Returns the category details.</response>
        /// <response code="404">If the category is not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<CategoryResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetCategory(Guid id)
        {
            var category = await this.categoryService.GetCategoryByIdAsync(id, this.User.GetRole());
            if (category == null) return this.NotFound();
            return this.Ok(new SingleResponse<CategoryResponseDTO>
            {
                Item = new CategoryResponseDTO
                {
                    Id = category.Id,
                    GenreName = category.GenreName,
                    Description = category.Description,
                },
                Message = "Category retrieved successfully",
            });
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="dto">The updated category details.</param>
        /// <returns>The updated category.</returns>
        /// <response code="200">Category successfully updated.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the category is not found.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<CategoryResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDTO dto)
        {
            var role = this.User.GetRole();
            var category = await this.categoryRepository.GetByIdAsync(id, role);
            if (category == null) return this.NotFound();
            category.GenreName = dto.GenreName;
            category.Description = dto.Description;
            var updated = await this.categoryRepository.UpdateAsync(category, role);
            return this.Ok(new SingleResponse<CategoryResponseDTO>
            {
                Item = new CategoryResponseDTO
                {
                    Id = updated.Id,
                    GenreName = updated.GenreName,
                    Description = updated.Description,
                },
                Message = "Category updated successfully",
            });
        }
    }
}
