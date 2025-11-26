using Bogus;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Text.Json;

namespace Gamesbakery.DataSeeder
{
    public class DataSeeder
    {
        private readonly GamesbakeryDbContext _context;

        public DataSeeder(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task SeedAsync()
        {
            try
            {
                // Очистка базы данных (триггеры автоматически удалят SQL-пользователей)
                await CleanDatabaseAsync();

                // Получаем список игр из Steam API
                var client = new RestClient("https://api.steampowered.com");
                var request = new RestRequest("ISteamApps/GetAppList/v2/", Method.Get);
                var response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                    throw new InvalidOperationException($"Failed to fetch games from Steam API: {response.StatusCode}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var appList = JsonSerializer.Deserialize<SteamAppListResponse>(response.Content, options);
                if (appList?.Applist?.Apps == null)
                    throw new InvalidOperationException("Failed to deserialize Steam API response.");

                // Генерация данных
                var (genreToCategoryId, selectedApps) = await SeedCategoriesAsync(appList.Applist.Apps);
                Console.WriteLine("Categories seeded.");

                var sellerIds = await SeedSellersAsync();
                Console.WriteLine($"Sellers seeded: {sellerIds.Count} sellers.");

                var userIds = await SeedUsersAsync();
                Console.WriteLine($"Users seeded: {userIds.Count} users.");

                var gameIds = await SeedGamesAsync(genreToCategoryId, selectedApps);
                Console.WriteLine($"Games seeded: {gameIds.Count} games.");

                var orderIds = await SeedOrdersAsync(userIds);
                Console.WriteLine($"Orders seeded: {orderIds.Count} orders.");

                await SeedOrderItemsAsync(orderIds, gameIds, sellerIds);
                Console.WriteLine("OrderItems seeded.");

                await SeedReviewsAsync(userIds, gameIds);
                Console.WriteLine("Reviews seeded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                throw new InvalidOperationException("Failed to seed the database.", ex);
            }
        }

        private async Task CleanDatabaseAsync()
        {
            // Очистка таблиц (триггеры автоматически удалят SQL-пользователей)
            _context.Database.ExecuteSqlRaw("DELETE FROM Reviews;");
            _context.Database.ExecuteSqlRaw("DELETE FROM OrderItems;");
            _context.Database.ExecuteSqlRaw("DELETE FROM Orders;");
            _context.Database.ExecuteSqlRaw("DELETE FROM Games;");
            _context.Database.ExecuteSqlRaw("DELETE FROM Users;");
            _context.Database.ExecuteSqlRaw("DELETE FROM Sellers;");
            _context.Database.ExecuteSqlRaw("DELETE FROM Categories;");

            await _context.SaveChangesAsync();
        }

        private async Task<(Dictionary<string, Guid> genreToCategoryId, List<SteamApp> selectedApps)> SeedCategoriesAsync(List<SteamApp> apps)
        {
            var client = new RestClient("https://store.steampowered.com");
            var genreToCategoryId = new Dictionary<string, Guid>();
            var categories = new List<Category>();
            var random = new Random();

            // Выбираем 50 случайных игр
            var selectedApps = apps.OrderBy(x => random.Next()).Take(50).ToList();
            var uniqueGenres = new HashSet<string>();

            // Обрабатываем игры партиями по 10
            for (int i = 0; i < selectedApps.Count; i += 10)
            {
                var batch = selectedApps.Skip(i).Take(10).ToList();
                foreach (var app in batch)
                {
                    var request = new RestRequest("api/appdetails", Method.Get);
                    request.AddParameter("appids", app.Appid);
                    var response = await client.ExecuteAsync(request);

                    if (!response.IsSuccessful)
                    {
                        Console.WriteLine($"Failed to fetch details for appid {app.Appid}: {response.StatusCode}");
                        continue;
                    }

                    var appDetails = JsonSerializer.Deserialize<Dictionary<string, SteamAppDetails>>(response.Content);
                    if (appDetails == null || !appDetails.ContainsKey(app.Appid.ToString()) || !appDetails[app.Appid.ToString()].Success)
                        continue;

                    var genres = appDetails[app.Appid.ToString()].Data.Genres ?? new List<SteamGenre>();
                    foreach (var genre in genres)
                        uniqueGenres.Add(genre.Description);
                }

                await Task.Delay(1000); // Задержка 1 секунда
            }

            // Создаём категории на основе уникальных жанров
            foreach (var genre in uniqueGenres)
            {
                categories.Add(new Category(Guid.NewGuid(),
                    genre,
                    $"Games in the {genre} genre."));
            }

            // Если жанров нет, используем стандартные
            if (!categories.Any())
            {
                var defaultGenres = new[] { "Action", "Adventure", "RPG", "Strategy", "Simulation", "Sports", "Racing", "Puzzle", "Indie", "Casual" };
                categories.AddRange(defaultGenres.Select(genre => new Category(Guid.NewGuid(),
                    genre,
                    $"Games in the {genre} genre.")
                ));
            }

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Обновляем genreToCategoryId
            var categoryIds = await _context.Categories.ToDictionaryAsync(c => c.GenreName, c => c.Id);
            foreach (var genre in categoryIds.Keys)
                genreToCategoryId[genre] = categoryIds[genre];

            Console.WriteLine($"Seeded {categories.Count} categories.");
            return (genreToCategoryId, selectedApps);
        }

        private async Task<List<Guid>> SeedSellersAsync()
        {
            var usedNames = new HashSet<string>();
            var faker = new Faker<Seller>()
                .RuleFor(s => s.Id, f => Guid.NewGuid())
                .RuleFor(s => s.SellerName, f =>
                {
                    string name;
                    do name = f.Company.CompanyName();
                    while (name.Length > 100 || usedNames.Contains(name));
                    usedNames.Add(name);
                    return name;
                    //f.Company.CompanyName().Length > 100 ? f.Company.CompanyName().Substring(0, 100) : f.Company.CompanyName()
                })
                .RuleFor(s => s.RegistrationDate, f => f.Date.Past(5))
                .RuleFor(s => s.AvgRating, f => f.Random.Double(0, 5))
                .RuleFor(s => s.Password, f => f.Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()"));

            var sellers = faker.Generate(1000);
            await _context.Sellers.AddRangeAsync(sellers);
            await _context.SaveChangesAsync();

            var sellerIds = await _context.Sellers.Select(s => s.Id).ToListAsync();
            if (!sellerIds.Any())
                throw new InvalidOperationException("Failed to seed sellers: No sellers were created in the database.");

            return sellerIds;
        }

        private async Task<List<Guid>> SeedUsersAsync()
        {
            var usedNames = new HashSet<string>();
            var faker = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Username, f =>
                {
                    string name;
                    do name = f.Internet.UserName();
                    while (name.Length > 50 || usedNames.Contains(name));
                    usedNames.Add(name);
                    return name;
                })
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.RegistrationDate, f => f.Date.Past(3))
                .RuleFor(u => u.Country, f =>
                {
                    string country;
                    do country = f.Address.Country();
                    while (country.Length > 300 || !CountryProvider.IsValidCountry(country));
                    return country;
                })
                .RuleFor(u => u.Password, f => f.Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()"))
                .RuleFor(u => u.IsBlocked, f => f.Random.Bool())
                .RuleFor(u => u.Balance, f => f.Random.Decimal(0, 1000));

            var users = faker.Generate(1000);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
            if (!userIds.Any())
                throw new InvalidOperationException("Failed to seed users: No users were created in the database.");

            return userIds;
        }

        private async Task<List<Guid>> SeedGamesAsync(Dictionary<string, Guid> genreToCategoryId, List<SteamApp> selectedApps)
        {
            var games = new List<Game>();
            var random = new Random();
            var faker = new Faker();

            foreach (var app in selectedApps)
            {
                if (string.IsNullOrWhiteSpace(app.Name))
                    continue;

                var categoryId = genreToCategoryId.Values.ElementAt(random.Next(genreToCategoryId.Count));
                var description = faker.Lorem.Paragraph();
                description = description.Length > 500 ? description.Substring(0, 500) : description;

                games.Add(new Game(
                    id: Guid.NewGuid(),
                    categoryId: categoryId,
                    title: app.Name.Length > 100 ? app.Name.Substring(0, 100) : app.Name,
                    price: (decimal)faker.Random.Double(0, 100),
                    releaseDate: faker.Date.Past(10),
                    description: description,
                    isForSale: faker.Random.Bool(),
                    originalPublisher: faker.Company.CompanyName().Length > 100 ? faker.Company.CompanyName().Substring(0, 100) : faker.Company.CompanyName()
                ));
            }

            await _context.Games.AddRangeAsync(games);
            await _context.SaveChangesAsync();

            var gameIds = await _context.Games.Select(g => g.Id).ToListAsync();
            if (!gameIds.Any())
                throw new InvalidOperationException("Failed to seed games: No games were created in the database.");

            return gameIds;
        }

        private async Task<List<Guid>> SeedOrdersAsync(List<Guid> userIds)
        {
            if (!userIds.Any())
                throw new InvalidOperationException("No user IDs available to create orders.");

            var random = new Random();
            var faker = new Faker<Order>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.UserId, f => userIds[random.Next(userIds.Count)])
                .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
                .RuleFor(o => o.Price, f => f.Random.Decimal(10, 500))
                .RuleFor(o => o.IsCompleted, f => f.Random.Bool())
                .RuleFor(o => o.IsOverdue, f => f.Random.Bool());

            var orders = faker.Generate(1000);
            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            var orderIds = await _context.Orders.Select(o => o.Id).ToListAsync();
            if (!orderIds.Any())
                throw new InvalidOperationException("Failed to seed orders: No orders were created in the database.");

            return orderIds;
        }

        private async Task SeedOrderItemsAsync(List<Guid> orderIds, List<Guid> gameIds, List<Guid> sellerIds)
        {
            if (!orderIds.Any())
                throw new InvalidOperationException("No order IDs available to create order items.");
            if (!gameIds.Any())
                throw new InvalidOperationException("No game IDs available to create order items.");
            if (!sellerIds.Any())
                throw new InvalidOperationException("No seller IDs available to create order items.");

            var random = new Random();
            var faker = new Faker<OrderItem>()
                .RuleFor(oi => oi.Id, f => Guid.NewGuid())
                .RuleFor(oi => oi.OrderId, f => orderIds[random.Next(orderIds.Count)])
                .RuleFor(oi => oi.GameId, f => gameIds[random.Next(gameIds.Count)])
                .RuleFor(oi => oi.SellerId, f => sellerIds[random.Next(sellerIds.Count)])
                .RuleFor(oi => oi.Key, f => f.Random.Bool() ? f.Random.String2(8, 50) : null);

            var orderItems = faker.Generate(1000);
            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();
        }

        private async Task SeedReviewsAsync(List<Guid> userIds, List<Guid> gameIds)
        {
            if (!userIds.Any())
                throw new InvalidOperationException("No user IDs available to create reviews.");
            if (!gameIds.Any())
                throw new InvalidOperationException("No game IDs available to create reviews.");

            var random = new Random();
            var faker = new Faker<Review>()
                .RuleFor(r => r.Id, f => Guid.NewGuid())
                .RuleFor(r => r.UserId, f => userIds[random.Next(userIds.Count)])
                .RuleFor(r => r.GameId, f => gameIds[random.Next(gameIds.Count)])
                .RuleFor(r => r.Text, f => f.Lorem.Paragraph())
                .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
                .RuleFor(r => r.CreationDate, f => f.Date.Past(1));

            var reviews = faker.Generate(1000);
            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();
        }
    }
}
