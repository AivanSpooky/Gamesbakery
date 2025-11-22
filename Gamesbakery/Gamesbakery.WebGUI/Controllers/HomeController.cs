using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public HomeController(ICategoryService categoryService, IConfiguration configuration)
            : base(Log.ForContext<HomeController>(), configuration)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                LogInformation("User accessed home page");
                var categories = await _categoryService.GetAllCategoriesAsync();
                var model = new HomeIndexViewModel
                {
                    Role = User.GetRole().ToString(),
                    Categories = categories.Select(c => new CategoryResponseDTO
                    {
                        Id = c.Id,
                        GenreName = c.GenreName,
                        Description = c.Description
                    })
                };
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error accessing home page");
                ViewBag.ErrorMessage = "Ошибка загрузки главной страницы.";
                return View(new HomeIndexViewModel());
            }
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}