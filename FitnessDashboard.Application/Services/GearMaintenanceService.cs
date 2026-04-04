using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Application.Services;

public class GearMaintenanceService : IGearMaintenanceService
{
    private readonly IAppDbContext _context;

    public GearMaintenanceService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task UpdateGearMaintenanceStatusAsync(string gearId)
    {
        var gear = await _context.Gears
            .Include(g => g.MaintenanceTasks)
            .FirstOrDefaultAsync(g => g.Id == gearId);

        if (gear == null) return;

        foreach (var task in gear.MaintenanceTasks)
        {
            task.Status = CalculateStatus(gear.TotalDistance, task.LastServiceMeters, task.IntervalMeters);
        }

        await _context.SaveChangesAsync();
    }

    public MaintenanceStatus CalculateStatus(double currentDistance, double lastServiceDistance, double interval)
    {
        var distanceSinceService = currentDistance - lastServiceDistance;
        var ratio = distanceSinceService / interval;

        if (ratio >= 1.0) return MaintenanceStatus.Critical;
        if (ratio >= 0.8) return MaintenanceStatus.ServiceRequired;
        
        return MaintenanceStatus.Healthy;
    }
}
