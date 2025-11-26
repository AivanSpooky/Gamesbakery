using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [AllowAnonymous]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService, IConfiguration configuration)
            : base(Log.ForContext<CategoryController>(), configuration)
        {
            this.categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await this.categoryService.GetAllCategoriesAsync();
                var categoriesResponse = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description,
                }).ToList();
                return this.View(categoriesResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading categories");
                this.ViewBag.ErrorMessage = "Ошибка загрузки категорий.";
                return this.View(new List<CategoryResponseDTO>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDTO model)
        {
            var role = this.User.GetRole();
            try
            {
                if (!this.ModelState.IsValid)
                    return this.View(model);
                var category = await this.categoryService.AddCategoryAsync(model.GenreName, model.Description, role);
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating category with GenreName={GenreName}", model.GenreName);
                this.ModelState.AddModelError(string.Empty, $"Ошибка при создании категории: {ex.Message}");
                return this.View(model);
            }
        }
    }
}
