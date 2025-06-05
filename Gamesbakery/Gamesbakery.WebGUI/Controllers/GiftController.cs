using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gamesbakery.WebGUI.Controllers
{
    [Authorize]
    public class GiftController : Controller
    {
        private readonly IGiftService _giftService;
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;

        public GiftController(IGiftService giftService, IAuthenticationService authService, IUserService userService)
        {
            _giftService = giftService ?? throw new ArgumentNullException(nameof(giftService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetByUsernameAsync(username, _authService.GetCurrentRole());
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var role = _authService.GetCurrentRole();
            var sentGifts = await _giftService.GetGiftsBySenderAsync(user.Id, role);
            var receivedGifts = await _giftService.GetGiftsByRecipientAsync(user.Id, role);

            var viewModel = new GiftIndexViewModel
            {
                SentGifts = sentGifts,
                ReceivedGifts = receivedGifts
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetByUsernameAsync(username, _authService.GetCurrentRole());
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var role = _authService.GetCurrentRole();
            var users = await _userService.GetAllUsersExceptAsync(user.Id);
            var orderItems = await _giftService.GetAvailableOrderItemsAsync(user.Id, role);

            ViewBag.Recipients = new SelectList(users, "Id", "Username");
            ViewBag.OrderItems = new SelectList(orderItems, "Id", "GameTitle");

            return View(new GiftCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiftCreateViewModel model)
        {
            var username = User.Identity.Name;
            var user = await _userService.GetByUsernameAsync(username, _authService.GetCurrentRole());
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var role = _authService.GetCurrentRole();

            if (!ModelState.IsValid)
            {
                var users = await _userService.GetAllUsersExceptAsync(user.Id);
                var orderItems = await _giftService.GetAvailableOrderItemsAsync(user.Id, role);
                ViewBag.Recipients = new SelectList(users, "Id", "Username");
                ViewBag.OrderItems = new SelectList(orderItems, "Id", "GameTitle");
                return View(model);
            }

            try
            {
                await _giftService.CreateGiftAsync(user.Id, model.RecipientId, model.OrderItemId, role);
                TempData["Success"] = "Gift sent successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending gift: {ex.Message}";
                var users = await _userService.GetAllUsersExceptAsync(user.Id);
                var orderItems = await _giftService.GetAvailableOrderItemsAsync(user.Id, role);
                ViewBag.Recipients = new SelectList(users, "Id", "Username");
                ViewBag.OrderItems = new SelectList(orderItems, "Id", "GameTitle");
                return View(model);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var role = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            if (currentUserId == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var sentGift = await _giftService.GetGiftsBySenderAsync(currentUserId.Value, role)
                    .ContinueWith(t => t.Result.FirstOrDefault(g => g.Id == id));
                if (sentGift != null)
                {
                    var gift = await _giftService.GetGiftByIdAsync(id, role, GiftSource.Sent);
                    return View(gift);
                }

                // Try UserReceivedGifts (user is recipient)
                var receivedGift = await _giftService.GetGiftsByRecipientAsync(currentUserId.Value, role)
                    .ContinueWith(t => t.Result.FirstOrDefault(g => g.Id == id));
                if (receivedGift != null)
                {
                    var gift = await _giftService.GetGiftByIdAsync(id, role, GiftSource.Received);
                    return View(gift);
                }

                // Admins can access any gift
                if (role == UserRole.Admin)
                {
                    var gift = await _giftService.GetGiftByIdAsync(id, role, GiftSource.Sent); // Default to Sent for admins
                    return View(gift);
                }

                TempData["Error"] = $"Gift with ID {id} not found or you do not have access.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving gift: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _giftService.DeleteGiftAsync(id, _authService.GetCurrentRole());
                TempData["Success"] = "Gift deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting gift: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}