using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.Extensions.Configuration;
using Allure.Xunit.Attributes;

namespace Gamesbakery.E2E.Tests
{
    [Collection("E2ETests")]
    public class DemoScenarioE2E
    {
        private readonly HttpClient _client = new HttpClient { BaseAddress = new Uri("http://web:80/api/") }; // Use web service name
        private static readonly ILogger _logger = Log.ForContext<DemoScenarioE2E>();

        public DemoScenarioE2E()
        {
            // Initialize Serilog from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set base path to current directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        [AllureXunit]
        [Trait("Category", "E2E")]
        public async Task DemoScenario_RegisterAddGameCreateOrder_Success()
        {
            _logger.Information("Starting E2E demo scenario");

            // Arrange & Act: Register user
            var registerContent = new StringContent("{\"username\": \"test\", \"email\": \"test@example.com\", \"password\": \"pass123\", \"country\": \"US\"}", Encoding.UTF8, "application/json");
            _logger.Information("Sending register request");
            var registerResponse = await _client.PostAsync("users/register", registerContent);
            _logger.Information("Register response: {StatusCode}", registerResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
            var userId = await registerResponse.Content.ReadAsStringAsync();

            // Act: Add game as admin
            var addGameContent = new StringContent("{\"categoryId\": \"550e8400-e29b-41d4-a716-446655440000\", \"title\": \"Test Game\", \"price\": 59.99, \"releaseDate\": \"2025-09-11\", \"description\": \"Desc\", \"originalPublisher\": \"Pub\"}", Encoding.UTF8, "application/json");
            _logger.Information("Sending add game request");
            var addGameResponse = await _client.PostAsync("games", addGameContent);
            _logger.Information("Add game response: {StatusCode}", addGameResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, addGameResponse.StatusCode);
            var gameId = await addGameResponse.Content.ReadAsStringAsync();

            // Act: Create order
            var createOrderContent = new StringContent("{\"userId\": \"" + userId + "\", \"gameIds\": [\"" + gameId + "\"]}", Encoding.UTF8, "application/json");
            _logger.Information("Sending create order request");
            var createOrderResponse = await _client.PostAsync("orders", createOrderContent);
            _logger.Information("Create order response: {StatusCode}", createOrderResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, createOrderResponse.StatusCode);

            // Assert: Verify order created
            _logger.Information("Verifying order for user {UserId}", userId);
            var getOrderResponse = await _client.GetAsync("orders/user/" + userId);
            Assert.Equal(HttpStatusCode.OK, getOrderResponse.StatusCode);
            var orders = await getOrderResponse.Content.ReadAsStringAsync();
            Assert.Contains(gameId, orders);
            _logger.Information("E2E demo scenario completed successfully");
        }
    }
}