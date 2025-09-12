using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class SqlServerDbContextFixture : IDbContextFixture, IDisposable
{
    public GamesbakeryDbContext Context { get; private set; }
    private readonly string _databaseName;

    public SqlServerDbContextFixture()
    {
        _databaseName = $"GamesbakeryTest_{Guid.NewGuid():N}";

        var connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
            ?? throw new InvalidOperationException("TEST_DB_CONNECTION is not set.");

        // Сначала создаем базу через master connection
        using (var masterConnection = new SqlConnection(connectionString.Replace("GamesbakeryTest", "master")))
        {
            masterConnection.Open();
            var createDbCommand = new SqlCommand($"CREATE DATABASE [{_databaseName}]", masterConnection);
            createDbCommand.ExecuteNonQuery();
        }

        // Затем подключаемся к созданной базе
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = _databaseName
        };

        var options = new DbContextOptionsBuilder<GamesbakeryDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options;

        Context = new GamesbakeryDbContext(options);

        // Применяем миграции
        Context.Database.Migrate();

        // Создаем функции и представления
        InitializeDatabase(builder.ConnectionString);
    }

    private void InitializeDatabase(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Создаем функции
        CreateFunctions(connection);

        // Создаем представления
        CreateViews(connection);
    }

    private void CreateFunctions(SqlConnection connection)
    {
        var functions = new Dictionary<string, string>
        {
            ["fn_GetUserTotalSpent"] = @"
            CREATE FUNCTION fn_GetUserTotalSpent (@UserID UNIQUEIDENTIFIER)
            RETURNS DECIMAL(12,2)
            AS
            BEGIN
                DECLARE @TotalSpent DECIMAL(12,2);
                SELECT @TotalSpent = SUM(o.TotalPrice)
                FROM Orders o
                WHERE o.UserID = @UserID;
                RETURN ISNULL(@TotalSpent, 0.00);
            END",

            ["fn_GetGameAverageRating"] = @"
            CREATE FUNCTION fn_GetGameAverageRating (@GameID UNIQUEIDENTIFIER)
            RETURNS DECIMAL(3,2)
            AS
            BEGIN
                DECLARE @AverageRating DECIMAL(3,2);
                SELECT @AverageRating = AVG(CAST(r.StarRating AS DECIMAL(3,2)))
                FROM Reviews r
                WHERE r.GameID = @GameID;
                RETURN ISNULL(@AverageRating, 0.00);
            END"
        };

        foreach (var function in functions)
        {
            var checkExists = $"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = '{function.Key}' AND type = 'FN')";
            var createFunction = $"{checkExists} EXEC('{function.Value.Replace("'", "''")}')";

            using var command = new SqlCommand(createFunction, connection);
            command.ExecuteNonQuery();
        }
    }

    private void CreateViews(SqlConnection connection)
    {
        var views = new Dictionary<string, string>
        {
            ["UserProfile"] = @"
            CREATE VIEW UserProfile AS
            SELECT 
                UserID as Id, 
                Name as Username, 
                Email, 
                RegistrationDate, 
                Country, 
                Password, 
                IsBlocked, 
                Balance
            FROM Users",

            ["UserOrders"] = @"
            CREATE VIEW UserOrders AS
            SELECT 
                OrderID as Id,
                UserID as UserId, 
                OrderDate, 
                TotalPrice as Price, 
                IsCompleted, 
                IsOverdue
            FROM Orders"
            // Добавьте остальные представления
        };

        foreach (var view in views)
        {
            var checkExists = $"IF NOT EXISTS (SELECT * FROM sys.views WHERE name = '{view.Key}')";
            var createView = $"{checkExists} EXEC('{view.Value.Replace("'", "''")}')";

            using var command = new SqlCommand(createView, connection);
            command.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        var connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
            ?? throw new InvalidOperationException("TEST_DB_CONNECTION is not set.");

        using (var masterConnection = new SqlConnection(connectionString.Replace("GamesbakeryTest", "master")))
        {
            masterConnection.Open();

            // Сначала переводим базу в SINGLE_USER mode чтобы закрыть все соединения
            var setSingleUser = $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            using var singleUserCmd = new SqlCommand(setSingleUser, masterConnection);
            singleUserCmd.ExecuteNonQuery();

            // Затем удаляем базу
            var deleteDbCommand = new SqlCommand($"DROP DATABASE [{_databaseName}]", masterConnection);
            deleteDbCommand.ExecuteNonQuery();
        }

        Context.Dispose();
    }
}