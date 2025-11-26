using Gamesbakery.DataAccess;
using Gamesbakery.DataSeeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gamesbakery.DataSeeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Настройка конфигурации
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Упрощённый путь
                    .Build();

                // Проверка строки подключения
                string connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
                }

                // Настройка DI
                var services = new ServiceCollection();
                services.AddDbContext<GamesbakeryDbContext>(options =>
                    options.UseSqlServer(connectionString));

                var serviceProvider = services.BuildServiceProvider();

                // Запуск сидера
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<GamesbakeryDbContext>();
                    var seeder = new DataSeeder(context);
                    await seeder.SeedAsync();
                }

                Console.WriteLine("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during seeding: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}
