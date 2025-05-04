using Gamesbakery.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using Gamesbakery.BusinessLogic.Services;
using System.Threading.Tasks;
using Gamesbakery.WebGUI.Models;

namespace Gamesbakery.WebGUI.Controllers
{
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
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed home page");
                    var model = new HomeIndexViewModel
                    {
                        Role = HttpContext.Session.GetString("Role") ?? "Не указана",
                        Categories = await _categoryService.GetAllCategoriesAsync()
                    };
                    return View(model);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing home page");
                    throw;
                }
            }
        }

        public IActionResult Error()
        {
            using (PushLogContext())
            {
                LogError("User accessed error page");
                return View();
            }
        }
    }
}