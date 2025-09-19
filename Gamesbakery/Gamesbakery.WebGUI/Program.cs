using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.ClickHouse;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Formatting.Json;

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

// В Program.cs, в секции настройки DbContext, добавьте проверку:
builder.Services.AddDbContext<GamesbakeryDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    string connectionString;

    if (EF.IsDesignTime)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    else
    {
        // Проверяем, нужно ли использовать ролевую аутентификацию
        var useRoleBasedConnections = configuration.GetValue<bool>("USE_ROLE_BASED_CONNECTIONS", true);

        if (useRoleBasedConnections)
        {
            // Ваша существующая логика с ролевой аутентификацией
            var role = httpContextAccessor.HttpContext?.Session.GetString("Role");
            var username = httpContextAccessor.HttpContext?.Session.GetString("Username");

            switch (role)
            {
                case "Admin":
                    connectionString = configuration.GetConnectionString("AdminConnection");
                    break;
                case "Seller":
                    connectionString = $"Server=db,1433;Database=Gamesbakery;User Id={username};Password=SellerPass123;TrustServerCertificate=True;Encrypt=False;";
                    break;
                case "User":
                    connectionString = $"Server=db,1433;Database=Gamesbakery;User Id={username};Password=UserPass123;TrustServerCertificate=True;Encrypt=False;";
                    break;
                default:
                    connectionString = configuration.GetConnectionString("GuestConnection");
                    break;
            }
        }
        else
        {
            // Для тестов используем всегда sa пользователя
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        sqlOptions.MigrationsAssembly("Gamesbakery.DataAccess");
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

Log.Information("=== Application built successfully ===");
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow.ToString("O"),
    version = "1.0.0"
}));
app.MapGet("/health/live", (IHostApplicationLifetime lifetime) =>
{
    return lifetime.ApplicationStarted.IsCancellationRequested
        ? Results.Ok(new { status = "healthy", check = "liveness" })
        : Results.StatusCode(503);
});

app.MapGet("/health/ready", async (IHostApplicationLifetime lifetime, ILogger<Program> logger) =>
{
    logger.LogInformation("Health check /health/ready called");

    // Проверяем, что приложение запущено
    if (!lifetime.ApplicationStarted.IsCancellationRequested)
    {
        logger.LogWarning("Application not started yet");
        return Results.StatusCode(503);
    }

    // Здесь можно добавить дополнительные проверки (база данных, внешние сервисы)
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GamesbakeryDbContext>();

        logger.LogInformation("Testing database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (!canConnect)
        {
            logger.LogError("Database connection failed");
            return Results.StatusCode(503);
        }

        // Проверяем, что база данных существует
        var dbExists = await dbContext.Database.CanConnectAsync();
        if (!dbExists)
        {
            logger.LogError("Database 'Gamesbakery' does not exist or inaccessible");
            return Results.StatusCode(503);
        }

        logger.LogInformation("Health check /health/ready successful");
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("O"),
            checks = new[] { "liveness", "readiness" }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Health check /health/ready failed");
        return Results.StatusCode(503);
    }
});
app.MapGet("/debug/db", async (ILogger<Program> logger) =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GamesbakeryDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();

        return Results.Ok(new
        {
            connected = canConnect,
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? "Not set",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Debug DB endpoint failed");
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});



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