using Gamesbakery.Core;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.EntityFrameworkCore;
using Gamesbakery.Infrastructure;

using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logPath = "/app/logs/gamesbakery-.log";
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Load Serilog settings from appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/Error";
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

// Register services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddDbContext<GamesbakeryDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    string connectionString;
    var role = httpContextAccessor.HttpContext?.Session.GetString("Role");
    var username = httpContextAccessor.HttpContext?.Session.GetString("Username");

    try
    {
        switch (role)
        {
            case "Admin":
                connectionString = configuration.GetConnectionString("AdminConnection");
                break;
            case "Seller":
                if (string.IsNullOrEmpty(username))
                    throw new InvalidOperationException("Username is null for Seller role");
                connectionString = $"Server=db;Database=Gamesbakery;User Id={username};Password=SellerPass123;TrustServerCertificate=True;";
                break;
            case "User":
                if (string.IsNullOrEmpty(username))
                    throw new InvalidOperationException("Username is null for User role");
                connectionString = $"Server=db;Database=Gamesbakery;User Id={username};Password=UserPass123;TrustServerCertificate=True;";
                break;
            default:
                connectionString = configuration.GetConnectionString("GuestConnection");
                break;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Error("Connection string is null for role: {Role}, username: {Username}", role, username);
            throw new InvalidOperationException("Connection string is not configured.");
        }

        Log.Information("Using connection string for role: {Role}, username: {Username}", role, username);
        options.UseSqlServer(connectionString);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to configure DbContext for role: {Role}, username: {Username}", role, username);
        throw;
    }
});

Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine("Serilog error: " + msg));
Log.Information("Application started");

var app = builder.Build();

try
{
    Log.Information("Starting Gamesbakery application");
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}