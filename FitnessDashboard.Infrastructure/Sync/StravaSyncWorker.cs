using FitnessDashboard.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FitnessDashboard.Infrastructure.Sync;

public class StravaSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StravaSyncWorker> _logger;

    public StravaSyncWorker(IServiceProvider serviceProvider, ILogger<StravaSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Strava Sync Worker running at: {time}", DateTimeOffset.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();

                try
                {
                    await syncService.SyncAllAthletesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync athletes.");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
