using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var stravaService = scope.ServiceProvider.GetRequiredService<IStravaService>();
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                var maintenanceService = scope.ServiceProvider.GetRequiredService<IGearMaintenanceService>();

                List<Domain.Entities.Athlete> athletes;
                try
                {
                    athletes = await context.Athletes.ToListAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch athletes. Database might not be ready.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                foreach (var athlete in athletes)
                {
                    try
                    {
                        // 1. Sync Gear first (new bikes/shoes)
                        var gears = await stravaService.GetAthleteGearsAsync(athlete);
                        foreach (var gear in gears)
                        {
                            var existingGear = await context.Gears.FirstOrDefaultAsync(g => g.Id == gear.Id, stoppingToken);
                            if (existingGear == null)
                            {
                                context.Gears.Add(gear);
                            }
                            else
                            {
                                existingGear.Name = gear.Name;
                                // We don't overwrite distance here, we let activity sync handle it to account for weather factor
                                // unless it's a new athlete with no activities yet.
                            }
                        }
                        await context.SaveChangesAsync(stoppingToken);

                        // 2. Sync Activities
                        var lastActivityDate = await context.Activities
                            .Where(a => a.AthleteId == athlete.Id)
                            .OrderByDescending(a => a.StartDate)
                            .Select(a => (DateTime?)a.StartDate)
                            .FirstOrDefaultAsync(stoppingToken);

                        var newActivities = await stravaService.GetActivitiesAsync(athlete, lastActivityDate);

                        foreach (var activity in newActivities)
                        {
                            if (!await context.Activities.AnyAsync(a => a.Id == activity.Id, stoppingToken))
                            {
                                context.Activities.Add(activity);

                                var (condition, multiplier) =
                                    await weatherService.GetWearFactorAsync(activity.StartDate, activity.StartLatitude, activity.StartLongitude);

                                activity.WeatherCondition = condition;
                                activity.WearMultiplier = multiplier;

                                if (!string.IsNullOrEmpty(activity.GearId))
                                {
                                    var gear = await context.Gears.FirstOrDefaultAsync(g => g.Id == activity.GearId, stoppingToken);
                                    if (gear != null)
                                    {
                                        gear.TotalDistance += (activity.Distance * multiplier);
                                    }
                                }
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        // Update maintenance statuses
                        var gearIds = newActivities.Where(a => !string.IsNullOrEmpty(a.GearId)).Select(a => a.GearId!).Distinct();
                        foreach (var gearId in gearIds)
                        {
                            await maintenanceService.UpdateGearMaintenanceStatusAsync(gearId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing data for athlete {AthleteId}", athlete.Id);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
