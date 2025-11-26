using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Gamesbakery.Infrastructure
{
    public class DatabaseHealthCheckService : BackgroundService
    {
        private readonly IDatabaseConnectionChecker _dbChecker;
        private bool _isDatabaseConnected;

        public DatabaseHealthCheckService(IDatabaseConnectionChecker dbChecker)
        {
            _dbChecker = dbChecker;
            _isDatabaseConnected = true;
        }

        public bool IsDatabaseConnected => _isDatabaseConnected;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool canConnect = await _dbChecker.CanConnectAsync();
                    if (canConnect && !_isDatabaseConnected)
                    {
                        _isDatabaseConnected = true;
                        // Можно логировать или уведомлять
                    }
                    else if (!canConnect && _isDatabaseConnected)
                    {
                        _isDatabaseConnected = false;
                        // Можно логировать или уведомлять
                    }
                }
                catch
                {
                    _isDatabaseConnected = false;
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
