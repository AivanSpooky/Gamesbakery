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
using Microsoft.AspNetCore.Identity;
using Gamesbakery.DataAccess.ClickHouse;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logPath = "/app/logs/gamesbakery-.log";
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning) // Suppress EF Core logs
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

// Add DbContext for SQL Server
builder.Services.AddDbContext<GamesbakeryDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    string connectionString;
    if (EF.IsDesignTime)
    {
        // Use DefaultConnection for design-time operations (migrations)
        connectionString = configuration.GetConnectionString("sa");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Error("DefaultConnection string is null for design-time operations");
            throw new InvalidOperationException("DefaultConnection string is not configured.");
        }
    }
    else
    {
        // Runtime: Use role-based connection strings
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
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure DbContext for role: {Role}, username: {Username}", role, username);
            throw;
        }
    }

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.MigrationsAssembly("Gamesbakery.DataAccess"); // Ensure migrations are in DataAccess
    });
});

var clickHouseConfig = builder.Configuration.GetSection("Storage:Types:ClickHouse");
var clickHouseConnectionString = $"Compress=True;CheckCompressedHash=False;Compressor=lz4;Host={clickHouseConfig["Host"]};Port={clickHouseConfig["Port"]};Database={clickHouseConfig["Database"]};User={clickHouseConfig["User"]};Password={clickHouseConfig["Password"]}";

// Register services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGiftService, GiftService>();

var storageType = builder.Configuration.GetValue<int>("SelectedStorage");
switch (storageType)
{
    case 0: // Sql
        Log.Information("Using MSSQL");
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IGameRepository, GameRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ISellerRepository, SellerRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
        builder.Services.AddScoped<IGiftRepository, GiftRepository>();
        break;
    case 1: // ClickHouse
        Log.Information("Using ClickHouse");
        builder.Services.AddScoped<IUserRepository>(sp => new ClickHouseUserRepository(clickHouseConnectionString));
        builder.Services.AddScoped<IGameRepository>(sp => new ClickHouseGameRepository(clickHouseConnectionString));
        builder.Services.AddScoped<ICategoryRepository>(sp => new ClickHouseCategoryRepository(clickHouseConnectionString));
        builder.Services.AddScoped<ISellerRepository>(sp => new ClickHouseSellerRepository(clickHouseConnectionString));
        builder.Services.AddScoped<IOrderRepository>(sp => new ClickHouseOrderRepository(clickHouseConnectionString));
        builder.Services.AddScoped<IOrderItemRepository>(sp => new ClickHouseOrderItemRepository(clickHouseConnectionString));
        builder.Services.AddScoped<IReviewRepository>(sp => new ClickHouseReviewRepository(clickHouseConnectionString));
        builder.Services.AddScoped<IGiftRepository>(sp => new ClickHouseGiftRepository(clickHouseConnectionString));
        break;
    default:
        throw new InvalidOperationException("Invalid storage type specified in configuration.");
}

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