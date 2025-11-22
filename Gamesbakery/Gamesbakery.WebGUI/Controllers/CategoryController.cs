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
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService, IConfiguration configuration)
            : base(Log.ForContext<CategoryController>(), configuration)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoriesResponse = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description
                }).ToList();
                return View(categoriesResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading categories");
                ViewBag.ErrorMessage = "Ошибка загрузки категорий.";
                return View(new List<CategoryResponseDTO>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDTO model)
        {
            var role = User.GetRole();
            try
            {
                if (!ModelState.IsValid)
                    return View(model);
                var category = await _categoryService.AddCategoryAsync(model.GenreName, model.Description, role);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating category with GenreName={GenreName}", model.GenreName);
                ModelState.AddModelError("", $"Ошибка при создании категории: {ex.Message}");
                return View(model);
            }
        }
    }
}