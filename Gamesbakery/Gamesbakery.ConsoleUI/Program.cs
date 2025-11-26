using Gamesbakery.Infrastructure;
using Gamesbakery.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gamesbakery.BusinessLogic.Schedulers;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.DataAccess;

namespace Gamesbakery.ConsoleUI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var baseConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Запрашиваем учетные данные для подключения к базе данных
            Console.WriteLine("=== Database Connection ===");
            Console.Write("Enter Database Username (): ");
            var dbUsername = Console.ReadLine();
            Console.Write("Enter Database Password: ");
            var dbPassword = Console.ReadLine();

            // Учетные данные для гостя
            const string guestUsername = "GuestUser";
            const string guestPassword = "GuestPass123";

            // Переменные для аутентификации
            string authUsername = dbUsername;
            string authPassword = dbPassword;
            IServiceProvider serviceProvider = null;

            // Пробуем подключиться с введенными учетными данными
            try
            {
                Console.WriteLine($"Attempting to connect with username: {dbUsername}");
                var connectionString = ModifyConnectionString(baseConnectionString, dbUsername, dbPassword);
                serviceProvider = DependencySetup.ConfigureServices(connectionString);
                Console.WriteLine("Database connection successful with provided credentials.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to the database with provided credentials: {ex.Message}");
                Console.WriteLine("Connecting as GuestUser...");

                // Пробуем подключиться с гостевыми учетными данными
                try
                {
                    var guestConnectionString = ModifyConnectionString(baseConnectionString, guestUsername, guestPassword);
                    serviceProvider = DependencySetup.ConfigureServices(guestConnectionString);

                    // Если подключение удалось, используем гостевые учетные данные для аутентификации
                    authUsername = guestUsername;
                    authPassword = guestPassword;
                    Console.WriteLine("Successfully connected as GuestUser.");
                }
                catch (Exception guestEx)
                {
                    Console.WriteLine($"Failed to connect as GuestUser: {guestEx.Message}");
                    Console.WriteLine("Exiting the application...");
                    return;
                }
            }

            //// Создаем ConsoleUI, передавая зависимости и учетные данные для аутентификации
            //var consoleUI = new ConsoleUI(
            //    serviceProvider.GetService<IUserService>(),
            //    serviceProvider.GetService<IGameService>(),
            //    serviceProvider.GetService<IOrderService>(),
            //    serviceProvider.GetService<IReviewService>(),
            //    serviceProvider.GetService<IOrderStatusScheduler>(),
            //    serviceProvider.GetService<IAuthenticationService>(),
            //    serviceProvider.GetService<ISellerService>(),
            //    serviceProvider.GetService<GamesbakeryDbContext>(), // Добавляем контекст
            //    authUsername,
            //    authPassword
            //);

            //// Запуск приложения
            //await consoleUI.RunAsync();
        }

        private static string ModifyConnectionString(string baseConnectionString, string username, string password)
        {
            var modifiedConnectionString = baseConnectionString
                .Replace("Trusted_Connection=True;", "")
                .TrimEnd(';');

            modifiedConnectionString += $";User Id={username};Password={password};";
            return modifiedConnectionString;
        }
    }
}
