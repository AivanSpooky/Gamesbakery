using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bogus.DataSets;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.Infrastructure;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Middleware;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var dataProtectionPath = "/app/dataprotection-keys";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("Gamesbakery")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365));
builder.Services.Configure<KeyManagementOptions>(options =>
{
    options.AutoGenerateKeys = true;
});

// Configure Serilog
var logPath = "/app/logs/gamesbakery-.log";
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(_ => new IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.FromMinutes(5),
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Invalid token\"}");
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
            },
        };
    });
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
try
{
    var keyDir = new DirectoryInfo("/app/dataprotection-keys");
    if (!keyDir.Exists)
    {
        keyDir.Create();
        Log.Information("Created DataProtection keys directory: {Path}", keyDir.FullName);
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Could not create DataProtection directory");
}

builder.Services.Configure<AntiforgeryOptions>(options =>
{
    options.HeaderName = null;
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));
});

builder.Services.AddHttpContextAccessor();

// Configure DbContext with role-based connections
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
        var useRoleBasedConnections = configuration.GetValue<bool>("USE_ROLE_BASED_CONNECTIONS", true);

        // if (useRoleBasedConnections)
        // {
        //    var role = httpContextAccessor.HttpContext?.Session.GetString("Role");
        //    var username = httpContextAccessor.HttpContext?.Session.GetString("Username");
        //    switch (role)
        //    {
        //        case "Admin":
        //            connectionString = configuration.GetConnectionString("AdminConnection");
        //            break;
        //        case "Seller":
        //            connectionString = $"Server=db,1433;Database=Gamesbakery;User Id={username};Password=SellerPass123;TrustServerCertificate=True;Encrypt=False;";
        //            break;
        //        case "User":
        //            connectionString = $"Server=db,1433;Database=Gamesbakery;User Id={username};Password=UserPass123;TrustServerCertificate=True;Encrypt=False;";
        //            break;
        //        default:
        //            connectionString = configuration.GetConnectionString("GuestConnection");
        //            break;
        //    }
        // }
        // else
        // {
        connectionString = configuration.GetConnectionString("DefaultConnection");

        // }
    }

    Log.Information("Using connection string: {ConnectionString}", connectionString);
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        sqlOptions.MigrationsAssembly("Gamesbakery.DataAccess");
    });
});

builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddApiVersioning(c =>
// {
//    c.DefaultApiVersion = new ApiVersion(2);
//    c.AssumeDefaultVersionWhenUnspecified = true;
//    c.ReportApiVersions = true;
//    c.ApiVersionReader = new HeaderApiVersionReader("X-Version");
// }).AddMvc(c =>
// {
//    c.Conventions.Add(new VersionByNamespaceConvention());
// }).AddApiExplorer(c =>
// {
//    c.GroupNameFormat = "'v'V";
//    c.SubstituteApiVersionInUrl = true;
//    c.AssumeDefaultVersionWhenUnspecified = true;
// });
// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gamesbakery API V1",
        Version = "v1",
        Description = "API Version 1",
    });
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Gamesbakery API V2",
        Version = "v2",
        Description = "API Version 2",
    });
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var routeTemplate = apiDesc.RelativePath?.ToLower() ?? string.Empty;

        if (docName == "v1")
        {
            return routeTemplate.StartsWith("api/v1/") ||
                   !routeTemplate.StartsWith("api/v2/");
        }
        else if (docName == "v2")
        {
            return routeTemplate.StartsWith("api/v2/");
        }

        return false;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Register services
builder.Services.AddScoped<IAuthorizeData, AuthorizeAttribute>();
builder.Services.AddTransient<IAuthorizeData>((serviceProvider) => new AuthorizeAttribute());
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = context =>
    {
        var problems = new CustomProblemDetails(context);
        return new BadRequestObjectResult(problems);
    };
});

var clickHouseConfig = builder.Configuration.GetSection("Storage:Types:ClickHouse");
var clickHouseConnectionString = $"Compress=True;CheckCompressedHash=False;Compressor=lz4;Host={clickHouseConfig["Host"]};Port={clickHouseConfig["Port"]};Database={clickHouseConfig["Database"]};User={clickHouseConfig["User"]};Password={clickHouseConfig["Password"]}";
var storageType = builder.Configuration.GetValue<int>("SelectedStorage");
switch (storageType)
{
    case 0: // Sql
        Log.Information("Using MSSQL");
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IGameRepository, GameRepository>();
        builder.Services.AddScoped<IGiftRepository, GiftRepository>();
        builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
        builder.Services.AddScoped<ISellerRepository, SellerRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        break;
    case 1: // ClickHouse
        Log.Information("Using ClickHouse");
        break;
    default:
        throw new InvalidOperationException("Invalid storage type specified in configuration.");
}

Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine("Serilog error: " + msg));
Log.Information("Application started");

var app = builder.Build();
Log.Information("=== Application built successfully ===");

// Configure health checks
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow.ToString("O"),
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
    if (!lifetime.ApplicationStarted.IsCancellationRequested)
    {
        logger.LogWarning("Application not started yet");
        return Results.StatusCode(503);
    }

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

        logger.LogInformation("Health check /health/ready successful");
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("O"),
            checks = new[] { "liveness", "readiness" },
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
            timestamp = DateTime.UtcNow,
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Debug DB endpoint failed");
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

// Configure UI based on UIMode
var uiMode = builder.Configuration.GetValue<string>("UIMode")?.ToLower();
Log.Information("UIMode set to: {UIMode}", uiMode ?? "not set");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

try
{
    Log.Information("Starting Gamesbakery application");
    app.UseExceptionHandler("/Home/Error");

    // Middleware pipeline
    app.UseRouting();
    app.MapControllers();
    app.UseMiddleware<JwtCookieMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    // app.UseSession();
    app.UseStaticFiles();

    if (uiMode == "api" || app.Environment.IsDevelopment())
    {
        Log.Information("Configuring Swagger UI at /swagger");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Gamesbakery API";
        });

        // V1
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gamesbakery API V1");
            c.RoutePrefix = "swagger/v1";
            c.DocumentTitle = "Gamesbakery API V1";
        });

        // V2
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Gamesbakery API V2");
            c.RoutePrefix = "swagger/v2";
            c.DocumentTitle = "Gamesbakery API V2";
        });
    }

    app.MapGet("/swagger-docs", (ISwaggerProvider provider) =>
    {
        var v1Info = provider.GetSwagger("v1");
        var v2Info = provider.GetSwagger("v2");

        return Results.Ok(new
        {
            Documents = new[]
            {
            new { Name = "v1", Title = v1Info.Info.Title, Version = v1Info.Info.Version },
            new { Name = "v2", Title = v2Info.Info.Title, Version = v2Info.Info.Version },
            },
            Endpoints = new[]
            {
            "/swagger/v1/swagger.json",
            "/swagger/v2/swagger.json",
            },
        });
    });
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // app.MapRazorPages();
    var frontMode = builder.Configuration.GetValue<string>("UIMode")?.ToLower() ?? "razor";
    if (frontMode == "spa")
    {
        // SPA serving: Static files + fallback to index.html for Vue routing
        app.UseDefaultFiles();
        app.MapFallbackToFile("index.html"); // Handles Vue routes

        // if (app.Environment.IsDevelopment())
        //    app.UseSpa(spa => {
        //        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        //    });
    }
    else
    {
        app.MapRazorPages();
    }

    // Fallback route for root URL
    app.MapGet("/", async context =>
    {
        Log.Information("Root URL '/' accessed, redirecting to Home/Index");
        context.Response.Redirect("/Home/Index");
    });
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
