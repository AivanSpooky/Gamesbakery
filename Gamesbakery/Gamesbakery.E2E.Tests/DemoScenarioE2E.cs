using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Allure.Xunit.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;
using System.Linq;
using System.IO;

namespace Gamesbakery.E2E.Tests
{
    [Collection("E2ETests")]
    [AllureTag("E2E")]
    public class DemoScenarioE2E
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private static readonly ILogger _logger = Log.ForContext<DemoScenarioE2E>();
        private readonly string _testUsername = "I";
        private readonly string _testPassword = "I";

        public DemoScenarioE2E()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            _baseUrl = _configuration.GetValue<string>("E2E:BaseUrl")?.TrimEnd('/') ?? "http://web:80";
            _client = new HttpClient { BaseAddress = new Uri(_baseUrl + "/") };
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            var envConnectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION");
            _connectionString = !string.IsNullOrEmpty(envConnectionString) ?
                envConnectionString.Replace("Database=GamesbakeryTest", "Database=Gamesbakery") :
                _configuration.GetValue<string>("TEST_DB_CONNECTION")?.Replace("Database=GamesbakeryTest", "Database=Gamesbakery") ??
                "Server=db,1433;Database=Gamesbakery;User Id=sa;Password=YourStrong@Pass;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60;";

            _logger.Information("E2E Test Config: BaseUrl={BaseUrl}, User={User}", _baseUrl, _testUsername);
        }

        [AllureXunit(DisplayName = "E2E: Login and Balance Top-Up")]
        [Trait("Category", "E2E")]
        public async Task LoginAndBalanceTopUp()
        {
            _logger.Information("Starting E2E test: Login and Balance Top-Up for user {User}", _testUsername);

            await WaitForServiceReadyAsync();
            await WaitForDatabaseReadyAsync();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            // STEP 1: Verify unauthenticated state
            _logger.Information("STEP 1: Checking unauthenticated state");
            var homeResponse = await _client.GetAsync("/");
            Assert.True(homeResponse.IsSuccessStatusCode);
            var homeContent = await homeResponse.Content.ReadAsStringAsync();
            Assert.Contains("Gamesbakery", homeContent);
            Assert.Contains("Login", homeContent);
            var canAccessBefore = await CanAccessProfileAsync();
            Assert.False(canAccessBefore, "Should not access profile before login");

            // STEP 2: Perform login
            _logger.Information("STEP 2: Performing login");
            var loginSuccess = await PerformLoginAsync();
            Assert.True(loginSuccess, "Login should succeed");

            // STEP 3: Verify authentication
            _logger.Information("STEP 3: Verifying authentication");
            var isAuthenticated = await CanAccessProfileAsync();
            Assert.True(isAuthenticated, "User should be authenticated after login");

            // STEP 4: Get profile content for analysis
            _logger.Information("STEP 4: Getting profile content");
            var profileResponse = await _client.GetAsync("User/Profile");
            Assert.True(profileResponse.IsSuccessStatusCode);
            var profileContent = await profileResponse.Content.ReadAsStringAsync();
            _logger.Information("Profile content length: {Length}", profileContent.Length);
            _logger.Debug("Profile HTML preview: {Preview}", profileContent.Substring(0, Math.Min(1000, profileContent.Length)));

            // STEP 5: Perform balance top-up
            _logger.Information("STEP 5: Performing balance top-up");
            var topUpSuccess = await PerformBalanceTopUpAsync(150.75m);
            Assert.True(topUpSuccess, "Balance top-up should succeed");

            // STEP 6: Verify session still valid
            _logger.Information("STEP 6: Verifying session after balance update");
            var sessionStillValid = await CanAccessProfileAsync();
            Assert.True(sessionStillValid, "Session should remain valid after balance update");

            _logger.Information("E2E Test PASSED: Login and Balance Top-Up successful for user {User}", _testUsername);
            Console.WriteLine("E2E Test PASSED: Login and Balance Top-Up successful!");

            // Rollback transaction to clean up test data
            _logger.Information("Rolling back transaction");
            await transaction.RollbackAsync();
            _logger.Information("Transaction rolled back successfully");
        }

        private async Task<bool> PerformLoginAsync()
        {
            // GET login form
            var loginFormResponse = await _client.GetAsync("Account/Login");
            Assert.True(loginFormResponse.IsSuccessStatusCode);
            var formContent = await loginFormResponse.Content.ReadAsStringAsync();

            // Extract token
            var token = ExtractAntiForgeryToken(formContent);
            Assert.NotNull(token);

            // POST login
            var loginData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Username", _testUsername),
                new KeyValuePair<string, string>("Password", _testPassword),
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("RememberMe", "false")
            };

            var loginContent = new FormUrlEncodedContent(loginData);
            var loginResponse = await _client.PostAsync("Account/Login", loginContent);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

            _logger.Information("Login response: Status={StatusCode}, Length={Length}",
                loginResponse.StatusCode, loginResponseContent.Length);

            // Check success criteria
            bool hasErrors = loginResponseContent.Contains("Invalid", StringComparison.OrdinalIgnoreCase) ||
                           loginResponseContent.Contains("неверное") ||
                           loginResponseContent.Contains("error", StringComparison.OrdinalIgnoreCase);
            bool isLoginForm = loginResponseContent.Contains("name=\"Username\"") &&
                             loginResponseContent.Contains("name=\"Password\"");

            if (!hasErrors && !isLoginForm)
            {
                _logger.Information("Login successful - no errors, not returned to login form");
                return true;
            }
            else
            {
                _logger.Error("Login failed: hasErrors={HasErrors}, isLoginForm={IsLoginForm}", hasErrors, isLoginForm);
                _logger.Error("Response preview: {Preview}", loginResponseContent.Substring(0, Math.Min(500, loginResponseContent.Length)));
                return false;
            }
        }

        private async Task<bool> CanAccessProfileAsync()
        {
            var profileResponse = await _client.GetAsync("User/Profile");
            if (profileResponse.IsSuccessStatusCode)
            {
                var profileContent = await profileResponse.Content.ReadAsStringAsync();
                bool hasUserData = profileContent.Contains(_testUsername) ||
                                 profileContent.Contains("Profile") ||
                                 profileContent.Contains("Balance") ||
                                 profileContent.Contains("balance", StringComparison.OrdinalIgnoreCase);
                _logger.Information("Profile access: Status={Status}, HasUserData={HasUserData}",
                    profileResponse.StatusCode, hasUserData);
                return hasUserData;
            }
            else
            {
                _logger.Information("Profile access: Status={Status}, ContentLength={Length}",
                    profileResponse.StatusCode,
                    profileResponse.Content.Headers.ContentLength);
                return false;
            }
        }

        private async Task<bool> PerformBalanceTopUpAsync(decimal amount)
        {
            // GET balance form
            var balanceFormResponse = await _client.GetAsync("User/UpdateBalance");
            Assert.True(balanceFormResponse.IsSuccessStatusCode);
            var balanceFormContent = await balanceFormResponse.Content.ReadAsStringAsync();
            var balanceToken = ExtractAntiForgeryToken(balanceFormContent);
            Assert.NotNull(balanceToken);

            // POST balance update
            var balanceData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("newBalance", amount.ToString("F2")),
                new KeyValuePair<string, string>("__RequestVerificationToken", balanceToken)
            };

            var balanceContent = new FormUrlEncodedContent(balanceData);
            var balanceUpdateResponse = await _client.PostAsync("User/UpdateBalance", balanceContent);
            _logger.Information("Balance update: Status={StatusCode}, Amount={Amount}",
                balanceUpdateResponse.StatusCode, amount);

            return balanceUpdateResponse.IsSuccessStatusCode ||
                   balanceUpdateResponse.StatusCode == HttpStatusCode.Redirect;
        }

        private string? ExtractAntiForgeryToken(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;

            var patterns = new[]
            {
                @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
                @"name=""__RequestVerificationToken""\s+value=""([^""]+)""",
                @"input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)"""
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var token = match.Groups[1].Value;
                    _logger.Debug("AntiForgeryToken extracted: {Length} chars", token.Length);
                    return token;
                }
            }

            _logger.Warning("AntiForgeryToken not found in HTML. First 500 chars: {Preview}",
                html.Substring(0, Math.Min(500, html.Length)));
            return null;
        }

        private async Task WaitForServiceReadyAsync()
        {
            var timeoutSeconds = _configuration.GetValue<int>("E2E:HealthCheckTimeoutSeconds", 60);
            _logger.Information("Waiting for web service (timeout: {Timeout}s)", timeoutSeconds);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(timeoutSeconds))
            {
                try
                {
                    var response = await _client.GetAsync("health");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.Information("Web service ready");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug("Health check failed: {Error}", ex.Message);
                }

                await Task.Delay(2000);
            }

            Assert.Fail($"Web service not ready within {timeoutSeconds}s");
        }

        private async Task WaitForDatabaseReadyAsync()
        {
            const int maxAttempts = 30;
            _logger.Information("Waiting for database");

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    var connectionString = "Server=db,1433;Database=Gamesbakery;User Id=sa;Password=YourStrong@Pass;TrustServerCertificate=True;Encrypt=False;";
                    await using var connection = new SqlConnection(connectionString);
                    await connection.OpenAsync();
                    await using var command = new SqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync();
                    _logger.Information("Database ready after {Attempt} attempts", attempt + 1);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Debug("Database attempt {Attempt}/{Max}: {Error}", attempt + 1, maxAttempts, ex.Message);
                    await Task.Delay(1000);
                }
            }

            Assert.Fail("Database not ready within timeout");
        }
    }
}
