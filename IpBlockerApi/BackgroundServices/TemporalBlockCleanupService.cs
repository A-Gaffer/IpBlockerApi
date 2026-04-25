using IpBlockerApi.interfaces;

namespace IpBlockerApi.BackgroundServices
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TemporalBlockCleanupService> _logger;
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public TemporalBlockCleanupService(
            IServiceProvider services,
            ILogger<TemporalBlockCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporal block cleanup service started.");

            // Keep running until the app shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                CleanExpiredBlocks();
            }
        }

        private void CleanExpiredBlocks()
        {
            // ITemporalBlockRepository is registered as Singleton, so we can resolve it directly
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITemporalBlockRepository>();

            var expired = repo.GetExpired().ToList();

            if (!expired.Any())
            {
                _logger.LogDebug("Cleanup: no expired temporal blocks found.");
                return;
            }

            foreach (var block in expired)
            {
                repo.TryRemove(block.CountryCode);
                _logger.LogInformation(
                    "Temporal block expired and removed for country: {Code} (was until {Expiry})",
                    block.CountryCode, block.ExpiresAt);
            }
        }
    }
    }
