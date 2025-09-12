using Gamesbakery.BusinessLogic;
using Gamesbakery.BusinessLogic.Schedulers;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Gamesbakery.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.ConsoleUI
{
    public class ConsoleUI : IConsoleUI
    {
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;
        private readonly IOrderStatusScheduler _orderStatusScheduler;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISellerService _sellerService;
        private readonly GamesbakeryDbContext _dbContext;
        private readonly Dictionary<string, (Func<Task> Action, string Description, UserRole[] AllowedRoles)> _menuActions;
        private readonly UserRole _currentUserRole;
        private readonly Guid? _currentUserId;
        private readonly Guid? _currentSellerId;
        private bool _isDatabaseConnected;
        private CancellationTokenSource _connectionCheckCts;

        public ConsoleUI(
            IUserService userService,
            IGameService gameService,
            IOrderService orderService,
            IReviewService reviewService,
            IOrderStatusScheduler orderStatusScheduler,
            IAuthenticationService authenticationService,
            ISellerService sellerService,
            GamesbakeryDbContext dbContext,
            string username,
            string password)
        {
            _userService = userService;
            _gameService = gameService;
            _orderService = orderService;
            _reviewService = reviewService;
            _orderStatusScheduler = orderStatusScheduler;
            _authenticationService = authenticationService;
            _sellerService = sellerService;
            _dbContext = dbContext;

            // Аутентификация с использованием переданных username и password
            var (role, userId, sellerId) = AuthenticateAsync(username, password).GetAwaiter().GetResult();
            _currentUserRole = role;
            _currentUserId = userId;
            _currentSellerId = sellerId;

            Console.WriteLine($"Logged in as: {username} (Role: {_currentUserRole})");

            _menuActions = new Dictionary<string, (Func<Task> Action, string Description, UserRole[] AllowedRoles)>
            {
                // UserService
                { "1", (RegisterAccountAsync, "Register Account", new[] { UserRole.Guest, UserRole.Admin }) },
                { "2", (GetUserByIdAsync, "Get User by ID", new[] { UserRole.User, UserRole.Admin }) },
                { "3", (GetUserByEmailAsync, "Get User by Email", new[] { UserRole.User, UserRole.Admin }) },
                { "4", (UpdateUserBalanceAsync, "Update User Balance", new[] { UserRole.User, UserRole.Admin }) },
                { "5", (BlockUserAsync, "Block User", new[] { UserRole.Admin }) },
                { "6", (UnblockUserAsync, "Unblock User", new[] { UserRole.Admin }) },

                // GameService
                { "7", (AddGameAsync, "Add Game", new[] { UserRole.Seller, UserRole.Admin }) },
                { "8", (GetGameByIdAsync, "Get Game by ID", new[] { UserRole.Guest, UserRole.User, UserRole.Seller, UserRole.Admin }) },
                { "9", (GetAllGamesAsync, "Get All Games", new[] { UserRole.Guest, UserRole.User, UserRole.Seller, UserRole.Admin }) },
                { "10", (SetGameForSaleAsync, "Set Game For Sale", new[] { UserRole.Seller, UserRole.Admin }) },

                // OrderService
                { "11", (CreateOrderAsync, "Create Order", new[] { UserRole.User }) },
                { "12", (GetOrderByIdAsync, "Get Order by ID", new[] { UserRole.User, UserRole.Seller, UserRole.Admin }) },
                { "13", (GetOrdersByUserIdAsync, "Get Orders by User ID", new[] { UserRole.User, UserRole.Admin }) },
                { "14", (SetOrderItemKeyAsync, "Set Order Item Key", new[] { UserRole.Seller, UserRole.Admin }) },
                { "15", (GetOrderItemsBySellerIdAsync, "Get Order Items by Seller ID", new[] { UserRole.Seller, UserRole.Admin }) },

                // ReviewService
                { "16", (AddReviewAsync, "Add Review", new[] { UserRole.User }) },
                { "17", (GetReviewsByGameIdAsync, "Get Reviews by Game ID", new[] { UserRole.Guest, UserRole.User, UserRole.Seller, UserRole.Admin }) },

                // OrderStatusScheduler
                { "18", (UpdateOrderStatusesAsync, "Update Order Statuses", new[] { UserRole.Admin }) }
            };

            _isDatabaseConnected = true; // Изначально считаем, что подключение есть
            _connectionCheckCts = new CancellationTokenSource();
            StartConnectionCheck(); // Запускаем фоновую проверку подключения
        }

        private async Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password)
        {
            try
            {
                return await _authenticationService.AuthenticateAsync(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return (UserRole.Guest, null, null);
            }
        }

        public async Task RunAsync()
        {
            while (true)
            {
                if (!_isDatabaseConnected)
                {
                    Console.WriteLine("Database connection lost. Waiting for reconnection...");
                    Console.WriteLine("Press any key to retry manually or wait for automatic reconnection...");
                    await Task.Delay(1000);
                    Console.ReadKey(true);
                    continue;
                }

                DisplayMainMenu();
                var choice = Console.ReadLine();

                if (choice == "0")
                {
                    _connectionCheckCts.Cancel(); // Останавливаем проверку подключения при выходе
                    break;
                }

                try
                {
                    if (_menuActions.TryGetValue(choice, out var menuAction))
                    {
                        if (!menuAction.AllowedRoles.Contains(_currentUserRole))
                        {
                            Console.WriteLine($"Access denied. This operation is not allowed for role: {_currentUserRole}");
                            continue;
                        }

                        await menuAction.Action();
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private void StartConnectionCheck()
        {
            Task.Run(async () =>
            {
                while (!_connectionCheckCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Проверяем подключение к базе данных
                        bool canConnect = await CheckDatabaseConnectionAsync();
                        if (canConnect && !_isDatabaseConnected)
                        {
                            _isDatabaseConnected = true;
                            Console.WriteLine("\nDatabase connection restored. You can now continue using the application.");
                        }
                        else if (!canConnect && _isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;
                            Console.WriteLine("\nDatabase connection lost. Operations are disabled until the connection is restored.");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_isDatabaseConnected)
                        {
                            _isDatabaseConnected = false;
                            Console.WriteLine($"\nDatabase connection lost: {ex.Message}. Operations are disabled until the connection is restored.");
                        }
                    }

                    // Ждём 15 секунд перед следующей проверкой
                    await Task.Delay(TimeSpan.FromSeconds(15), _connectionCheckCts.Token);
                }
            }, _connectionCheckCts.Token);
        }

        private async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DisplayMainMenu()
        {
            Console.WriteLine($"role = {(int)_authenticationService.GetCurrentRole()}");
            Console.WriteLine($"=== Gamesbakery Console UI (Role: {_currentUserRole}) ===");
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Database connection lost. Please wait for reconnection...");
                return;
            }

            foreach (var action in _menuActions.OrderBy(a => int.Parse(a.Key)))
            {
                if (action.Value.AllowedRoles.Contains(_currentUserRole))
                {
                    Console.WriteLine($"{action.Key}. {action.Value.Description}");
                }
            }
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");
        }

        // UserService Methods
        private async Task RegisterAccountAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Register Account ===");
            Console.WriteLine("Select account type:");
            Console.WriteLine("1. User");
            Console.WriteLine("2. Seller");
            Console.Write("Enter your choice (1 or 2): ");
            var accountType = Console.ReadLine();

            if (accountType == "1")
            {
                Console.WriteLine("\n=== Register User ===");
                Console.Write("Username: ");
                var username = Console.ReadLine();
                Console.Write("Email: ");
                var email = Console.ReadLine();
                Console.Write("Password: ");
                var password = Console.ReadLine();
                Console.Write("Country: ");
                var country = Console.ReadLine();

                var user = await _userService.RegisterUserAsync(username, email, password, country);
                Console.WriteLine("User registered successfully:");
                DisplayUserProfile(user);
            }
            else if (accountType == "2")
            {
                Console.WriteLine("\n=== Register Seller ===");
                Console.Write("Seller Name: ");
                var sellerName = Console.ReadLine();
                Console.Write("Password: ");
                var password = Console.ReadLine();

                var seller = await _sellerService.RegisterSellerAsync(sellerName, password);
                Console.WriteLine("Seller registered successfully:");
                DisplaySellerProfile(seller);
            }
            else
            {
                Console.WriteLine("Invalid choice. Please select 1 (User) or 2 (Seller).");
            }
        }

        private async Task GetUserByIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get User by ID ===");
            Console.Write("Enter User ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var user = await _userService.GetUserByIdAsync(userId);
            Console.WriteLine("User found:");
            DisplayUserProfile(user);
        }

        private async Task GetUserByEmailAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get User by Email ===");
            Console.Write("Enter Email: ");
            var email = Console.ReadLine();

            var user = await _userService.GetUserByEmailAsync(email);
            Console.WriteLine("User found:");
            DisplayUserProfile(user);
        }

        private async Task UpdateUserBalanceAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Update User Balance ===");
            Console.Write("Enter User ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            Console.Write("Enter New Balance: ");
            if (!decimal.TryParse(Console.ReadLine(), out var newBalance))
            {
                Console.WriteLine("Invalid balance format.");
                return;
            }

            var user = await _userService.UpdateBalanceAsync(userId, newBalance);
            Console.WriteLine("Balance updated successfully:");
            DisplayUserProfile(user);
        }

        private async Task BlockUserAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Block User ===");
            Console.Write("Enter User ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var user = await _userService.BlockUserAsync(userId);
            Console.WriteLine("User blocked successfully:");
            DisplayUserProfile(user);
        }

        private async Task UnblockUserAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Unblock User ===");
            Console.Write("Enter User ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var user = await _userService.UnblockUserAsync(userId);
            Console.WriteLine("User unblocked successfully:");
            DisplayUserProfile(user);
        }

        // GameService Methods
        private async Task AddGameAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Add Game ===");
            Console.Write("Category ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var categoryId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            Console.Write("Title: ");
            var title = Console.ReadLine();
            Console.Write("Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price))
            {
                Console.WriteLine("Invalid price format.");
                return;
            }

            Console.Write("Release Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var releaseDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.Write("Description: ");
            var description = Console.ReadLine();
            Console.Write("Original Publisher: ");
            var originalPublisher = Console.ReadLine();

            var game = await _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher);
            Console.WriteLine("Game added successfully:");
            DisplayGameDetails(game);
        }

        private async Task GetGameByIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get Game by ID ===");
            Console.Write("Enter Game ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var gameId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var game = await _gameService.GetGameByIdAsync(gameId);
            Console.WriteLine("Game found:");
            DisplayGameDetails(game);
        }

        private async Task GetAllGamesAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get All Games ===");
            var games = await _gameService.GetAllGamesAsync();
            if (!games.Any())
            {
                Console.WriteLine("No games found.");
                return;
            }

            Console.WriteLine("Games:");
            foreach (var game in games)
            {
                DisplayGameListItem(game);
            }
        }

        private async Task SetGameForSaleAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Set Game For Sale ===");
            Console.Write("Enter Game ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var gameId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            Console.Write("Is For Sale (true/false): ");
            if (!bool.TryParse(Console.ReadLine(), out var isForSale))
            {
                Console.WriteLine("Invalid boolean format.");
                return;
            }

            var game = await _gameService.SetGameForSaleAsync(gameId, isForSale);
            Console.WriteLine("Game updated successfully:");
            DisplayGameDetails(game);
        }

        // OrderService Methods
        private async Task CreateOrderAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            if (_currentUserId == null)
            {
                Console.WriteLine("You must be logged in as a User to create an order.");
                return;
            }

            Console.WriteLine("\n=== Create Order ===");
            Console.Write("Enter Game IDs (Guids, separated by commas): ");
            var gameIdsInput = Console.ReadLine();
            var gameIds = new List<Guid>();
            foreach (var id in gameIdsInput.Split(','))
            {
                if (Guid.TryParse(id.Trim(), out var gameId))
                {
                    gameIds.Add(gameId);
                }
                else
                {
                    Console.WriteLine($"Invalid Guid format for ID: {id}");
                    return;
                }
            }

            var order = await _orderService.CreateOrderAsync(_currentUserId.Value, gameIds);
            Console.WriteLine("Order created successfully:");
            DisplayOrder(order);
        }

        private async Task GetOrderByIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get Order by ID ===");
            Console.Write("Enter Order ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var orderId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            Console.WriteLine("Order found:");
            //DisplayOrder(order);
        }

        private async Task GetOrdersByUserIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            if (_currentUserId == null)
            {
                Console.WriteLine("You must be logged in as a User to view your orders.");
                return;
            }

            Console.WriteLine("\n=== Get Orders by User ID ===");
            Console.Write("Enter User ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            if (!orders.Any())
            {
                Console.WriteLine("No orders found for this user.");
                return;
            }

            Console.WriteLine("Orders:");
            foreach (var order in orders)
            {
                DisplayOrder(order);
            }
        }

        private async Task SetOrderItemKeyAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            if (_currentSellerId == null)
            {
                Console.WriteLine("You must be logged in as a Seller to set an order item key.");
                return;
            }

            Console.WriteLine("\n=== Set Order Item Key ===");
            Console.Write("Enter Order Item ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var orderItemId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            Console.Write("Enter Key: ");
            var key = Console.ReadLine();

            await _orderService.SetOrderItemKeyAsync(orderItemId, key, _currentSellerId.Value);
            Console.WriteLine("Order item key set successfully.");
        }

        private async Task GetOrderItemsBySellerIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            if (_currentSellerId == null)
            {
                Console.WriteLine("You must be logged in as a Seller to view your order items.");
                return;
            }

            Console.WriteLine("\n=== Get Order Items by Seller ID ===");
            var orderItems = await _orderService.GetOrderItemsBySellerIdAsync(_currentSellerId.Value);
            if (!orderItems.Any())
            {
                Console.WriteLine("No order items found for this seller.");
                return;
            }

            Console.WriteLine("Order Items:");
            foreach (var item in orderItems)
            {
                DisplayOrderItem(item);
            }
        }

        // ReviewService Methods
        private async Task AddReviewAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            if (_currentUserId == null)
            {
                Console.WriteLine("You must be logged in as a User to add a review.");
                return;
            }

            Console.WriteLine("\n=== Add Review ===");
            Console.Write("Enter Game ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var gameId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            Console.Write("Enter Review Text: ");
            var text = Console.ReadLine();
            Console.Write("Enter Rating (1-5): ");
            if (!int.TryParse(Console.ReadLine(), out var rating))
            {
                Console.WriteLine("Invalid rating format.");
                return;
            }

            var review = await _reviewService.AddReviewAsync(_currentUserId.Value, gameId, text, rating);
            Console.WriteLine("Review added successfully:");
            DisplayReview(review);
        }

        private async Task GetReviewsByGameIdAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Get Reviews by Game ID ===");
            Console.Write("Enter Game ID (Guid): ");
            if (!Guid.TryParse(Console.ReadLine(), out var gameId))
            {
                Console.WriteLine("Invalid Guid format.");
                return;
            }

            var reviews = await _reviewService.GetReviewsByGameIdAsync(gameId);
            if (!reviews.Any())
            {
                Console.WriteLine("No reviews found for this game.");
                return;
            }

            Console.WriteLine("Reviews:");
            foreach (var review in reviews)
            {
                DisplayReview(review);
            }
        }

        // OrderStatusScheduler Method
        private async Task UpdateOrderStatusesAsync()
        {
            if (!_isDatabaseConnected)
            {
                Console.WriteLine("Cannot perform this operation: Database connection is lost.");
                return;
            }

            Console.WriteLine("\n=== Update Order Statuses ===");
            await _orderStatusScheduler.UpdateOrderStatusesAsync();
            Console.WriteLine("Order statuses updated successfully.");
        }

        // Display Methods
        private void DisplayUserProfile(UserProfileDTO user)
        {
            Console.WriteLine($"ID: {user.Id}");
            Console.WriteLine($"Username: {user.Username}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Registration Date: {user.RegistrationDate}");
            Console.WriteLine($"Country: {user.Country}");
            Console.WriteLine($"Is Blocked: {user.IsBlocked}");
            Console.WriteLine($"Balance: {user.Balance}");
        }

        private void DisplaySellerProfile(Seller seller)
        {
            Console.WriteLine($"ID: {seller.Id}");
            Console.WriteLine($"Seller Name: {seller.SellerName}");
            Console.WriteLine($"Registration Date: {seller.RegistrationDate}");
            Console.WriteLine($"Average Rating: {seller.AvgRating}");
        }

        private void DisplayGameDetails(GameDetailsDTO game)
        {
            Console.WriteLine($"ID: {game.Id}");
            Console.WriteLine($"Category ID: {game.CategoryId}");
            Console.WriteLine($"Title: {game.Title}");
            Console.WriteLine($"Price: {game.Price}");
            Console.WriteLine($"Release Date: {game.ReleaseDate}");
            Console.WriteLine($"Description: {game.Description}");
            Console.WriteLine($"Is For Sale: {game.IsForSale}");
            Console.WriteLine($"Original Publisher: {game.OriginalPublisher}");
        }

        private void DisplayGameListItem(GameListDTO game)
        {
            Console.WriteLine($"ID: {game.Id}, Title: {game.Title}, Price: {game.Price}, Is For Sale: {game.IsForSale}");
        }

        private void DisplayOrder(OrderListDTO order)
        {
            Console.WriteLine($"ID: {order.Id}");
            Console.WriteLine($"Order Date: {order.OrderDate}");
            Console.WriteLine($"Price: {order.Price}");
            Console.WriteLine($"IsCompleted: {order.IsCompleted}");
            Console.WriteLine($"IsOverdue: {order.IsOverdue}");
        }

        private void DisplayOrderItem(OrderItemDTO item)
        {
            Console.WriteLine($"ID: {item.Id}, Order ID: {item.OrderId}, Game ID: {item.GameId}, Seller ID: {item.SellerId}, Key: {item.Key ?? "Not Set"}");
        }

        private void DisplayReview(ReviewDTO review)
        {
            Console.WriteLine($"ID: {review.Id}");
            Console.WriteLine($"User ID: {review.UserId}");
            Console.WriteLine($"Game ID: {review.GameId}");
            Console.WriteLine($"Text: {review.Text}");
            Console.WriteLine($"Rating: {review.Rating}");
            Console.WriteLine($"Creation Date: {review.CreationDate}");
        }
    }
}