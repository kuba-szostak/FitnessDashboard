using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessDashboard.Application.Services;

public class SyncService : ISyncService
{
    private readonly IAppDbContext _context;
    private readonly IStravaService _stravaService;
    private readonly IWeatherService _weatherService;
    private readonly IGearMaintenanceService _maintenanceService;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IAppDbContext context,
        IStravaService stravaService,
        IWeatherService weatherService,
        IGearMaintenanceService maintenanceService,
        ILogger<SyncService> _logger)
    {
        _context = context;
        _stravaService = stravaService;
        _weatherService = weatherService;
        _maintenanceService = maintenanceService;
        this._logger = _logger;
    }

    public async Task SyncAthleteDataAsync(Athlete athlete, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting sync for athlete {AthleteId} ({FirstName} {LastName})", 
            athlete.Id, athlete.FirstName, athlete.LastName);

        try
        {
            // 1. Sync Gear first (new bikes/shoes)
            var gears = await _stravaService.GetAthleteGearsAsync(athlete);
            foreach (var gear in gears)
            {
                var existingGear = await _context.Gears.FirstOrDefaultAsync(g => g.Id == gear.Id, ct);
                if (existingGear == null)
                {
                    _context.Gears.Add(gear);
                }
                else
                {
                    existingGear.Name = gear.Name;
                    // Note: We don't overwrite distance here, we let activity sync handle it 
                    // to account for weather factors in total distance calculation.
                }
            }
            await _context.SaveChangesAsync(ct);

            // 2. Sync Activities
            var lastActivityDate = await _context.Activities
                .Where(a => a.AthleteId == athlete.Id)
                .OrderByDescending(a => a.StartDate)
                .Select(a => (DateTime?)a.StartDate)
                .FirstOrDefaultAsync(ct);

            var newActivities = await _stravaService.GetActivitiesAsync(athlete, lastActivityDate);

            foreach (var activity in newActivities)
            {
                if (!await _context.Activities.AnyAsync(a => a.Id == activity.Id, ct))
                {
                    _context.Activities.Add(activity);

                    var (condition, multiplier) =
                        await _weatherService.GetWearFactorAsync(activity.StartDate, activity.StartLatitude, activity.StartLongitude);

                    activity.WeatherCondition = condition;
                    activity.WearMultiplier = multiplier;

                    if (!string.IsNullOrEmpty(activity.GearId))
                    {
                        var gear = await _context.Gears.FirstOrDefaultAsync(g => g.Id == activity.GearId, ct);
                        if (gear != null)
                        {
                            gear.TotalDistance += (activity.Distance * multiplier);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(ct);

            // 3. Update maintenance statuses
            var gearIds = newActivities.Where(a => !string.IsNullOrEmpty(a.GearId)).Select(a => a.GearId!).Distinct();
            foreach (var gearId in gearIds)
            {
                await _maintenanceService.UpdateGearMaintenanceStatusAsync(gearId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing data for athlete {AthleteId}", athlete.Id);
            throw; // Re-throw to allow caller to handle if needed
        }
    }

    public async Task SyncAllAthletesAsync(CancellationToken ct = default)
    {
        var athletes = await _context.Athletes.ToListAsync(ct);
        foreach (var athlete in athletes)
        {
            await SyncAthleteDataAsync(athlete, ct);
        }
    }
}
