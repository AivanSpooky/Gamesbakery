using Gamesbakery.Core.Repositories;
using Gamesbakery.Core;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.BusinessLogic.Schedulers;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Gamesbakery.BusinessLogic;

namespace Gamesbakery.Infrastructure
{
    public static class DependencySetup
    {
        public static IServiceProvider ConfigureServices(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDbContext<GamesbakeryDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            services.AddScoped<IOrderStatusScheduler, OrderStatusScheduler>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISellerService, SellerService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services.BuildServiceProvider();
        }
    }
}